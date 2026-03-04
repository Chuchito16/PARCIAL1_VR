using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer = 10f;
    public float fuerzaSalto = 5f;
    public float gravedad = -9.81f;

    [Header("Cámara")]
    public Transform camTransform;
    public float sensibilidad = 2f;
    public float pitchMin = -80f;
    public float pitchMax = 80f;

    [Header("Interacción con Máquinas")]
    [Tooltip("Radio para detectar máquinas cercanas")]
    public float radioDeteccionMaquina = 2f;
    public LayerMask layerMaquinas;

    // ── Internos ──────────────────────────────────────────────────────────────
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool grounded;
    private float pitch;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool corriendo;

    // ── Mouse ─────────────────────────────────────────────────────────────────
    private bool usaMouse = false;

    // ── Rol ───────────────────────────────────────────────────────────────────
    public bool EstaVivo { get; private set; } = true;

    // ── Máquina actual ────────────────────────────────────────────────────────
    private MachineRepair maquinaActual = null;
    private bool presionandoInteraccion = false;

    // ── Awake ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // ── Start ─────────────────────────────────────────────────────────────────
    private void Start()
    {
        PlayerInput pi = GetComponent<PlayerInput>();
        if (pi != null)
        {
            foreach (var device in pi.devices)
            {
                if (device is Keyboard)
                {
                    SetUsaMouse(true);
                    break;
                }
            }
        }
    }

    public void SetUsaMouse(bool valor)
    {
        usaMouse = valor;
        if (usaMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        Debug.Log($"[PlayerController] {gameObject.name} → usaMouse = {valor}");
    }

    // ── Update ────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (!EstaVivo) return;

        grounded = controller.isGrounded;
        if (grounded && playerVelocity.y < -2f)
            playerVelocity.y = -2f;

        if (usaMouse && Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            lookInput = delta;
        }

        HandleMovimiento();
        HandleCamara();

        playerVelocity.y += gravedad * Time.deltaTime;

        // Solo los sobrevivientes interactúan con máquinas
        if (CompareTag("Survivor"))
            HandleInteraccionMaquina();
    }

    // ── Movimiento ────────────────────────────────────────────────────────────
    private void HandleMovimiento()
    {
        float speed = corriendo ? velocidadCorrer : velocidadCaminar;

        Vector3 forward = camTransform != null ? camTransform.forward : transform.forward;
        Vector3 right = camTransform != null ? camTransform.right : transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = Vector3.ClampMagnitude(forward * moveInput.y + right * moveInput.x, 1f);
        controller.Move((moveDir * speed + Vector3.up * playerVelocity.y) * Time.deltaTime);
    }

    // ── Cámara ────────────────────────────────────────────────────────────────
    private void HandleCamara()
    {
        transform.Rotate(Vector3.up * lookInput.x * sensibilidad);

        pitch -= lookInput.y * sensibilidad;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        if (camTransform != null)
        {
            Vector3 angles = camTransform.localEulerAngles;
            angles.x = pitch;
            camTransform.localEulerAngles = angles;
        }
    }

    // ── Detección y uso de máquinas ───────────────────────────────────────────
    private void HandleInteraccionMaquina()
    {
        // Detectar la máquina más cercana en rango
        Collider[] hits = Physics.OverlapSphere(transform.position, radioDeteccionMaquina, layerMaquinas);

        MachineRepair maquinaMasCercana = null;
        float distMin = float.MaxValue;

        foreach (var hit in hits)
        {
            MachineRepair maq = hit.GetComponent<MachineRepair>();
            if (maq != null && !maq.EstaReparada)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < distMin)
                {
                    distMin = dist;
                    maquinaMasCercana = maq;
                }
            }
        }

        // Cambiar la máquina objetivo si cambió
        if (maquinaMasCercana != maquinaActual)
        {
            // Dejar de reparar la anterior
            if (maquinaActual != null)
                maquinaActual.SetInteractuando(false);

            maquinaActual = maquinaMasCercana;
        }

        // Seguir reparando si mantiene la interacción
        if (maquinaActual != null)
            maquinaActual.SetInteractuando(presionandoInteraccion);
    }

    // ── Muerte del jugador ────────────────────────────────────────────────────
    public void Morir()
    {
        if (!EstaVivo) return;
        EstaVivo = false;

        // Soltar máquina si estaba reparando
        if (maquinaActual != null)
            maquinaActual.SetInteractuando(false);

        Debug.Log($"[PlayerController] {gameObject.name} ha muerto.");
        // Aquí puedes agregar: ragdoll, desactivar control, etc.
        controller.enabled = false;
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  EVENTOS DE INPUT
    // ═════════════════════════════════════════════════════════════════════════

    public void OnMove(InputAction.CallbackContext context)
        => moveInput = context.ReadValue<Vector2>();

    public void OnCamara(InputAction.CallbackContext context)
    {
        if (!usaMouse)
            lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && grounded)
            playerVelocity.y = Mathf.Sqrt(fuerzaSalto * -2f * gravedad);
    }

    public void OnCorrer(InputAction.CallbackContext context)
    {
        if (context.performed) corriendo = true;
        else if (context.canceled) corriendo = false;
    }

    /// <summary>
    /// Mantener presionado → repara máquina cercana (solo sobrevivientes)
    /// </summary>
    public void OnInteractuar(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            presionandoInteraccion = true;

            // Cazador: lógica de ataque/captura aquí si se desea
            if (CompareTag("Hunter"))
                Debug.Log("[Hunter] ¡Atacando!");
        }
        else if (context.canceled)
        {
            presionandoInteraccion = false;
        }
    }

    // ── Gizmo de radio de interacción ─────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccionMaquina);
    }
}