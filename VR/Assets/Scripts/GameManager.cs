using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ponlo en el mismo GameObject que tiene el PlayerInputManager.
/// Se encarga de reaccionar cuando un jugador se une o se va,
/// y conecta automáticamente los eventos del InputSystem al PlayerController.
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Color[] coloresJugadores = new Color[]
    {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow
    };

    // ── Start: unir jugador de teclado+mouse automáticamente ─────────────────
    private void Start()
    {
        // Verificar que haya teclado y mouse disponibles
        if (Keyboard.current == null || Mouse.current == null)
        {
            Debug.LogWarning("[GameManager] No se encontró teclado o mouse. No se unirá jugador de teclado.");
            return;
        }

        // Unir al jugador 0 con teclado+mouse automáticamente
        PlayerInputManager.instance.JoinPlayer(
            playerIndex: 0,
            splitScreenIndex: -1,
            controlScheme: null,           // null = que Unity detecte el scheme automáticamente
            pairWithDevices: new InputDevice[] { Keyboard.current, Mouse.current }
        );

        Debug.Log("[GameManager] Jugador de teclado+mouse unido automáticamente.");
    }

    // ── El PlayerInputManager llama este método automáticamente ──────────────
    public void OnPlayerJoined(PlayerInput nuevoJugador)
    {
        int index = nuevoJugador.playerIndex;
        Debug.Log($"[GameManager] Jugador {index} se unió con dispositivo: {nuevoJugador.devices[0].displayName}");

        PlayerController pc = nuevoJugador.GetComponent<PlayerController>();
        if (pc != null)
        {
            // Habilitar todos los action maps para asegurar que las acciones estén activas
            foreach (var map in nuevoJugador.actions.actionMaps)
                map.Enable();

            InputActionMap actionMap = nuevoJugador.actions.actionMaps[0];

            // Movimiento
            InputAction moveAction = actionMap.FindAction("Movimiento");
            if (moveAction != null)
            {
                moveAction.performed += pc.OnMove;
                moveAction.canceled += pc.OnMove;
            }
            else Debug.LogWarning($"[GameManager] Acción 'Movimiento' no encontrada para jugador {index}");

            // Cámara — para gamepad; el mouse se lee directo en PlayerController.Update
            InputAction camaraAction = actionMap.FindAction("Camara");
            if (camaraAction != null)
            {
                camaraAction.performed += pc.OnCamara;
                camaraAction.canceled += pc.OnCamara;
            }
            else Debug.LogWarning($"[GameManager] Acción 'Camara' no encontrada para jugador {index}");

            // Salto
            InputAction jumpAction = actionMap.FindAction("Salto");
            if (jumpAction != null)
                jumpAction.performed += pc.OnJump;
            else Debug.LogWarning($"[GameManager] Acción 'Salto' no encontrada para jugador {index}");

            // Correr
            InputAction correrAction = actionMap.FindAction("Correr");
            if (correrAction != null)
            {
                correrAction.performed += pc.OnCorrer;
                correrAction.canceled += pc.OnCorrer;
            }
            else Debug.LogWarning($"[GameManager] Acción 'Correr' no encontrada para jugador {index}");

            // Interactuar
            InputAction interactuarAction = actionMap.FindAction("Interactuar");
            if (interactuarAction != null)
                interactuarAction.performed += pc.OnInteractuar;
            else Debug.LogWarning($"[GameManager] Acción 'Interactuar' no encontrada para jugador {index}");

            Debug.Log($"[GameManager] PlayerController del jugador {index} conectado correctamente.");

            // ── Detectar si el jugador usa mouse ─────────────────────────────
            bool tieneMouse = false;
            foreach (var device in nuevoJugador.devices)
            {
                if (device is Mouse)
                {
                    tieneMouse = true;
                    break;
                }
            }

            if (tieneMouse)
            {
                pc.SetUsaMouse(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Debug.Log($"[GameManager] Mouse activado para jugador {index}.");
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] No se encontró PlayerController en el jugador {index}.");
        }

        // Opcional: color del mesh
        Renderer meshRenderer = nuevoJugador.GetComponentInChildren<Renderer>();
        if (meshRenderer != null && index < coloresJugadores.Length)
            meshRenderer.material.color = coloresJugadores[index];

        // Opcional: spawn point
        SpawnEnPosicion(nuevoJugador.gameObject, index);
    }

    // ── El PlayerInputManager llama este método automáticamente ──────────────
    public void OnPlayerLeft(PlayerInput jugadorSaliente)
    {
        Debug.Log($"[GameManager] Jugador {jugadorSaliente.playerIndex} se desconectó.");

        PlayerController pc = jugadorSaliente.GetComponent<PlayerController>();
        if (pc != null)
        {
            foreach (var actionMap in jugadorSaliente.actions.actionMaps)
            {
                InputAction moveAction = actionMap.FindAction("Movimiento");
                if (moveAction != null) { moveAction.performed -= pc.OnMove; moveAction.canceled -= pc.OnMove; }

                InputAction camaraAction = actionMap.FindAction("Camara");
                if (camaraAction != null) { camaraAction.performed -= pc.OnCamara; camaraAction.canceled -= pc.OnCamara; }

                InputAction jumpAction = actionMap.FindAction("Salto");
                if (jumpAction != null) jumpAction.performed -= pc.OnJump;

                InputAction correrAction = actionMap.FindAction("Correr");
                if (correrAction != null) { correrAction.performed -= pc.OnCorrer; correrAction.canceled -= pc.OnCorrer; }

                InputAction interactuarAction = actionMap.FindAction("Interactuar");
                if (interactuarAction != null) interactuarAction.performed -= pc.OnInteractuar;
            }

            // Liberar cursor si era el jugador con mouse
            foreach (var device in jugadorSaliente.devices)
            {
                if (device is Mouse)
                {
                    pc.SetUsaMouse(false);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
                }
            }
        }
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────
    private void SpawnEnPosicion(GameObject jugador, int index)
    {
        GameObject spawnPoint = GameObject.Find($"SpawnPoint{index}");
        if (spawnPoint != null)
            jugador.transform.position = spawnPoint.transform.position;
        else
            jugador.transform.position = new Vector3(index * 3f, 1f, 0f);
    }
}