using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

// ============================================================
// GameOverScreen.cs
// ============================================================
// Lógica de la pantalla de Game Over.
// Lee el ActionStack y muestra las últimas acciones del
// jugador usando el patrón LIFO — lo más reciente primero.
//
// Este script va en el objeto GameOverCanvas.
// El Canvas empieza desactivado y se activa al morir.
// ============================================================

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance;

    [Header("Referencias UI — asignar desde el Inspector")]
    public TextMeshProUGUI textoTitulo;       // "GAME OVER"
    public TextMeshProUGUI textoPuntaje;      // Puntaje final
    public TextMeshProUGUI textoAcciones;     // Historial del Stack
    public Button botonReiniciar;             // Botón Reiniciar
    [SerializeField] GameObject hud;              // Referencia al HUD para ocultarlo

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // El Canvas empieza oculto
        gameObject.SetActive(false);
    }

    void Start()
    {
        // Conectamos el botón al método Reiniciar
        if (botonReiniciar != null)
            botonReiniciar.onClick.AddListener(Reiniciar);
    }

    // ----------------------------------------------------------
    // Mostrar() — activa la pantalla y llena los datos
    // Llamado desde PlayerHealth.Die()
    // ----------------------------------------------------------
    public void Mostrar(int puntajeFinal)
    {
        if(hud!=null) hud.SetActive(false);

        // Activamos el Canvas
        gameObject.SetActive(true);

        // Pausamos el juego
        Time.timeScale = 0f;

        // Título
        if (textoTitulo != null)
            textoTitulo.text = "Fallaste en la mision";

        // Puntaje
        if (textoPuntaje != null)
            textoPuntaje.text = "Puntaje final: " + puntajeFinal;

        // ----------------------------------------------------------
        // STACK EN ACCIÓN: leemos las últimas acciones
        // ObtenerUltimas() hace Pop del Stack — LIFO —
        // la acción más reciente aparece primero en la lista
        // ----------------------------------------------------------
        if (textoAcciones != null && ActionStack.Instance != null)
        {
            List<string> ultimas = ActionStack.Instance.ObtenerUltimas(6);

            if (ultimas.Count == 0)
            {
                textoAcciones.text = "Sin acciones registradas.";
            }
            else
            {
                string contenido = "Tus últimas acciones:\n";
                foreach (string accion in ultimas)
                {
                    contenido += "→ " + accion + "\n";
                }
                textoAcciones.text = contenido;
            }
        }
    }

    // ----------------------------------------------------------
    // Reiniciar() — conectado al botón, recarga la escena
    // ----------------------------------------------------------
    void Reiniciar()
    {
        // Reanudamos el tiempo antes de recargar
        Time.timeScale = 1f;

        // Limpiamos el Stack
        if (ActionStack.Instance != null)
            ActionStack.Instance.Limpiar();

        // Recargamos la escena actual
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}