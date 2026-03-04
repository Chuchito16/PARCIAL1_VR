using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cada jugador que se une recibe su propio prefab según el orden de entrada.
/// Los prefabs reales NO necesitan tener PlayerInput — se gestiona aquí.
/// </summary>
public class GameRoleManager : MonoBehaviour
{
    public static GameRoleManager Instance { get; private set; }

    [Header("Prefabs (uno por jugador, en orden de entrada)")]
    [Tooltip("Index 0 = Cazador  ·  Index 1-3 = Sobrevivientes")]
    public GameObject[] playerPrefabs = new GameObject[4];

    [Header("Spawn Points")]
    public Transform hunterSpawnPoint;
    public Transform[] survivorSpawnPoints = new Transform[3];

    [Header("Límite")]
    public int maxJugadores = 4;

    // ── Estado interno ────────────────────────────────────────────────────────
    private readonly List<PlayerController> jugadores = new();

    // ── Singleton ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── PlayerInputManager.onPlayerJoined apunta aquí ─────────────────────────
    public void OnPlayerJoined(PlayerInput inputTemporal)
    {
        int playerIndex = jugadores.Count;

        if (playerIndex >= maxJugadores)
        {
            Debug.LogWarning("[GameRoleManager] Límite de jugadores alcanzado.");
            Destroy(inputTemporal.gameObject);
            return;
        }

        if (playerIndex >= playerPrefabs.Length || playerPrefabs[playerIndex] == null)
        {
            Debug.LogError($"[GameRoleManager] No hay prefab para el jugador {playerIndex + 1}.");
            Destroy(inputTemporal.gameObject);
            return;
        }

        // ── Guardar devices ANTES de destruir el objeto temporal ──────────────
        InputDevice[] devices = inputTemporal.devices.ToArray();
        bool usaTeclado = false;
        foreach (var d in devices)
            if (d is Keyboard) { usaTeclado = true; break; }

        // ── Destruir prefab trampolín ──────────────────────────────────────────
        Destroy(inputTemporal.gameObject);

        // ── Calcular spawn ─────────────────────────────────────────────────────
        Transform spawn = ObtenerSpawnPoint(playerIndex);
        Vector3 pos = spawn != null ? spawn.position : Vector3.zero;
        Quaternion rot = spawn != null ? spawn.rotation : Quaternion.identity;

        // ── Instanciar el prefab real normalmente ──────────────────────────────
        GameObject instancia = Instantiate(playerPrefabs[playerIndex], pos, rot);
        instancia.name = $"Jugador_{playerIndex + 1}_{playerPrefabs[playerIndex].name}";

        // ── PlayerController ───────────────────────────────────────────────────
        PlayerController pc = instancia.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError($"[GameRoleManager] '{playerPrefabs[playerIndex].name}' necesita PlayerController.");
            return;
        }

        jugadores.Add(pc);

        if (usaTeclado)
            pc.SetUsaMouse(true);

        // ── Asignar rol ───────────────────────────────────────────────────────
        if (playerIndex == 0) ConfigurarCazador(instancia);
        else ConfigurarSobreviviente(instancia);

        Debug.Log($"[GameRoleManager] Jugador {playerIndex + 1} → " +
                  $"'{playerPrefabs[playerIndex].name}' como " +
                  $"{(playerIndex == 0 ? "CAZADOR" : "SOBREVIVIENTE")}");
    }

    // ── Spawn point ───────────────────────────────────────────────────────────
    private Transform ObtenerSpawnPoint(int index)
    {
        if (index == 0) return hunterSpawnPoint;
        int si = index - 1;
        return (si < survivorSpawnPoints.Length) ? survivorSpawnPoints[si] : null;
    }

    // ── Roles ─────────────────────────────────────────────────────────────────
    private void ConfigurarCazador(GameObject jugador)
    {
        jugador.tag = "Hunter";

        Camera cam = jugador.GetComponentInChildren<Camera>();
        if (cam != null && cam.GetComponent<HunterVision>() == null)
            cam.gameObject.AddComponent<HunterVision>();
        else if (cam == null)
            Debug.LogWarning("[GameRoleManager] El prefab del Cazador no tiene Camera hija.");
    }

    private void ConfigurarSobreviviente(GameObject jugador)
    {
        jugador.tag = "Survivor";

        SurvivorDeafness deafness = jugador.AddComponent<SurvivorDeafness>();
        deafness.Inicializar(jugador);
    }

    // ── API pública ───────────────────────────────────────────────────────────
    public List<PlayerController> ObtenerJugadores() => jugadores;

    public bool TodosSobrevivientesVivos()
    {
        foreach (var p in jugadores)
            if (p != null && p.CompareTag("Survivor") && !p.EstaVivo)
                return false;
        return true;
    }
}