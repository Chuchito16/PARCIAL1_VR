using UnityEngine;
using UnityEngine.UI;


public class SurvivorHealth : MonoBehaviour
{
    [Header("Vida")]
    public int vidasMaximas = 3;
    public float invencibilidadTrasGolpe = 1.5f; 

    [Header("Feedback Visual")]
    public Image[] iconosVida;   
    public Color colorVidaLlena = Color.red;
    public Color colorVidaVacia = new Color(0.3f, 0.3f, 0.3f);

    
    public int VidasActuales { get; private set; }
    public bool EstaVivo { get; private set; } = true;

    private float tiempoUltimoGolpe = -99f;
    private PlayerController pc;

    private void Awake()
    {
        pc = GetComponent<PlayerController>();
        VidasActuales = vidasMaximas;
        ActualizarIconos();
    }

    
    public void RecibirGolpe()
    {
        if (!EstaVivo) return;
        if (Time.time - tiempoUltimoGolpe < invencibilidadTrasGolpe) return;

        tiempoUltimoGolpe = Time.time;
        VidasActuales--;
        ActualizarIconos();

        Debug.Log($"[SurvivorHealth] {gameObject.name} recibió golpe. Vidas: {VidasActuales}/{vidasMaximas}");

        if (VidasActuales <= 0)
            Morir();
    }

    
    private void Morir()
    {
        EstaVivo = false;
        pc?.Morir();
        Debug.Log($"[SurvivorHealth] {gameObject.name} ha muerto.");

        
        ExitDoor puerta = FindFirstObjectByType<ExitDoor>();
        puerta?.NotificarSobrevivienteMuerto();
    }

    
    public void Revivir()
    {
        EstaVivo = true;
        VidasActuales = vidasMaximas;
        pc?.Revivir();
        ActualizarIconos();
        Debug.Log($"[SurvivorHealth] {gameObject.name} revivido.");
    }

    
    private void ActualizarIconos()
    {
        if (iconosVida == null) return;
        for (int i = 0; i < iconosVida.Length; i++)
        {
            if (iconosVida[i] == null) continue;
            iconosVida[i].color = (i < VidasActuales) ? colorVidaLlena : colorVidaVacia;
        }
    }
}