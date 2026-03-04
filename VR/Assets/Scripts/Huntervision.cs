using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Se adjunta automáticamente a la cámara del Cazador.
/// Reduce el FOV y agrega un overlay oscuro con hueco central
/// para simular "visión de túnel" / visión reducida.
/// </summary>
[RequireComponent(typeof(Camera))]
public class HunterVision : MonoBehaviour
{
    [Header("Visión de Túnel")]
    [Tooltip("FOV reducido del cazador (normal ~60)")]
    public float fovReducido = 45f;

    [Tooltip("Qué tan oscuro es el borde (0 = transparente, 1 = negro total)")]
    [Range(0f, 1f)]
    public float intensidadVignette = 0.85f;

    [Tooltip("Radio del área visible en el centro (0.0 - 1.0)")]
    [Range(0.1f, 0.9f)]
    public float radioVisible = 0.35f;

    [Tooltip("Suavizado del borde de la vignette")]
    [Range(0.01f, 0.5f)]
    public float suavizado = 0.15f;

    // ── Internos ──────────────────────────────────────────────────────────────
    private Camera cam;
    private Canvas canvas;
    private RawImage vignetteImage;
    private Texture2D vignetteTex;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = fovReducido;

        CrearVignetteUI();
    }

    // ── Crear overlay de vignette via UI Canvas ───────────────────────────────
    private void CrearVignetteUI()
    {
        // Canvas en espacio de screen overlay, ligado a esta cámara
        GameObject canvasGO = new GameObject("HunterVignette");
        canvasGO.transform.SetParent(transform);

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Imagen que cubre toda la pantalla
        GameObject imgGO = new GameObject("VignetteImg");
        imgGO.transform.SetParent(canvasGO.transform, false);

        vignetteImage = imgGO.AddComponent<RawImage>();
        RectTransform rect = vignetteImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GenerarTextura();
        vignetteImage.texture = vignetteTex;
        vignetteImage.color = new Color(1f, 1f, 1f, intensidadVignette);
    }

    // ── Generar textura de vignette (radial gradient) ─────────────────────────
    private void GenerarTextura()
    {
        int res = 256;
        vignetteTex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        vignetteTex.wrapMode = TextureWrapMode.Clamp;
        vignetteTex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[res * res];
        Vector2 center = new Vector2(0.5f, 0.5f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                Vector2 uv = new Vector2((float)x / (res - 1), (float)y / (res - 1));
                float dist = Vector2.Distance(uv, center);

                // Fuera del radio → negro; dentro → transparente
                float alpha = Mathf.InverseLerp(radioVisible, radioVisible + suavizado, dist);
                pixels[y * res + x] = new Color(0f, 0f, 0f, alpha);
            }
        }

        vignetteTex.SetPixels(pixels);
        vignetteTex.Apply();
    }

    // ── Permite ajustar en runtime ────────────────────────────────────────────
    public void SetIntensidad(float valor)
    {
        intensidadVignette = Mathf.Clamp01(valor);
        if (vignetteImage != null)
            vignetteImage.color = new Color(1f, 1f, 1f, intensidadVignette);
    }

    public void SetRadioVisible(float radio)
    {
        radioVisible = Mathf.Clamp(radio, 0.1f, 0.9f);
        GenerarTextura();
        if (vignetteImage != null)
            vignetteImage.texture = vignetteTex;
    }

    private void OnDestroy()
    {
        if (vignetteTex != null) Destroy(vignetteTex);
        if (canvas != null) Destroy(canvas.gameObject);
    }
}