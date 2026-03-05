using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class HunterVision : MonoBehaviour
{
    [Header("FOV")]
    public float fovReducido = 65f;

    [Header("Niebla")]
    [Range(0f, 0.3f)] public float radioVisible = 0.12f;
    [Range(0.1f, 0.5f)] public float difusion = 0.30f;
    [Range(0f, 0.6f)] public float neblinaCentro = 0.25f;
    [Range(0f, 1f)] public float intensidadRuido = 0.35f;

    [Header("Animación")]
    public float velocidadPulso = 0.4f;
    public float velocidadDesplaz = 0.08f;
    [Range(0f, 0.04f)] public float amplitudPulso = 0.025f;

    private Camera cam;
    private Canvas canvas;
    private RawImage img;
    private Texture2D tex;
    private float offsetX, offsetY;

    private const int RES = 256;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = fovReducido;
        LimpiarAnteriores();
        Construir();
    }

    private void Update()
    {
        // Actualizar posición y tamaño del overlay para que coincida
        // exactamente con el viewport de ESTA cámara en pantalla
        ActualizarRectOverlay();

        offsetX += velocidadDesplaz * Time.deltaTime * 0.7f;
        offsetY += velocidadDesplaz * Time.deltaTime * 0.4f;

        float pulso = Mathf.Sin(Time.time * velocidadPulso * Mathf.PI * 2f);
        GenerarTextura(radioVisible + pulso * amplitudPulso);
        img.texture = tex;
    }

    // ── Ajustar el RectTransform al viewport de esta cámara ───────────────────
    private void ActualizarRectOverlay()
    {
        Rect vp = cam.rect; // valores 0..1 del viewport en pantalla

        float sw = Screen.width;
        float sh = Screen.height;

        // Convertir de viewport (0..1) a píxeles de pantalla
        float px = vp.x * sw;
        float py = vp.y * sh;
        float pw = vp.width * sw;
        float ph = vp.height * sh;

        // El Canvas ScreenSpaceOverlay usa coordenadas de píxel directamente
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;
        rt.anchoredPosition = new Vector2(px, py);
        rt.sizeDelta = new Vector2(pw, ph);
    }

    private void Construir()
    {
        GameObject go = new GameObject("HunterFog");
        // Hijo de la escena, NO de la cámara, para que no se duplique
        go.transform.SetParent(null);

        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();

        GameObject imgGO = new GameObject("FogImg");
        imgGO.transform.SetParent(go.transform, false);

        img = imgGO.AddComponent<RawImage>();
        img.color = Color.white;

        // Posición inicial
        ActualizarRectOverlay();

        GenerarTextura(radioVisible);
        img.texture = tex;
    }

    private void GenerarTextura(float radio)
    {
        if (tex != null) Destroy(tex);

        tex = new Texture2D(RES, RES, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[RES * RES];
        float noiseScale = 3.5f;

        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                float uvx = (float)x / (RES - 1) - 0.5f;
                float uvy = (float)y / (RES - 1) - 0.5f;
                float dist = Mathf.Sqrt(uvx * uvx + uvy * uvy) * 2f;

                float gradiente = Mathf.InverseLerp(radio, radio + difusion, dist);
                float alphaBase = Mathf.Lerp(neblinaCentro, 1f, gradiente);

                float nx = (float)x / RES * noiseScale + offsetX;
                float ny = (float)y / RES * noiseScale + offsetY;
                float ruido = Mathf.PerlinNoise(nx, ny);
                float peso = Mathf.Sin(gradiente * Mathf.PI) * intensidadRuido;
                float alpha = Mathf.Clamp01(alphaBase + (ruido - 0.5f) * peso);

                float nx2 = (float)x / RES * noiseScale * 2.3f - offsetY * 0.6f;
                float ny2 = (float)y / RES * noiseScale * 2.3f + offsetX * 0.6f;
                float ruido2 = Mathf.PerlinNoise(nx2, ny2);
                float peso2 = Mathf.Sin(gradiente * Mathf.PI * 0.8f) * intensidadRuido * 0.4f;
                alpha = Mathf.Clamp01(alpha + (ruido2 - 0.5f) * peso2);

                pixels[y * RES + x] = new Color(0f, 0f, 0f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
    }

    private void LimpiarAnteriores()
    {
        foreach (var c in GetComponentsInChildren<Canvas>())
            Destroy(c.gameObject);

        // Buscar canvas huérfanos de versiones anteriores por nombre
        foreach (string n in new[]{ "HunterFog", "HunterVignette",
                                     "HunterDarknessOverlay" })
        {
            GameObject viejo = GameObject.Find(n);
            if (viejo != null) Destroy(viejo);
        }

        if (transform.parent != null)
        {
            foreach (string n in new[] { "HunterProximityLight" })
            {
                Transform t = transform.parent.Find(n);
                if (t != null) Destroy(t.gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        if (tex != null) Destroy(tex);
        if (canvas != null) Destroy(canvas.gameObject);
    }
}