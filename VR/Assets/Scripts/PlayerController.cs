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
    public float sensibilidad = 0.15f;
    public float pitchMin = -80f;
    public float pitchMax = 80f;

    // ── Internos ──────────────────────────────────────────────────────────────
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool grounded;
    private float pitch;
    private bool corriendo;
    private bool cursorBloqueado;

    // Input Actions generadas por código — no depende del Inspector
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction interactAction;
    private InputAction escapeAction;

    // ── Awake ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Movimiento WASD
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Cámara: Mouse Delta — este es el correcto para FPS
        lookAction = new InputAction("Look", InputActionType.Value);
        lookAction.AddBinding("<Mouse>/delta");

        // Salto
        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");

        // Correr
        runAction = new InputAction("Run", InputActionType.Button);
        runAction.AddBinding("<Keyboard>/leftShift");

        // Interactuar
        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");

        // Escape
        escapeAction = new InputAction("Escape", InputActionType.Button);
        escapeAction.AddBinding("<Keyboard>/escape");

        // Activar todas
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        runAction.Enable();
        interactAction.Enable();
        escapeAction.Enable();
    }

    // ── Start ─────────────────────────────────────────────────────────────────
    private void Start()
    {
        BloquearCursor(true);
    }

    // ── OnDestroy: limpiar acciones ───────────────────────────────────────────
    private void OnDestroy()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        runAction.Disable();
        interactAction.Disable();
        escapeAction.Disable();
    }

    // ── Update ────────────────────────────────────────────────────────────────
    private void Update()
    {
        // Escape
        if (escapeAction.WasPressedThisFrame())
            BloquearCursor(!cursorBloqueado);

        // Interactuar
        if (interactAction.WasPressedThisFrame())
            Debug.Log("¡Interactuando!");

        grounded = controller.isGrounded;
        if (grounded && playerVelocity.y < -2f)
            playerVelocity.y = -2f;

        corriendo = runAction.IsPressed();

        HandleMovimiento();
        HandleCamara();
        HandleSalto();

        playerVelocity.y += gravedad * Time.deltaTime;
    }

    // ── Cursor ────────────────────────────────────────────────────────────────
    private void BloquearCursor(bool bloquear)
    {
        cursorBloqueado = bloquear;
        Cursor.lockState = bloquear ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !bloquear;
    }

    // ── Movimiento ────────────────────────────────────────────────────────────
    private void HandleMovimiento()
    {
        float speed = corriendo ? velocidadCorrer : velocidadCaminar;
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 moveDir = Vector3.ClampMagnitude(
            transform.forward * input.y + transform.right * input.x, 1f);

        controller.Move((moveDir * speed + Vector3.up * playerVelocity.y) * Time.deltaTime);
    }

    // ── Cámara ────────────────────────────────────────────────────────────────
    private void HandleCamara()
    {
        if (!cursorBloqueado) return;

        Vector2 delta = lookAction.ReadValue<Vector2>();

        // Horizontal → rota el cuerpo del Player
        transform.Rotate(Vector3.up * delta.x * sensibilidad);

        // Vertical → solo la cámara (pitch)
        pitch -= delta.y * sensibilidad;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        if (camTransform != null)
            camTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // ── Salto ─────────────────────────────────────────────────────────────────
    private void HandleSalto()
    {
        if (jumpAction.WasPressedThisFrame() && grounded)
            playerVelocity.y = Mathf.Sqrt(fuerzaSalto * -2f * gravedad);
    }
}