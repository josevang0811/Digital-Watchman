using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// ============================================================
// MainMenuManager.cs
// ============================================================
// Maneja la lógica del menú principal:
// - Iniciar partida nueva (pide nombre)
// - Continuar última partida
// - Mostrar leaderboard
// ============================================================

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject PanelMenu;
    public GameObject PanelNombre;
    public GameObject PanelLeaderboard;

    [Header("Panel Nombre")]
    public TMP_InputField InputNombre;

    [Header("Panel Leaderboard")]
    public TextMeshProUGUI Entrada1;
    public TextMeshProUGUI Entrada2;
    public TextMeshProUGUI Entrada3;
    public TextMeshProUGUI Entrada4;
    public TextMeshProUGUI Entrada5;

    [Header("Mensaje de aviso")]
    public TextMeshProUGUI TextoAviso;

    // Nombre de la escena del juego — cámbialo por el tuyo
    private string escenaJuego = "SampleScene";

    void Start()
    {
        PanelMenu.SetActive(true);
        PanelNombre.SetActive(false);
        PanelLeaderboard.SetActive(false);

        if (TextoAviso != null)
            TextoAviso.gameObject.SetActive(false);
    }

    // ----------------------------------------------------------
    // INICIAR — muestra el panel para pedir el nombre
    // ----------------------------------------------------------
    public void OnClickIniciar()
    {
        PanelMenu.SetActive(false);
        PanelNombre.SetActive(true);
    }

    // ----------------------------------------------------------
    // CONFIRMAR NOMBRE — arranca el juego con el nombre dado
    // ----------------------------------------------------------
    public void OnClickConfirmar()
    {
        string nombre = InputNombre.text.Trim();

        if (string.IsNullOrEmpty(nombre))
        {
            MostrarAviso("Por favor ingresa un nombre.");
            return;
        }

        // Guardamos el identificador para usarlo en el juego
        PlayerPrefs.SetString("IdentificadorActual", nombre);
        PlayerPrefs.Save();

        // Borramos el guardado anterior para empezar limpio
        if (FileManager.Instance != null)
            FileManager.Instance.EliminarGuardado();

        SceneManager.LoadScene(escenaJuego);
    }

    // ----------------------------------------------------------
    // CONTINUAR — carga la última partida guardada
    // ----------------------------------------------------------
    public void OnClickContinuar()
    {
        if (FileManager.Instance == null)
        {
            MostrarAviso("Error al acceder al sistema de guardado.");
            return;
        }

        DatosGuardado datos = FileManager.Instance.Cargar();

        if (datos == null)
        {
            MostrarAviso("No hay partida guardada.");
            return;
        }

        // Restauramos el identificador del último guardado
        PlayerPrefs.SetString("IdentificadorActual", datos.nombreJugador);
        PlayerPrefs.Save();

        SceneManager.LoadScene(escenaJuego);
    }

    // ----------------------------------------------------------
    // LEADERBOARD — muestra el top 5
    // ----------------------------------------------------------
    public void OnClickLeaderboard()
    {
        PanelMenu.SetActive(false);
        PanelLeaderboard.SetActive(true);

        LeaderboardEntry[] entradas = FileManager.Instance != null
            ? FileManager.Instance.CargarLeaderboard()
            : new LeaderboardEntry[0];

        TextMeshProUGUI[] textos = { Entrada1, Entrada2, Entrada3, Entrada4, Entrada5 };

        for (int i = 0; i < textos.Length; i++)
        {
            if (i < entradas.Length && entradas[i] != null)
                textos[i].text = (i + 1) + ". " + entradas[i].identificador + " — " + entradas[i].puntaje + " pts";
            else
                textos[i].text = (i + 1) + ". ---";
        }
    }

    // ----------------------------------------------------------
    // VOLVER — regresa al panel principal
    // ----------------------------------------------------------
    public void OnClickVolver()
    {
        PanelLeaderboard.SetActive(false);
        PanelNombre.SetActive(false);
        PanelMenu.SetActive(true);
    }

    // ----------------------------------------------------------
    // MostrarAviso — muestra un mensaje temporal en pantalla
    // ----------------------------------------------------------
    private void MostrarAviso(string mensaje)
    {
        if (TextoAviso == null) return;
        TextoAviso.text = mensaje;
        TextoAviso.gameObject.SetActive(true);
        Invoke("OcultarAviso", 2f);
    }

    private void OcultarAviso()
    {
        if (TextoAviso != null)
            TextoAviso.gameObject.SetActive(false);
    }
}