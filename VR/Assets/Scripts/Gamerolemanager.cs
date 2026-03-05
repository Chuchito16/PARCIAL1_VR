using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Antes de que cada jugador se una, cambia el prefab del PlayerInputManager
/// al que corresponde según el orden de entrada. Así PlayerInputManager
/// spawnea directamente el prefab correcto con los devices ya pareados.
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

    private readonly List<PlayerController> jugadores = new();
    private PlayerInputManager inputManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();
        if (inputManager == null)
            inputManager = FindFirstObjectByType<PlayerInputManager>();

        if (inputManager == null)
        {
            Debug.LogError("[GameRoleManager] No se encontró PlayerInputManager en la escena.");
            return;
        }

        // Prepara el primer prefab (Cazador) antes de que alguien se una
        ActualizarPrefabDelManager();
    }

    // ── PlayerInputManager.onPlayerJoined apunta aquí ─────────────────────────
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        int playerIndex = jugadores.Count;

        if (playerIndex >= maxJugadores)
        {
            Debug.LogWarning("[GameRoleManager] Límite de jugadores alcanzado.");
            Destroy(playerInput.gameObject);
            return;
        }

        // ── Mover al spawn correcto ────────────────────────────────────────────
        Transform spawn = ObtenerSpawnPoint(playerIndex);
        if (spawn != null)
            playerInput.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

        // ── PlayerController ───────────────────────────────────────────────────
        PlayerController pc = playerInput.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError($"[GameRoleManager] '{playerInput.gameObject.name}' no tiene PlayerController.");
            return;
        }

        jugadores.Add(pc);

        // Detectar teclado → activar mouse
        foreach (var device in playerInput.devices)
            if (device is Keyboard) { pc.SetUsaMouse(true); break; }

        // ── Asignar rol ───────────────────────────────────────────────────────
        if (playerIndex == 0) ConfigurarCazador(playerInput.gameObject);
        else ConfigurarSobreviviente(playerInput.gameObject);

        Debug.Log($"[GameRoleManager] Jugador {playerIndex + 1} unido como " +
                  $"{(playerIndex == 0 ? "CAZADOR" : "SOBREVIVIENTE")} " +
                  $"con prefab '{playerInput.gameObject.name}'");

        // ── Preparar el prefab para el SIGUIENTE jugador ───────────────────────
        ActualizarPrefabDelManager();
    }

    // ── Cambia el playerPrefab del manager al siguiente en la lista ───────────
    private void ActualizarPrefabDelManager()
    {
        if (inputManager == null) return;

        int siguiente = jugadores.Count; // después de añadir ya apunta al próximo
        if (siguiente < playerPrefabs.Length && playerPrefabs[siguiente] != null)
        {
            inputManager.playerPrefab = playerPrefabs[siguiente];
            Debug.Log($"[GameRoleManager] Siguiente prefab listo: '{playerPrefabs[siguiente].name}'");
        }
        else
        {
            // No quedan más prefabs, deshabilitar joining
            inputManager.DisableJoining();
            Debug.Log("[GameRoleManager] No hay más slots disponibles, joining desactivado.");
        }
    }

    // ── Spawn points ──────────────────────────────────────────────────────────
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
            Debug.LogWarning("[GameRoleManager] El prefab Cazador no tiene Camera hija.");
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