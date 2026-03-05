using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Máquina reparable por los sobrevivientes.
/// El jugador debe mantenerse cerca y presionar Interactuar (E/South button)
/// durante 'tiempoReparacion' segundos para completarla.
///
/// Notifica a ExitDoor automáticamente cuando se repara.
/// </summary>
public class MachineRepair : MonoBehaviour
{
    [Header("Reparación")]
    public float tiempoReparacion = 5f;
    [Tooltip("Distancia máxima para interactuar")]
    public float radioInteraccion = 2f;

    [Header("Feedback Visual")]
    public GameObject promptUI;          // "Mantén E para reparar"
    public Slider barraProgreso;
    public MeshRenderer indicadorLuz;    // Cambia de rojo a verde al reparar

    [Header("Audio")]
    public AudioClip sonidoReparando;
    public AudioClip sonidoCompletado;

    // ── Estado ────────────────────────────────────────────────────────────────
    public bool EstaReparada { get; private set; } = false;

    private float progresoActual = 0f;
    private bool jugadorEnRango = false;
    private bool jugadorInteractuando = false;

    private AudioSource audioSource;

    // ── Evento ────────────────────────────────────────────────────────────────
    public event Action<MachineRepair> OnReparada;

    // ── Init ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        ActualizarUI();
        ActualizarLuz(false);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (EstaReparada) return;

        if (jugadorInteractuando && jugadorEnRango)
        {
            progresoActual += Time.deltaTime;

            // Sonido continuo de reparación
            if (sonidoReparando != null && !audioSource.isPlaying)
                audioSource.PlayOneShot(sonidoReparando);

            if (progresoActual >= tiempoReparacion)
                Completar();
        }
        else
        {
            // Decaimiento del progreso si se suelta (opcional: quitar si no quieres decaimiento)
            progresoActual = Mathf.Max(0f, progresoActual - Time.deltaTime * 0.5f);
            if (audioSource.isPlaying) audioSource.Stop();
        }

        ActualizarUI();
    }

    // ── Trigger de proximidad ─────────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Survivor"))
        {
            jugadorEnRango = true;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Survivor"))
        {
            jugadorEnRango = false;
            jugadorInteractuando = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }

    // ── Llamado desde PlayerController.OnInteractuar ──────────────────────────
    /// <summary>
    /// El PlayerController debe llamar este método cuando detecta
    /// que la máquina más cercana está en rango.
    /// </summary>
    public void SetInteractuando(bool valor)
    {
        if (EstaReparada) return;
        jugadorInteractuando = valor;
    }

    // ── Completar reparación ──────────────────────────────────────────────────
    private void Completar()
    {
        EstaReparada = true;
        progresoActual = tiempoReparacion;
        jugadorInteractuando = false;

        if (sonidoCompletado != null)
            audioSource.PlayOneShot(sonidoCompletado);

        ActualizarLuz(true);
        ActualizarUI();

        if (promptUI != null) promptUI.SetActive(false);

        Debug.Log($"[MachineRepair] {gameObject.name} reparada.");
        OnReparada?.Invoke(this);
    }

    // ── UI ────────────────────────────────────────────────────────────────────
    private void ActualizarUI()
    {
        if (barraProgreso != null)
        {
            barraProgreso.value = progresoActual / tiempoReparacion;
            barraProgreso.gameObject.SetActive(!EstaReparada && jugadorEnRango);
        }
    }

    private void ActualizarLuz(bool reparada)
    {
        if (indicadorLuz == null) return;
        indicadorLuz.material.color = reparada
            ? new Color(0.1f, 0.9f, 0.1f)   // Verde
            : new Color(0.9f, 0.1f, 0.1f);  // Rojo
    }

    // ── Gizmo de rango en editor ──────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}