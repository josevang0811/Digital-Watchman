using UnityEngine;
using TMPro; // Namespace de TextMeshPro — necesario para usar TMP_Text

// ============================================================
// HUDManager.cs
// ============================================================
// Controla toda la interfaz visible durante el juego.
// Se actualiza cada frame para reflejar el estado actual
// del jugador, la misión y el puntaje.
//
// Usa TextMeshPro (TMP) que es el sistema de texto moderno
// de Unity — más flexible y legible que el Text legacy.
// ============================================================

public class HUDManager : MonoBehaviour
{
    // Singleton — un solo HUD en toda la escena
    public static HUDManager Instance;

    [Header("Referencias UI")]
    // Estos campos se llenan arrastrando los objetos
    // de texto desde la Hierarchy al Inspector
    public TMP_Text textVida;       // Muestra "Vida: 70/100"
    public TMP_Text textMision;     // Muestra título y objetivo
    public TMP_Text textPuntaje;    // Muestra puntaje total

    [Header("Referencias Juego")]
    public PlayerHealth playerHealth; // Para leer la vida actual

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // Actualizamos el HUD cada frame para que siempre
        // refleje el estado más reciente del juego
        ActualizarVida();
        ActualizarMision();
        ActualizarPuntaje();
    }

    // ----------------------------------------------------------
    // ActualizarVida() — muestra la vida actual del jugador
    // ----------------------------------------------------------
    void ActualizarVida()
    {
        if (playerHealth == null || textVida == null) return;

        float vidaActual = playerHealth.GetVidaActual();
        float vidaMaxima = playerHealth.GetVidaMaxima();

        textVida.text = "Vida: " + vidaActual + " / " + vidaMaxima;

        // Cambiamos el color según la vida restante
        // Verde = sano, Amarillo = herido, Rojo = crítico
        if (vidaActual > vidaMaxima * 0.6f)
            textVida.color = Color.green;
        else if (vidaActual > vidaMaxima * 0.3f)
            textVida.color = Color.yellow;
        else
            textVida.color = Color.red;
    }

    // ----------------------------------------------------------
    // ActualizarMision() — muestra la misión activa y progreso
    // ----------------------------------------------------------
    void ActualizarMision()
    {
        if (textMision == null) return;

        // Obtenemos la misión actual del MissionManager
        Mision mision = MissionManager.Instance?.GetMisionActual();

        if (mision == null)
        {
            textMision.text = "¡Juego Completado!";
            return;
        }

        // Mostramos título, objetivo y progreso
        textMision.text =  mision.titulo + "\n" +
                          mision.objetivo + "\n" +
                          "(" + mision.enemigosEliminados +
                          "/" + mision.enemigosRequeridos + ")";
    }

    // ----------------------------------------------------------
    // ActualizarPuntaje() — muestra el puntaje acumulado
    // ----------------------------------------------------------
    void ActualizarPuntaje()
    {
        if (textPuntaje == null) return;

        int puntaje = MissionManager.Instance?.GetPuntaje() ?? 0;
        textPuntaje.text = "Puntaje: " + puntaje;
    }

    // ----------------------------------------------------------
    // MostrarMensaje() — muestra un mensaje temporal en pantalla
    // Se llama desde otros scripts para notificar al jugador
    // Ejemplo: "¡Misión completada!" o "¡Item recogido!"
    // ----------------------------------------------------------
    public void MostrarMensaje(string mensaje)
    {
        Debug.Log("HUD: " + mensaje);
        // Más adelante conectamos esto a un texto temporal en pantalla
    }
}