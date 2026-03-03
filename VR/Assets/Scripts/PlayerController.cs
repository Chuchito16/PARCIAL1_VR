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

    // ── Internos ─────────────────────────────────────────────────────────────
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool grounded;
    private float pitch;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool corriendo;

    // ── Mouse ─────────────────────────────────────────────────────────────────
    private bool usaMouse = false;

    // ── Awake ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // ── Start ─────────────────────────────────────────────────────────────────
    private void Start()
    {
        // Detectar si este jugador tiene teclado → si sí, usa mouse para cámara
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
        grounded = controller.isGrounded;
        if (grounded && playerVelocity.y < -2f)
            playerVelocity.y = -2f;

        // Leer mouse delta directo — completamente independiente del InputSystem
        if (usaMouse && Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            lookInput = delta;
        }

        HandleMovimiento();
        HandleCamara();

        playerVelocity.y += gravedad * Time.deltaTime;
    }

    // ── Lógica de movimiento ──────────────────────────────────────────────────
    private void HandleMovimiento()
    {
        float speed = corriendo ? velocidadCorrer : velocidadCaminar;

        Vector3 forward = camTransform != null ? camTransform.forward : transform.forward;
        Vector3 right = camTransform != null ? camTransform.right : transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * moveInput.y + right * moveInput.x);
        moveDir = Vector3.ClampMagnitude(moveDir, 1f);
        Vector3 finalMove = moveDir * speed + Vector3.up * playerVelocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }

    // ── Lógica de cámara ──────────────────────────────────────────────────────
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

    // ═════════════════════════════════════════════════════════════════════════
    //  EVENTOS
    // ═════════════════════════════════════════════════════════════════════════

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Solo activo para gamepad; mouse se lee en Update()
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
        if (context.performed)
            corriendo = true;
        else if (context.canceled)
            corriendo = false;
    }

    public void OnInteractuar(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("¡Interactuando!");
        }
    }
}