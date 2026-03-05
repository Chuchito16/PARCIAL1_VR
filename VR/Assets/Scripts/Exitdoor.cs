using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Puerta de salida que se abre cuando TODAS las máquinas están reparadas.
/// Al abrirse, los sobrevivientes pueden atravesarla para pasar al siguiente nivel.
///
/// Asigna todas las MachineRepair del nivel en el Inspector.
/// </summary>
public class ExitDoor : MonoBehaviour
{
    [Header("Máquinas requeridas")]
    [Tooltip("Arrastra aquí todas las MachineRepair del nivel")]
    public MachineRepair[] maquinas;

    [Header("Animación de puerta")]
    [Tooltip("Transform de la puerta que se moverá/girará al abrirse")]
    public Transform puertaTransform;
    public Vector3 posicionAbierta = new Vector3(0f, 3f, 0f);   // Offset relativo
    public float velocidadApertura = 1.5f;

    [Header("Siguiente nivel")]
    [Tooltip("Nombre exacto de la escena del siguiente nivel")]
    public string nombreSiguienteNivel = "Level02";
    [Tooltip("Segundos de espera antes de cargar la siguiente escena")]
    public float esperaAntesDeCarga = 2f;

    [Header("Feedback")]
    public AudioClip sonidoApertura;
    public AudioClip sonidoEntrada;
    public GameObject promptEntrada;   // "¡Salida desbloqueada!"
    public Light luzPuerta;

    // ── Estado ────────────────────────────────────────────────────────────────
    public bool EstaAbierta { get; private set; } = false;

    private int maquinasReparadas = 0;
    private Vector3 posicionCerrada;
    private AudioSource audioSource;

    // ── Init ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (puertaTransform != null)
            posicionCerrada = puertaTransform.localPosition;

        if (promptEntrada != null)
            promptEntrada.SetActive(false);

        if (luzPuerta != null)
            luzPuerta.color = Color.red;
    }

    private void Start()
    {
        // Suscribirse al evento de cada máquina
        foreach (var maquina in maquinas)
        {
            if (maquina != null)
                maquina.OnReparada += ManejarMaquinaReparada;
        }

        Debug.Log($"[ExitDoor] Esperando {maquinas.Length} máquinas.");
    }

    // ── Manejar evento de máquina reparada ────────────────────────────────────
    private void ManejarMaquinaReparada(MachineRepair maquina)
    {
        maquinasReparadas++;
        Debug.Log($"[ExitDoor] Máquinas reparadas: {maquinasReparadas}/{maquinas.Length}");

        if (maquinasReparadas >= maquinas.Length)
            Abrir();
    }

    // ── Abrir puerta ──────────────────────────────────────────────────────────
    private void Abrir()
    {
        if (EstaAbierta) return;
        EstaAbierta = true;

        Debug.Log("[ExitDoor] ¡Todas las máquinas reparadas! Abriendo puerta.");

        if (sonidoApertura != null)
            audioSource.PlayOneShot(sonidoApertura);

        if (promptEntrada != null)
            promptEntrada.SetActive(true);

        if (luzPuerta != null)
            luzPuerta.color = Color.green;

        if (puertaTransform != null)
            StartCoroutine(AnimarApertura());
    }

    private IEnumerator AnimarApertura()
    {
        Vector3 objetivo = posicionCerrada + posicionAbierta;
        while (Vector3.Distance(puertaTransform.localPosition, objetivo) > 0.01f)
        {
            puertaTransform.localPosition = Vector3.MoveTowards(
                puertaTransform.localPosition,
                objetivo,
                velocidadApertura * Time.deltaTime);
            yield return null;
        }
        puertaTransform.localPosition = objetivo;
    }

    // ── Trigger: sobreviviente entra a la puerta ──────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (!EstaAbierta) return;

        if (other.CompareTag("Survivor"))
        {
            Debug.Log($"[ExitDoor] {other.name} entró por la puerta. Cargando {nombreSiguienteNivel}...");

            if (sonidoEntrada != null)
                audioSource.PlayOneShot(sonidoEntrada);

            StartCoroutine(CargarSiguienteNivel());
        }
    }

    private IEnumerator CargarSiguienteNivel()
    {
        yield return new WaitForSeconds(esperaAntesDeCarga);
        SceneManager.LoadScene(nombreSiguienteNivel);
    }

    // ── Desuscribir al destruir ───────────────────────────────────────────────
    private void OnDestroy()
    {
        foreach (var maquina in maquinas)
            if (maquina != null)
                maquina.OnReparada -= ManejarMaquinaReparada;
    }
}