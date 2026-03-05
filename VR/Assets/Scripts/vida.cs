using UnityEngine;

public class vida : MonoBehaviour
{

    public int vidaMaxima = 2;
    private CharacterController cc;
    private Animator animator;
    
    void Start()
    {
        cc = GetComponentInChildren<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    
    void Update()
    {
        if (vidaMaxima == 0)
        {
            
            cc.enabled = false;
            animator.SetBool("dead", true);
        }
        
    }
}
