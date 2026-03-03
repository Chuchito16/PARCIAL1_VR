using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelMenu;
    public GameObject panelCreditos;
    public GameObject panelOpciones;

    [Header("Audio")]
    public AudioSource musicaFondo;
    public Slider sliderVolumen;

    void Start()
    {
        panelMenu.SetActive(true);
        panelCreditos.SetActive(false);
        panelOpciones.SetActive(false);

        if (sliderVolumen != null)
        {
            sliderVolumen.value = musicaFondo.volume;
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
        }
    }

    // ▶️ INICIAR JUEGO
    public void IniciarJuego()
    {
        SceneManager.LoadScene("NombreDeTuEscenaDelJuego");
    }

    // 🎬 MOSTRAR CREDITOS
    public void MostrarCreditos()
    {
        panelMenu.SetActive(false);
        panelCreditos.SetActive(true);
    }

    // ⚙️ MOSTRAR OPCIONES
    public void MostrarOpciones()
    {
        panelMenu.SetActive(false);
        panelOpciones.SetActive(true);
    }

    // 🔙 VOLVER AL MENU
    public void VolverMenu()
    {
        panelMenu.SetActive(true);
        panelCreditos.SetActive(false);
        panelOpciones.SetActive(false);
    }

    // 🔊 CAMBIAR VOLUMEN
    public void CambiarVolumen(float valor)
    {
        musicaFondo.volume = valor;
    }

    // ❌ SALIR
    public void Salir()
    {
        Application.Quit();
        Debug.Log("Juego cerrado");
    }
}