using UnityEngine;

public class vida : MonoBehaviour
{

    public int vidaMaxima = 2;
    private CharacterController cc;
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponentInChildren<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (vidaMaxima == 0)
        {
            
            cc.enabled = false;
            animator.SetBool("dead", true);
        }
        
    }
}
