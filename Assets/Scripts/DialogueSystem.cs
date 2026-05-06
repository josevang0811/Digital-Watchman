using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
//  DIGITAL VIGILANTE — Sistema de Diálogo
//  Controla dos modos:
//    1. NARRACIÓN  → solo texto + botón Vale (pantallas de contexto)
//    2. DIÁLOGO    → foto del personaje + burbuja de texto
//  Avance: clic izquierdo O tecla Espacio / Enter
// ============================================================

[System.Serializable]
public class DialogueLine
{
    [Header("Contenido")]
    [TextArea(2, 5)]
    public string texto;

    [Header("Modo")]
    public bool esNarracion = false;   // true = solo texto, sin personaje

    [Header("Personaje (solo si esNarracion = false)")]
    public string nombrePersonaje = "";
    public Sprite fotoPersonaje;       // Arrastra la foto aquí en el Inspector
}

[System.Serializable]
public class DialogueSequence
{
    public string nombreSecuencia;     // Ej: "Misión 1 - Intro"
    public DialogueLine[] lineas;
}

public class DialogueSystem : MonoBehaviour
{
    // ── Referencia a los dos paneles del Canvas ──────────────────
    [Header("Panel de NARRACIÓN (solo texto + Vale)")]
    public GameObject panelNarracion;
    public TextMeshProUGUI textoNarracion;
    public Button botonValeNarracion;

    [Header("Panel de DIÁLOGO (personaje + burbuja)")]
    public GameObject panelDialogo;
    public Image fotoPersonajeUI;
    public TextMeshProUGUI nombrePersonajeUI;
    public TextMeshProUGUI textoDialogo;

    [Header("Efecto typewriter")]
    public float velocidadLetras = 0.03f;  // Segundos entre cada letra

    // ── Estado interno ────────────────────────────────────────────
    private DialogueLine[] lineasActuales;
    private int indiceActual = 0;
    public bool dialogoActivo = false;
    private bool escribiendo = false;
    private string textoCompleto = "";
    private System.Action callbackAlTerminar;

    // ── Singleton simple para acceso global ──────────────────────
    public static DialogueSystem Instance;

    void Awake()
    {
        Instance = this;
        panelNarracion.SetActive(false);
        panelDialogo.SetActive(false);
    }

    void Update()
    {
        if (!dialogoActivo) return;

        // Clic izquierdo O Espacio O Enter para avanzar
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (escribiendo)
            {
                // Si está escribiendo → mostrar todo de golpe
                TerminarEscritura();
            }
            else
            {
                // Ya terminó de escribir → avanzar a la siguiente línea
                SiguienteLinea();
            }
        }
    }

    // ── API pública ───────────────────────────────────────────────

    /// <summary>
    /// Inicia una secuencia de diálogo.
    /// Llama esto desde cualquier otro script.
    /// Ejemplo: DialogueSystem.Instance.IniciarDialogo(miSecuencia.lineas, OnDialogoTerminado);
    /// </summary>
    public void IniciarDialogo(DialogueLine[] lineas, System.Action alTerminar = null)
    {
        lineasActuales = lineas;
        indiceActual = 0;
        callbackAlTerminar = alTerminar;
        dialogoActivo = true;

        // Pausar movimiento del jugador mientras habla
        Time.timeScale = 1f; // Mantener tiempo normal; pausa solo el input del jugador
        NotificarJugador(true);

        MostrarLineaActual();
    }

    // ── Lógica interna ────────────────────────────────────────────

    void MostrarLineaActual()
    {
        if (indiceActual >= lineasActuales.Length)
        {
            CerrarDialogo();
            return;
        }

        DialogueLine linea = lineasActuales[indiceActual];

        if (linea.esNarracion)
        {
            panelDialogo.SetActive(false);
            panelNarracion.SetActive(true);
            StartCoroutine(EscribirTexto(textoNarracion, linea.texto));
        }
        else
        {
            panelNarracion.SetActive(false);
            panelDialogo.SetActive(true);

            // Foto y nombre
            fotoPersonajeUI.sprite = linea.fotoPersonaje;
            fotoPersonajeUI.gameObject.SetActive(linea.fotoPersonaje != null);
            nombrePersonajeUI.text = linea.nombrePersonaje;

            StartCoroutine(EscribirTexto(textoDialogo, linea.texto));
        }
    }

    IEnumerator EscribirTexto(TextMeshProUGUI campoTexto, string texto)
    {
        escribiendo = true;
        textoCompleto = texto;
        campoTexto.text = "";

        foreach (char letra in texto)
        {
            campoTexto.text += letra;
            yield return new WaitForSeconds(velocidadLetras);
        }

        escribiendo = false;
    }

    void TerminarEscritura()
    {
        StopAllCoroutines();
        escribiendo = false;

        // Mostrar el texto completo inmediatamente
        DialogueLine linea = lineasActuales[indiceActual];
        if (linea.esNarracion)
            textoNarracion.text = textoCompleto;
        else
            textoDialogo.text = textoCompleto;
    }

    void SiguienteLinea()
    {
        indiceActual++;
        MostrarLineaActual();
    }

    void CerrarDialogo()
    {
        panelNarracion.SetActive(false);
        panelDialogo.SetActive(false);
        dialogoActivo = false;
        NotificarJugador(false);
        Time.timeScale = 1f;
    callbackAlTerminar?.Invoke();  // Llama al callback si existe
    }

    // Congela/descongela el input del jugador durante el diálogo
    void NotificarJugador(bool dialogoAbierto)
    {
        // Busca el script de movimiento y lo desactiva temporalmente
        PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
        if (pm != null) pm.enabled = !dialogoAbierto;
    }
}