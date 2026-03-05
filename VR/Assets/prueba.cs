using UnityEngine;

public class prueba : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Survivor"))
        {
            Debug.Log("El jugador ha entrado en el trigger.");
            // Aquí puedes agregar cualquier lógica adicional que quieras ejecutar cuando el jugador entre en el trigger
        }

    }
}
