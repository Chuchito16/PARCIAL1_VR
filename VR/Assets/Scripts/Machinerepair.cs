using System;
using UnityEngine;
using UnityEngine.UI;


public class MachineRepair : MonoBehaviour
{
    [Header("Reparación")]
    public float tiempoReparacion = 5f;
    [Tooltip("Distancia máxima para interactuar")]
    public float radioInteraccion = 2f;

    [Header("Feedback Visual")]
    public GameObject promptUI;
    public Slider barraProgreso;
    public MeshRenderer indicadorLuz;

    [Header("Highlight (Visible a través de paredes)")]
    public GameObject highlightVisual;   

    [Header("Audio")]
    public AudioClip sonidoReparando;
    public AudioClip sonidoCompletado;

    public bool EstaReparada { get; private set; } = false;

    private float progresoActual = 0f;
    private bool jugadorEnRango = false;
    private bool jugadorInteractuando = false;

    private AudioSource audioSource;

    public event Action<MachineRepair> OnReparada;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

   
        if (highlightVisual != null)
            highlightVisual.SetActive(true);

        ActualizarUI();
        ActualizarLuz(false);
    }


    private void Update()
    {
        if (EstaReparada) return;

        if (jugadorInteractuando && jugadorEnRango)
        {
            progresoActual += Time.deltaTime;

       
            if (sonidoReparando != null && !audioSource.isPlaying)
                audioSource.PlayOneShot(sonidoReparando);

            if (progresoActual >= tiempoReparacion)
                Completar();
        }
        else
        {
            progresoActual = Mathf.Max(0f, progresoActual - Time.deltaTime * 0.5f);

            if (audioSource.isPlaying)
                audioSource.Stop();
        }

        ActualizarUI();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Survivor"))
        {
            jugadorEnRango = true;

            if (promptUI != null)
                promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Survivor"))
        {
            jugadorEnRango = false;
            jugadorInteractuando = false;

            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    public void SetInteractuando(bool valor)
    {
        if (EstaReparada) return;
        jugadorInteractuando = valor;
    }


    private void Completar()
    {
        EstaReparada = true;
        progresoActual = tiempoReparacion;
        jugadorInteractuando = false;

        if (sonidoCompletado != null)
            audioSource.PlayOneShot(sonidoCompletado);

        ActualizarLuz(true);
        ActualizarUI();

        if (promptUI != null)
            promptUI.SetActive(false);

        if (highlightVisual != null)
            highlightVisual.SetActive(false);

        Debug.Log($"[MachineRepair] {gameObject.name} reparada.");

        OnReparada?.Invoke(this);
    }

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
            ? new Color(0.1f, 0.9f, 0.1f)   
            : new Color(0.9f, 0.1f, 0.1f);  
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}