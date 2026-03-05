using UnityEngine;


public class MirarCamara : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam != null)
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward);
    }
}