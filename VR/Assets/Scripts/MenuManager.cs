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

    // ─── BOTÓN: JUGAR ───────────────────────────────────────────
    public void Jugar()
    {
        SceneManager.LoadScene("Level01");
    }

    // ─── BOTÓN: OPTIONS ─────────────────────────────────────────
    public void AbrirOptions()
    {
        MostrarPanel(panelOptions);
    }

    // ─── BOTÓN: CRÉDITOS ────────────────────────────────────────
    public void AbrirCreditos()
    {
        MostrarPanel(panelCreditos);
    }

    // ─── BOTÓN: EXIT ────────────────────────────────────────────
    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // ─── BOTÓN: VOLVER (Back) ────────────────────────────────────
    public void Volver()
    {
        MostrarPanel(panelMain);
    }

    // ─── HELPER ──────────────────────────────────────────────────
    private void MostrarPanel(GameObject panelActivo)
    {
        panelMain.SetActive(false);
        panelOptions.SetActive(false);
        panelCreditos.SetActive(false);
        panelActivo.SetActive(true);
    }
}