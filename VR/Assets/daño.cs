using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class daño : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int cantidadDaño = 1;
    public bool aplicarDaño = true;

   

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Survivor"))
        {
            vida scriptVida = other.GetComponent<vida>(); 
            if (aplicarDaño==true)
            {
                scriptVida.vidaMaxima -= cantidadDaño;
                Debug.Log("el jugador recibio un hit");
                StartCoroutine(AplicarDañoGlobal());
            }
            
        }

    }

    IEnumerator AplicarDañoGlobal()
    {
        
        aplicarDaño = false;
        
        yield return new WaitForSeconds(3f); // Pequeña espera para asegurar que el golpe se registre
        
        aplicarDaño = true;
        
    }

}
