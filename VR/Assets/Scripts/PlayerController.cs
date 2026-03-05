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
    private bool usaMouse = false;

    // ── Estado ────────────────────────────────────────────────────────────────
    public bool EstaVivo { get; private set; } = true;

    // ── Máquina actual ────────────────────────────────────────────────────────
    private MachineRepair maquinaActual = null;
    private bool presionandoInteraccion = false;

    // ── Ataque (cazador) ──────────────────────────────────────────────────────
    private HunterAttack hunterAttack;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        hunterAttack = GetComponent<HunterAttack>();
    }

    private void Start()
    {
        PlayerInput pi = GetComponent<PlayerInput>();
        if (pi != null)
            foreach (var device in pi.devices)
                if (device is Keyboard) { SetUsaMouse(true); break; }
    }

    public void SetUsaMouse(bool valor)
    {
        usaMouse = valor;
        if (usaMouse) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        Debug.Log($"[PlayerController] {gameObject.name} → usaMouse = {valor}");
    }

    private void Update()
    {
        if (!EstaVivo) return;

        grounded = controller.isGrounded;
        if (grounded && playerVelocity.y < -2f) playerVelocity.y = -2f;

        if (usaMouse && Mouse.current != null)
            lookInput = Mouse.current.delta.ReadValue();

        HandleMovimiento();
        HandleCamara();
        playerVelocity.y += gravedad * Time.deltaTime;

        if (CompareTag("Survivor")) HandleInteraccionMaquina();
    }

    private void HandleMovimiento()
    {
        float speed = corriendo ? velocidadCorrer : velocidadCaminar;
        Vector3 forward = camTransform != null ? camTransform.forward : transform.forward;
        Vector3 right = camTransform != null ? camTransform.right : transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 moveDir = Vector3.ClampMagnitude(forward * moveInput.y + right * moveInput.x, 1f);
        controller.Move((moveDir * speed + Vector3.up * playerVelocity.y) * Time.deltaTime);
    }

    private void HandleCamara()
    {
        transform.Rotate(Vector3.up * lookInput.x * sensibilidad);
        pitch -= lookInput.y * sensibilidad;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        if (camTransform != null)
        {
            Vector3 a = camTransform.localEulerAngles;
            a.x = pitch;
            camTransform.localEulerAngles = a;
        }
    }

    private void HandleInteraccionMaquina()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radioDeteccionMaquina, layerMaquinas);
        MachineRepair cercana = null;
        float distMin = float.MaxValue;

        foreach (var hit in hits)
        {
            MachineRepair maq = hit.GetComponent<MachineRepair>();
            if (maq != null && !maq.EstaReparada)
            {
                float d = Vector3.Distance(transform.position, hit.transform.position);
                if (d < distMin) { distMin = d; cercana = maq; }
            }
        }

        if (cercana != maquinaActual)
        {
            if (maquinaActual != null) maquinaActual.SetInteractuando(false);
            maquinaActual = cercana;
        }

        if (maquinaActual != null)
            maquinaActual.SetInteractuando(presionandoInteraccion);
    }

    public void Morir()
    {
        if (!EstaVivo) return;
        EstaVivo = false;
        if (maquinaActual != null) maquinaActual.SetInteractuando(false);
        controller.enabled = false;
        Debug.Log($"[PlayerController] {gameObject.name} ha muerto.");
    }

    public void Revivir()
    {
        EstaVivo = true;
        controller.enabled = true;
        playerVelocity = Vector3.zero;
        Debug.Log($"[PlayerController] {gameObject.name} revivido.");
    }

    // ── Eventos Input ─────────────────────────────────────────────────────────
    public void OnMove(InputAction.CallbackContext ctx)
        => moveInput = ctx.ReadValue<Vector2>();

    public void OnCamara(InputAction.CallbackContext ctx)
    { if (!usaMouse) lookInput = ctx.ReadValue<Vector2>(); }

    public void OnJump(InputAction.CallbackContext ctx)
    { if (ctx.performed && grounded) playerVelocity.y = Mathf.Sqrt(fuerzaSalto * -2f * gravedad); }

    public void OnCorrer(InputAction.CallbackContext ctx)
    { if (ctx.performed) corriendo = true; else if (ctx.canceled) corriendo = false; }

    public void OnInteractuar(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            presionandoInteraccion = true;
            if (CompareTag("Hunter")) hunterAttack?.Atacar();
        }
        else if (ctx.canceled)
        {
            presionandoInteraccion = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccionMaquina);
    }
}