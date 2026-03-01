using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 5f;
    [SerializeField] private float velocidadCorrer = 10f;
    [SerializeField] private float deadzone = 0.4f;

    [Header("Salto")]
    [SerializeField] private float fuerzaSalto = 5f;
    [SerializeField] private float gravedad = -9.81f;

    [Header("Cámara")]
    [SerializeField] private Transform camTransform;
    [SerializeField] private float sensibilidadCamara = 2f;
    [SerializeField] private float pitchMin = -80f;
    [SerializeField] private float pitchMax = 80f;

    private CharacterController controller;
    private PlayerInput playerInput;

    private InputAction accionMovimiento;
    private InputAction accionSalto;
    private InputAction accionCorrer;
    private InputAction accionCamara;
    private InputAction accionInteractuar;

    private Vector2 inputMovimiento;
    private Vector2 inputCamara;
    private bool corriendo;
    private Vector3 velocidadVertical;
    private float pitchActual = 0f;

    // Referencias guardadas para desuscribirse correctamente
    private System.Action<InputAction.CallbackContext> onMovimientoPerformed;
    private System.Action<InputAction.CallbackContext> onMovimientoCanceled;
    private System.Action<InputAction.CallbackContext> onSaltoPerformed;
    private System.Action<InputAction.CallbackContext> onCorrerPerformed;
    private System.Action<InputAction.CallbackContext> onCorrerCanceled;
    private System.Action<InputAction.CallbackContext> onCamaraPerformed;
    private System.Action<InputAction.CallbackContext> onCamaraCanceled;
    private System.Action<InputAction.CallbackContext> onInteractuarPerformed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Asigna la cámara automáticamente desde los hijos del prefab
        if (camTransform == null)
        {
            Camera camHija = GetComponentInChildren<Camera>();
            if (camHija != null)
                camTransform = camHija.transform;
            else
                Debug.LogWarning($"[PlayerController] Jugador {playerInput.playerIndex}: no se encontró cámara hija.");
        }

        // Solo el jugador 0 mantiene el AudioListener activo para evitar conflictos
        AudioListener audioListener = GetComponentInChildren<AudioListener>();
        if (audioListener != null)
            audioListener.enabled = (playerInput.playerIndex == 0);

        // Lockear cursor solo en PC
        if (playerInput.playerIndex == 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        accionMovimiento = playerInput.actions["Movimiento"];
        accionSalto = playerInput.actions["Salto"];
        accionCorrer = playerInput.actions["Correr"];
        accionCamara = playerInput.actions["Camara"];
        accionInteractuar = playerInput.actions["Interactuar"];
    }

    private void OnEnable()
    {
        // Se crean y guardan las lambdas para poder desuscribirse después
        onMovimientoPerformed = ctx => inputMovimiento = ctx.ReadValue<Vector2>();
        onMovimientoCanceled = ctx => inputMovimiento = Vector2.zero;
        onSaltoPerformed = ctx => Saltar();
        onCorrerPerformed = ctx => corriendo = ctx.ReadValue<float>() > 0.5f;
        onCorrerCanceled = ctx => corriendo = false;
        onCamaraPerformed = ctx => inputCamara = ctx.ReadValue<Vector2>();
        onCamaraCanceled = ctx => inputCamara = Vector2.zero;
        onInteractuarPerformed = ctx => Interactuar();

        accionMovimiento.performed += onMovimientoPerformed;
        accionMovimiento.canceled += onMovimientoCanceled;
        accionSalto.performed += onSaltoPerformed;
        accionCorrer.performed += onCorrerPerformed;
        accionCorrer.canceled += onCorrerCanceled;
        accionCamara.performed += onCamaraPerformed;
        accionCamara.canceled += onCamaraCanceled;
        accionInteractuar.performed += onInteractuarPerformed;

        accionMovimiento.Enable();
        accionSalto.Enable();
        accionCorrer.Enable();
        accionCamara.Enable();
        accionInteractuar.Enable();
    }

    private void OnDisable()
    {
        accionMovimiento.performed -= onMovimientoPerformed;
        accionMovimiento.canceled -= onMovimientoCanceled;
        accionSalto.performed -= onSaltoPerformed;
        accionCorrer.performed -= onCorrerPerformed;
        accionCorrer.canceled -= onCorrerCanceled;
        accionCamara.performed -= onCamaraPerformed;
        accionCamara.canceled -= onCamaraCanceled;
        accionInteractuar.performed -= onInteractuarPerformed;

        accionMovimiento.Disable();
        accionSalto.Disable();
        accionCorrer.Disable();
        accionCamara.Disable();
        accionInteractuar.Disable();
    }

    private void Update()
    {
        ManejarMovimiento();
        ManejarCamara();
        AplicarGravedad();
    }

    private void ManejarMovimiento()
    {
        Vector2 inputFiltrado = inputMovimiento;
        if (inputFiltrado.magnitude < deadzone)
            inputFiltrado = Vector2.zero;

        float velocidad = corriendo ? velocidadCorrer : velocidadCaminar;
        Vector3 direccion = transform.right * inputFiltrado.x
                          + transform.forward * inputFiltrado.y;
        controller.Move(direccion * velocidad * Time.deltaTime);
    }

    private void Saltar()
    {
        if (controller.isGrounded)
            velocidadVertical.y = Mathf.Sqrt(fuerzaSalto * -2f * gravedad);
    }

    private void AplicarGravedad()
    {
        if (controller.isGrounded && velocidadVertical.y < 0f)
            velocidadVertical.y = -2f;

        velocidadVertical.y += gravedad * Time.deltaTime;
        controller.Move(velocidadVertical * Time.deltaTime);
    }

    private void ManejarCamara()
    {
        if (camTransform == null) return;

        Vector2 camaraFiltrada = inputCamara;
        if (camaraFiltrada.magnitude < deadzone)
            camaraFiltrada = Vector2.zero;

        transform.Rotate(Vector3.up * camaraFiltrada.x * sensibilidadCamara);

        pitchActual -= camaraFiltrada.y * sensibilidadCamara;
        pitchActual = Mathf.Clamp(pitchActual, pitchMin, pitchMax);
        camTransform.localRotation = Quaternion.Euler(pitchActual, 0f, 0f);
    }

    private void Interactuar()
    {
        if (camTransform == null) return;

        Ray ray = new Ray(camTransform.position, camTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            IInteractuable interactuable = hit.collider.GetComponent<IInteractuable>();
            interactuable?.Interactuar();
        }

        Debug.Log($"[PlayerController] Jugador {playerInput.playerIndex} interactuó.");
    }
}

public interface IInteractuable
{
    void Interactuar();
}