using UnityEngine;

/// <summary>
/// Permite al cazador golpear sobrevivientes cercanos al presionar Interactuar.
/// Se adjunta automáticamente junto con PlayerController en el prefab del cazador.
/// </summary>
public class HunterAttack : MonoBehaviour
{
    [Header("Ataque")]
    public float radioAtaque = 1.5f;
    public float cooldown = 0.8f;

    private float tiempoUltimoAtaque = -99f;

    // Llamado desde PlayerController.OnInteractuar cuando el tag es Hunter
    public void Atacar()
    {
        if (Time.time - tiempoUltimoAtaque < cooldown) return;
        tiempoUltimoAtaque = Time.time;

        Collider[] hits = Physics.OverlapSphere(transform.position, radioAtaque);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Survivor"))
            {
                SurvivorHealth health = hit.GetComponent<SurvivorHealth>();
                if (health != null)
                {
                    health.RecibirGolpe();
                    Debug.Log($"[HunterAttack] Golpeó a {hit.name}");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioAtaque);
    }
}