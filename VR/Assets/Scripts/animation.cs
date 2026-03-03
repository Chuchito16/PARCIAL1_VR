using UnityEngine;

public class animation : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

    // Nombres de los parámetros en el Animator
    [SerializeField] private string walkParam = "walk";
    [SerializeField] private string runParam = "run";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        // Buscamos el PlayerController en los padres
        playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (playerController == null || animator == null) return;

        ActualizarAnimaciones();
    }

    private void ActualizarAnimaciones()
    {
        // 1. Obtener la velocidad actual del CharacterController o del input
        // Usamos la magnitud del movimiento horizontal para el float "Walk"
        // Accedemos a los datos de movimiento a través de la lógica del padre

        // Calculamos si el jugador se está moviendo basándonos en sus inputs
        // Nota: Como moveAction es privado en tu script original, 
        // lo ideal sería que el PlayerController tuviera una propiedad pública.
        // Pero para no modificar tu script original, calcularemos la velocidad relativa:

        Vector3 horizontalVelocity = GetComponentInParent<CharacterController>().velocity;
        horizontalVelocity.y = 0; // Ignoramos el salto/caída

        float currentSpeed = horizontalVelocity.magnitude;

        // 2. Pasar el Float (Caminar)
        // Normalizamos el valor (0 a 1) para un Blend Tree, o pasamos la velocidad pura
        animator.SetFloat(walkParam, currentSpeed);

        // 3. Pasar el Bool (Correr)
        // Detectamos si el usuario está pulsando el shift (esto requiere que 'corriendo' 
        // en PlayerController sea accesible o usar una lógica similar)

        // Para que esto funcione sin errores, asegúrate de que 'corriendo' 
        // en PlayerController sea 'public' o usa esta técnica:
        bool isRunning = currentSpeed > playerController.velocidadCaminar + 0.1f;
        animator.SetBool(runParam, isRunning);
    }
}