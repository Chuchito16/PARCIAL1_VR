using UnityEngine;

/// <summary>
/// Adjunta este script al Canvas de la máquina.
/// Hace que el Canvas siempre mire hacia la cámara activa.
/// </summary>
public class MirarCamara : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam != null)
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward);
    }
}