using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Hace al sobreviviente "sordo":
///   - Desactiva su AudioListener
///   - Opcionalmente añade UI de icono de "sin audio"
///
/// En un juego local split-screen con múltiples AudioListeners,
/// Unity emite advertencia; puedes usar AudioMixerGroups por jugador
/// o simplemente deshabilitar el listener del sobreviviente.
/// </summary>
public class SurvivorDeafness : MonoBehaviour
{
    [Header("Opciones")]
    [Tooltip("Si true, desactiva completamente el AudioListener de la cámara del jugador.")]
    public bool desactivarAudioListener = true;

    [Tooltip("Mixer group exclusivo para audio del cazador (opcional).")]
    public AudioMixerGroup hunterAudioGroup;

    private AudioListener audioListener;
    private GameObject deafIcon;

    /// <summary>
    /// Llamado por GameRoleManager al registrar al sobreviviente.
    /// </summary>
    public void Inicializar(GameObject jugador)
    {
        // Buscar AudioListener en la cámara hija
        audioListener = jugador.GetComponentInChildren<AudioListener>();

        if (audioListener != null && desactivarAudioListener)
        {
            audioListener.enabled = false;
            Debug.Log($"[SurvivorDeafness] AudioListener desactivado en {jugador.name}");
        }

        MostrarIconoSordera(jugador);
    }

    // ── UI: ícono "sin audio" ─────────────────────────────────────────────────
    private void MostrarIconoSordera(GameObject jugador)
    {
        // Buscar Canvas del jugador (si existe un HUD por jugador)
        Canvas hud = jugador.GetComponentInChildren<Canvas>();
        if (hud == null) return;

        deafIcon = new GameObject("DeafIcon");
        deafIcon.transform.SetParent(hud.transform, false);

        UnityEngine.UI.Image img = deafIcon.AddComponent<UnityEngine.UI.Image>();

        // Cuadrado rojo en esquina superior derecha como indicador
        img.color = new Color(0.8f, 0.1f, 0.1f, 0.75f);

        RectTransform rect = deafIcon.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-10f, -10f);
        rect.sizeDelta = new Vector2(30f, 30f);

        // Texto "🔇" encima
        GameObject textGO = new GameObject("DeafLabel");
        textGO.transform.SetParent(deafIcon.transform, false);
        var label = textGO.AddComponent<UnityEngine.UI.Text>();
        label.text = "X";
        label.fontSize = 18;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    // ── Restaurar audio (por si acaso se usa un power-up o evento) ────────────
    public void RestaurarAudio()
    {
        if (audioListener != null)
            audioListener.enabled = true;

        if (deafIcon != null)
            deafIcon.SetActive(false);

        Debug.Log($"[SurvivorDeafness] Audio restaurado en {gameObject.name}");
    }
}