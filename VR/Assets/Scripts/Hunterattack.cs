using System.Collections;
using UnityEngine;

/// <summary>
/// Permite al cazador golpear sobrevivientes cercanos al presionar Interactuar.
/// Se adjunta automáticamente junto con PlayerController en el prefab del cazador.
/// </summary>
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
