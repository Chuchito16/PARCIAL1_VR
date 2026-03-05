using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    [Header("Máquinas requeridas")]
    public MachineRepair[] maquinas;

    [Header("Puerta")]
    [Tooltip("GameObject de la puerta brillante — se activa al reparar todas las máquinas")]
    public GameObject puertaVisual;
    public Light luzPuerta;

    [Header("Siguiente nivel")]
    public string nombreSiguienteNivel = "Level02";
    public float esperaAntesDeCarga = 2f;

    [Header("UI")]
    public GameObject promptEntrada; // "¡Salida desbloqueada!" (opcional)

    // ── Estado ────────────────────────────────────────────────────────────────
    public bool EstaAbierta { get; private set; } = false;

    private int maquinasReparadas = 0;
    private int sobrevivientesVivos = 0;
    private int sobrevivientesCruzaron = 0;

    private readonly HashSet<GameObject> yaEntraron = new();

    // ── Init ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (puertaVisual != null) puertaVisual.SetActive(false);
        if (promptEntrada != null) promptEntrada.SetActive(false);
        if (luzPuerta != null) luzPuerta.color = Color.red;
    }

    private void Start()
    {
        foreach (var m in maquinas)
            if (m != null) m.OnReparada += ManejarMaquinaReparada;

        RecalcularSobrevivientesVivos();
        Debug.Log($"[ExitDoor] Esperando {maquinas.Length} máquinas. Sobrevivientes: {sobrevivientesVivos}");
    }

    // ── Llamado desde SurvivorHealth ──────────────────────────────────────────
    public void NotificarSobrevivienteMuerto()
    {
        RecalcularSobrevivientesVivos();
        Debug.Log($"[ExitDoor] Sobreviviente muerto. Vivos: {sobrevivientesVivos}");
        if (EstaAbierta) VerificarCondicionVictoria();
    }

    private void RecalcularSobrevivientesVivos()
    {
        sobrevivientesVivos = 0;
        foreach (var s in FindObjectsByType<SurvivorHealth>(FindObjectsSortMode.None))
            if (s.EstaVivo) sobrevivientesVivos++;
    }

    // ── Máquina reparada ──────────────────────────────────────────────────────
    private void ManejarMaquinaReparada(MachineRepair m)
    {
        maquinasReparadas++;
        Debug.Log($"[ExitDoor] Máquinas: {maquinasReparadas}/{maquinas.Length}");
        if (maquinasReparadas >= maquinas.Length) Abrir();
    }

    // ── Abrir puerta ──────────────────────────────────────────────────────────
    private void Abrir()
    {
        if (EstaAbierta) return;
        EstaAbierta = true;

        if (puertaVisual != null) puertaVisual.SetActive(true);
        if (luzPuerta != null) luzPuerta.color = new Color(0.4f, 0.8f, 1f);
        if (promptEntrada != null) promptEntrada.SetActive(true);

        RecalcularSobrevivientesVivos();
        Debug.Log($"[ExitDoor] ¡Puerta abierta! Necesitan cruzar: {sobrevivientesVivos}");
    }

    // ── Sobreviviente entra ───────────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (!EstaAbierta) return;
        if (!other.CompareTag("Survivor")) return;
        if (yaEntraron.Contains(other.gameObject)) return;

        SurvivorHealth health = other.GetComponent<SurvivorHealth>();
        if (health != null && !health.EstaVivo) return;

        yaEntraron.Add(other.gameObject);
        sobrevivientesCruzaron++;
        Debug.Log($"[ExitDoor] {other.name} cruzó ({sobrevivientesCruzaron}/{sobrevivientesVivos})");

        VerificarCondicionVictoria();
    }

    private void VerificarCondicionVictoria()
    {
        if (sobrevivientesCruzaron >= sobrevivientesVivos && sobrevivientesVivos > 0)
        {
            Debug.Log("[ExitDoor] ¡Todos cruzaron! Cargando siguiente nivel...");
            StartCoroutine(CargarSiguienteNivel());
        }
    }

    private IEnumerator CargarSiguienteNivel()
    {
        // Revivir muertos antes de cambiar de escena
        foreach (var s in FindObjectsByType<SurvivorHealth>(FindObjectsSortMode.None))
            if (!s.EstaVivo) s.Revivir();

        yield return new WaitForSeconds(esperaAntesDeCarga);
        SceneManager.LoadScene(nombreSiguienteNivel);
    }

    private void OnDestroy()
    {
        foreach (var m in maquinas)
            if (m != null) m.OnReparada -= ManejarMaquinaReparada;
    }
}