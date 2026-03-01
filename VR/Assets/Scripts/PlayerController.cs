using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer  = 10f;
    public float fuerzaSalto      = 5f;
    public float gravedad         = -9.81f;

    [Header("Cámara")]
    public Transform camTransform;
    public float sensibilidad = 2f;
    public float pitchMin     = -80f;
    public float pitchMax     =  80f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference runAction;
    public InputActionReference jumpAction;
    public InputActionReference lookAction;
    public InputActionReference interactAction;

    // Se asigna automáticamente, no hace falta arrastrar en el Inspector
    private CharacterController controller;

    private Vector3 playerVelocity;
    private bool    grounded;
    private float   pitch;

    private void Awake()
    {
        // Auto-asigna el CharacterController y la cámara
        controller = GetComponent<CharacterController>();

        if (camTransform == null)
            camTransform = GetComponentInChildren<Camera>()?.transform;
    }

    private void OnEnable()
    {
        // Verifica que cada acción esté asignada antes de habilitarla
        moveAction?.action.Enable();
        runAction?.action.Enable();
        jumpAction?.action.Enable();
        lookAction?.action.Enable();
        interactAction?.action.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
        runAction?.action.Disable();
        jumpAction?.action.Disable();
        lookAction?.action.Disable();
        interactAction?.action.Disable();
    }

    void Update()
    {
        grounded = controller.isGrounded;
        if (grounded && playerVelocity.y < -2f)
            playerVelocity.y = -2f;

        // --- Movimiento ---
        Vector2 input     = moveAction.action.ReadValue<Vector2>();
        bool    corriendo = runAction.action.IsPressed();
        float   velocidad = corriendo ? velocidadCorrer : velocidadCaminar;
        Vector3 move      = transform.right * input.x + transform.forward * input.y;
        controller.Move(move * velocidad * Time.deltaTime);

        // --- Cámara ---
        if (camTransform != null)
        {
            Vector2 look = lookAction.action.ReadValue<Vector2>();
            transform.Rotate(Vector3.up * look.x * sensibilidad);
            pitch = Mathf.Clamp(pitch - look.y * sensibilidad, pitchMin, pitchMax);
            camTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // --- Salto ---
        if (grounded && jumpAction.action.WasPressedThisFrame())
            playerVelocity.y = Mathf.Sqrt(fuerzaSalto * -2f * gravedad);

        // --- Gravedad ---
        playerVelocity.y += gravedad * Time.deltaTime;
        controller.Move(Vector3.up * playerVelocity.y * Time.deltaTime);

        // --- Interacción ---
        if (interactAction.action.WasPressedThisFrame() && camTransform != null)
            if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit, 3f))
                hit.collider.GetComponent<IInteractuable>()?.Interactuar();
    }
}

public interface IInteractuable
{
    void Interactuar();
}