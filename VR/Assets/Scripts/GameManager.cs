using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ponlo en el mismo GameObject que tiene el PlayerInputManager.
/// Se encarga de reaccionar cuando un jugador se une o se va.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Colores para identificar a cada jugador (opcional, útil para debug)
    [SerializeField]
    private Color[] coloresJugadores = new Color[]
    {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow
    };

    // El PlayerInputManager llama este método automáticamente
    // cuando un nuevo jugador se une (nuevo control detectado)
    public void OnPlayerJoined(PlayerInput nuevoJugador)
    {
        int index = nuevoJugador.playerIndex;
        Debug.Log($"[GameManager] Jugador {index} se unió con dispositivo: {nuevoJugador.devices[0].displayName}");

        // Opcional: cambia el color del mesh del jugador para diferenciarlo
        Renderer meshRenderer = nuevoJugador.GetComponentInChildren<Renderer>();
        if (meshRenderer != null && index < coloresJugadores.Length)
            meshRenderer.material.color = coloresJugadores[index];

        // Opcional: coloca al jugador en un spawn point según su índice
        SpawnEnPosicion(nuevoJugador.gameObject, index);
    }

    // El PlayerInputManager llama este método automáticamente
    // cuando un jugador se desconecta
    public void OnPlayerLeft(PlayerInput jugadorSaliente)
    {
        Debug.Log($"[GameManager] Jugador {jugadorSaliente.playerIndex} se desconectó.");
    }

    private void SpawnEnPosicion(GameObject jugador, int index)
    {
        // Busca GameObjects llamados "SpawnPoint0", "SpawnPoint1", etc.
        // Si no existen simplemente los separa en el eje X
        GameObject spawnPoint = GameObject.Find($"SpawnPoint{index}");
        if (spawnPoint != null)
            jugador.transform.position = spawnPoint.transform.position;
        else
            jugador.transform.position = new Vector3(index * 3f, 1f, 0f);
    }
}