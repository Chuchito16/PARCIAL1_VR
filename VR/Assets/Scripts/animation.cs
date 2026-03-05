using UnityEngine;

public class animation : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

   
    [SerializeField] private string walkParam = "walk";
    [SerializeField] private string runParam = "run";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    
        playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (playerController == null || animator == null) return;

        ActualizarAnimaciones();
    }

    private void ActualizarAnimaciones()
    {
 

        Vector3 horizontalVelocity = GetComponentInParent<CharacterController>().velocity;
        horizontalVelocity.y = 0; 

        float currentSpeed = horizontalVelocity.magnitude;

 
        animator.SetFloat(walkParam, currentSpeed);

   
        bool isRunning = currentSpeed > playerController.velocidadCaminar + 0.1f;
        animator.SetBool(runParam, isRunning);
    }
}