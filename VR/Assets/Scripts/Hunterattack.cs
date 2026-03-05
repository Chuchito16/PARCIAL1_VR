using System.Collections;
using UnityEngine;


public class HunterAttack : MonoBehaviour
{
    private bool cooldown = false;
    private daño hit;
    private CharacterController cc;    

    private void Start()
    {
        hit = GetComponentInChildren<daño>();
        cc = GetComponentInChildren<CharacterController>();

    }
    private void Update()
    {
        cooldown = hit.aplicarDaño;
        StartCoroutine(Gothit());
    }

        IEnumerator Gothit()
    {
        
        if (cooldown == false)
        {

            cc.enabled = false; 

            yield return new WaitForSeconds(2f);

            cc.enabled = true; 
        }
    }
}
