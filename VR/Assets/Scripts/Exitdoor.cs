using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    [Header("Required machines")]
    public MachineRepair[] maquinas;

    [Header("Door")]
    [Tooltip("Door visual object, activated when all machines are repaired")]
    public GameObject puertaVisual;
    public Light luzPuerta;

    [Header("Next level")]
    public string nombreSiguienteNivel = "Level02";
    public float esperaAntesDeCarga = 2f;

    [Header("UI")]
    public GameObject promptEntrada; 
    public bool EstaAbierta { get; private set; } = false;

    private int maquinasReparadas = 0;
    private int maquinasRequeridas = 0;

    private int sobrevivientesVivos = 0;
    private int sobrevivientesCruzaron = 0;

    private bool cargandoNivel = false;

    private readonly HashSet<GameObject> yaEntraron = new HashSet<GameObject>();

    private void Awake()
    {
        if (puertaVisual != null) puertaVisual.SetActive(false);
        if (promptEntrada != null) promptEntrada.SetActive(false);
        if (luzPuerta != null) luzPuerta.color = Color.red;
    }

    private void Start()
    {
        maquinasReparadas = 0;
        maquinasRequeridas = 0;

        if (maquinas == null || maquinas.Length == 0)
        {
            Debug.LogError("[ExitDoor] No machines assigned in inspector.");
            return;
        }

        foreach (var m in maquinas)
        {
            if (m == null) continue;

            maquinasRequeridas++;

           
            m.OnReparada += ManejarMaquinaReparada;

           
            if (m.EstaReparada) maquinasReparadas++;
        }

        Debug.Log("[ExitDoor] Machines required: " + maquinasRequeridas + ". Already repaired: " + maquinasReparadas);

        
        if (maquinasRequeridas > 0 && maquinasReparadas >= maquinasRequeridas)
            Abrir();

        RecalcularSobrevivientesVivos();
        Debug.Log("[ExitDoor] Survivors alive: " + sobrevivientesVivos);
    }

    public void NotificarSobrevivienteMuerto()
    {
        RecalcularSobrevivientesVivos();
        Debug.Log("[ExitDoor] Survivor died. Alive: " + sobrevivientesVivos);

        if (EstaAbierta) VerificarCondicionVictoria();
    }

    private void RecalcularSobrevivientesVivos()
    {
        sobrevivientesVivos = 0;

        foreach (var s in FindObjectsByType<SurvivorHealth>(FindObjectsSortMode.None))
        {
            if (s != null && s.EstaVivo) sobrevivientesVivos++;
        }
    }

    private void ManejarMaquinaReparada(MachineRepair m)
    {
        maquinasReparadas++;
        Debug.Log("[ExitDoor] Machines: " + maquinasReparadas + "/" + maquinasRequeridas);

        if (maquinasRequeridas > 0 && maquinasReparadas >= maquinasRequeridas)
            Abrir();
    }

    private void Abrir()
    {
        if (EstaAbierta) return;
        EstaAbierta = true;

        if (puertaVisual != null) puertaVisual.SetActive(true);
        else Debug.LogWarning("[ExitDoor] puertaVisual is NOT assigned.");

        if (luzPuerta != null) luzPuerta.color = new Color(0.4f, 0.8f, 1f);

        if (promptEntrada != null) promptEntrada.SetActive(true);

        RecalcularSobrevivientesVivos();
        Debug.Log("[ExitDoor] Door open! Need to cross: " + sobrevivientesVivos);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!EstaAbierta) return;
        if (other == null) return;
        if (!other.CompareTag("Survivor")) return;

        GameObject go = other.gameObject;
        if (yaEntraron.Contains(go)) return;

        SurvivorHealth health = other.GetComponent<SurvivorHealth>();
        if (health != null && !health.EstaVivo) return;

        yaEntraron.Add(go);
        sobrevivientesCruzaron++;

        // Recalcular SIEMPRE antes de verificar
        RecalcularSobrevivientesVivos();

        Debug.Log($"[ExitDoor] {other.name} cruzó ({sobrevivientesCruzaron}/{sobrevivientesVivos})");

        VerificarCondicionVictoria();
    }

    private void VerificarCondicionVictoria()
    {
        if (cargandoNivel) return;

        RecalcularSobrevivientesVivos();

        Debug.Log($"[ExitDoor] Verificando: cruzaron={sobrevivientesCruzaron}, vivos={sobrevivientesVivos}");

        // Si no hay survivors vivos pero alguien cruzó, avanzar igual
        if (sobrevivientesVivos <= 0 && sobrevivientesCruzaron > 0)
        {
            Debug.Log("[ExitDoor] Sin survivors vivos restantes, cargando nivel...");
            cargandoNivel = true;
            StartCoroutine(CargarSiguienteNivel());
            return;
        }

        if (sobrevivientesVivos > 0 && sobrevivientesCruzaron >= sobrevivientesVivos)
        {
            Debug.Log("[ExitDoor] ¡Todos cruzaron! Cargando siguiente nivel...");
            cargandoNivel = true;
            StartCoroutine(CargarSiguienteNivel());
        }
    }

    private IEnumerator CargarSiguienteNivel()
    {
      
        foreach (var s in FindObjectsByType<SurvivorHealth>(FindObjectsSortMode.None))
        {
            if (s != null && !s.EstaVivo) s.Revivir();
        }

        yield return new WaitForSeconds(esperaAntesDeCarga);

        // Scene name must exist in Build Settings
        SceneManager.LoadScene(nombreSiguienteNivel);
    }

    private void OnDestroy()
    {
        if (maquinas == null) return;

        foreach (var m in maquinas)
        {
            if (m != null) m.OnReparada -= ManejarMaquinaReparada;
        }
    }
}