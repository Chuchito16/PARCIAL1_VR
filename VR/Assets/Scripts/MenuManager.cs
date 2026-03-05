using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelMain;
    public GameObject panelOptions;
    public GameObject panelCreditos;

    void Start()
    {
        MostrarPanel(panelMain);
    }


    public void Jugar()
    {
        SceneManager.LoadScene("Level01");
    }


    public void AbrirOptions()
    {
        MostrarPanel(panelOptions);
    }

    public void AbrirCreditos()
    {
        MostrarPanel(panelCreditos);
    }

 
    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }


    public void Volver()
    {
        MostrarPanel(panelMain);
    }


    private void MostrarPanel(GameObject panelActivo)
    {
        panelMain.SetActive(false);
        panelOptions.SetActive(false);
        panelCreditos.SetActive(false);
        panelActivo.SetActive(true);
    }
}