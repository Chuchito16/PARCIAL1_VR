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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        accionMovimiento = playerInput.actions["Movimiento"];
        accionSalto = playerInput.actions["Salto"];
        accionCorrer = playerInput.actions["Correr"];
        accionCamara = playerInput.actions["Camara"];
        accionInteractuar = playerInput.actions["Interactuar"];
    }

    private void OnEnable()
    {
        accionMovimiento.performed += ctx => inputMovimiento = ctx.ReadValue<Vector2>();
        accionMovimiento.canceled += ctx => inputMovimiento = Vector2.zero;

        accionSalto.performed += ctx => Saltar();

        accionCorrer.performed += ctx => corriendo = ctx.ReadValue<float>() > 0.5f;
        accionCorrer.canceled += ctx => corriendo = false;

        accionCamara.performed += ctx => inputCamara = ctx.ReadValue<Vector2>();
        accionCamara.canceled += ctx => inputCamara = Vector2.zero;

        accionInteractuar.performed += ctx => Interactuar();

        accionMovimiento.Enable();
        accionSalto.Enable();
        accionCorrer.Enable();
        accionCamara.Enable();
        accionInteractuar.Enable();
    }

    private void OnDisable()
    {
        accionMovimiento.performed -= ctx => inputMovimiento = ctx.ReadValue<Vector2>();
        accionMovimiento.canceled -= ctx => inputMovimiento = Vector2.zero;
        accionSalto.performed -= ctx => Saltar();
        accionCorrer.performed -= ctx => corriendo = ctx.ReadValue<float>() > 0.5f;
        accionCorrer.canceled -= ctx => corriendo = false;
        accionCamara.performed -= ctx => inputCamara = ctx.ReadValue<Vector2>();
        accionCamara.canceled -= ctx => inputCamara = Vector2.zero;
        accionInteractuar.performed -= ctx => Interactuar();
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
        Debug.Log("Interactuar presionado");
    }
}

public interface IInteractuable
{
    void Interactuar();
}