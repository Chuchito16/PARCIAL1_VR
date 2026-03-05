using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameRoleManager : MonoBehaviour
{
    public static GameRoleManager Instance { get; private set; }

    [Header("Prefab único (con los 4 modelos hijos)")]
    public GameObject playerPrefab;

    [Header("Nombres de los modelos hijos en orden de entrada")]
    public string[] nombresModelos = new string[] { "Rojo", "Azul", "Verde", "Amarillo" };

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
            Debug.LogError("[GameRoleManager] No se encontró PlayerInputManager.");
            return;
        }

        inputManager.playerPrefab = playerPrefab;
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        int playerIndex = jugadores.Count;

        if (playerIndex >= maxJugadores)
        {
            Debug.LogWarning("[GameRoleManager] Límite de jugadores alcanzado.");
            Destroy(playerInput.gameObject);
            return;
        }

        // ── Spawn: desactivar CharacterController para poder teleportar ───────
        Transform spawn = ObtenerSpawnPoint(playerIndex);
        if (spawn != null)
        {
            CharacterController cc = playerInput.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerInput.transform.position = spawn.position;
            playerInput.transform.rotation = spawn.rotation;
            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning($"[GameRoleManager] No hay spawn point para el jugador {playerIndex + 1}.");
        }

        // ── Activar solo el modelo de este jugador ────────────────────────────
        AsignarModelo(playerInput.gameObject, playerIndex);

        // ── PlayerController ──────────────────────────────────────────────────
        PlayerController pc = playerInput.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError("[GameRoleManager] El prefab no tiene PlayerController.");
            return;
        }

        jugadores.Add(pc);

        foreach (var device in playerInput.devices)
            if (device is Keyboard) { pc.SetUsaMouse(true); break; }

        if (playerIndex == 0) ConfigurarCazador(playerInput.gameObject);
        else ConfigurarSobreviviente(playerInput.gameObject);

        Debug.Log($"[GameRoleManager] Jugador {playerIndex + 1} → modelo '{nombresModelos[playerIndex]}' " +
                  $"en spawn {spawn?.name ?? "NO ENCONTRADO"} " +
                  $"como {(playerIndex == 0 ? "CAZADOR" : "SOBREVIVIENTE")}");

        if (jugadores.Count >= maxJugadores)
            inputManager.DisableJoining();
    }

    // ── Activa el modelo correcto y desactiva los demás ───────────────────────
    private void AsignarModelo(GameObject jugador, int index)
    {
        for (int i = 0; i < nombresModelos.Length; i++)
        {
            Transform modelo = jugador.transform.Find(nombresModelos[i]);
            if (modelo != null)
                modelo.gameObject.SetActive(i == index);
            else
                Debug.LogWarning($"[GameRoleManager] No se encontró hijo '{nombresModelos[i]}' en el prefab.");
        }
    }

    private Transform ObtenerSpawnPoint(int index)
    {
        if (index == 0) return hunterSpawnPoint;
        int si = index - 1;
        return (si < survivorSpawnPoints.Length) ? survivorSpawnPoints[si] : null;
    }

    private void ConfigurarCazador(GameObject jugador)
    {
        jugador.tag = "Hunter";
        Camera cam = jugador.GetComponentInChildren<Camera>();
        if (cam != null && cam.GetComponent<HunterVision>() == null)
            cam.gameObject.AddComponent<HunterVision>();
        else if (cam == null)
            Debug.LogWarning("[GameRoleManager] El prefab no tiene Camera hija.");
    }

    private void ConfigurarSobreviviente(GameObject jugador)
    {
        jugador.tag = "Survivor";
        SurvivorDeafness deafness = jugador.AddComponent<SurvivorDeafness>();
        deafness.Inicializar(jugador);
    }

    public List<PlayerController> ObtenerJugadores() => jugadores;

    public bool TodosSobrevivientesVivos()
    {
        foreach (var p in jugadores)
            if (p != null && p.CompareTag("Survivor") && !p.EstaVivo)
                return false;
        return true;
    }
}