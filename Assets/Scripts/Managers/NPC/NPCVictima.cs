using System.Collections;
using UnityEngine;

// ============================================================
//  NPCVictima.cs  —  v3
//
//  FLUJO:
//  1. Jugador entra al trigger  → se activa la NARRACIÓN sola
//  2. Narración termina         → aparece el ícono "!" y el aviso "Presiona E"
//  3. Jugador presiona E        → NPC voltea + inicia DIÁLOGO
// ============================================================

public class NPCVictima : MonoBehaviour
{
    [Header("Líneas de NARRACIÓN (contexto, automático al acercarse)")]
    public DialogueLine[] lineasNarracion;

    [Header("Líneas de DIÁLOGO (personaje habla, al presionar E)")]
    public DialogueLine[] lineasDialogo;

    [Header("Interacción")]
    public KeyCode teclaInteraccion = KeyCode.E;
    public GameObject iconoInteraccion;   // El "!" flotante
    public GameObject avisoPresionaE;     // UI texto "Presiona E para hablar" (opcional)

    [Header("Volteo al hablar")]
    public float velocidadVolteo = 5f;

    // ── Estado interno ────────────────────────────────────────────
    private bool jugadorCerca      = false;
    private bool narracionMostrada = false;
    private bool dialogoHecho      = false;
    private bool dialogoActivo     = false;
    private Transform jugador;
    private Quaternion rotacionOriginal;
    private Coroutine corrutinVolteo;

    void Start()
    {
        rotacionOriginal = transform.rotation;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) jugador = playerObj.transform;

        // Ocultar íconos al inicio
        if (iconoInteraccion != null) iconoInteraccion.SetActive(false);
        if (avisoPresionaE   != null) avisoPresionaE.SetActive(false);
    }

    void Update()
    {
        // Mantener volteo mientras el diálogo está activo
        if (dialogoActivo && jugador != null)
            VoltearHaciaJugador();

        // Esperar E para iniciar el diálogo (solo si la narración ya terminó)
        if (jugadorCerca && narracionMostrada && !dialogoHecho
            && Input.GetKeyDown(teclaInteraccion))
        {
            IniciarDialogo();
        }
    }

    // ── Trigger ───────────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorCerca = true;

        if (!narracionMostrada)
        {
            if (lineasNarracion != null && lineasNarracion.Length > 0)
            {
                // Hay narración: mostrarla primero, el ícono E sale al terminar
                DialogueSystem.Instance.IniciarDialogo(lineasNarracion, OnNarracionTerminada);
            }
            else
            {
                // Sin narración: saltar directo al ícono E
                narracionMostrada = true;
                if (!dialogoHecho) MostrarIconoInteraccion(true);
            }
        }
        else if (!dialogoHecho)
        {
            // Jugador salió y volvió: mostrar ícono directo
            MostrarIconoInteraccion(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorCerca = false;
        MostrarIconoInteraccion(false);
    }

    // ── Callbacks ─────────────────────────────────────────────────

    void OnNarracionTerminada()
    {
        narracionMostrada = true;
        // Ahora sí mostrar el ícono para que el jugador sepa que puede presionar E
        if (jugadorCerca)
            MostrarIconoInteraccion(true);
    }

    void IniciarDialogo()
    {
        dialogoHecho   = true;
        dialogoActivo  = true;
        MostrarIconoInteraccion(false);

        DialogueSystem.Instance.IniciarDialogo(lineasDialogo, OnDialogoTerminado);
    }

    void OnDialogoTerminado()
    {
        dialogoActivo = false;

        // Volver suavemente a rotación original
        if (corrutinVolteo != null) StopCoroutine(corrutinVolteo);
        corrutinVolteo = StartCoroutine(VolverRotacionOriginal());

        Debug.Log($"[{gameObject.name}] Diálogo terminado. Activar misión aquí.");
        // Ejemplo: MisionManager.Instance.ActivarMision(0);
    }

    // ── Volteo ────────────────────────────────────────────────────

    void VoltearHaciaJugador()
    {
        Vector3 dir = jugador.position - transform.position;
        dir.y = 0f;
        if (dir == Vector3.zero) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * velocidadVolteo
        );
    }

    IEnumerator VolverRotacionOriginal()
    {
        float t = 0f;
        Quaternion inicio = transform.rotation;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadVolteo * 0.5f;
            transform.rotation = Quaternion.Slerp(inicio, rotacionOriginal, t);
            yield return null;
        }
        transform.rotation = rotacionOriginal;
    }

    // ── UI helper ─────────────────────────────────────────────────

    void MostrarIconoInteraccion(bool mostrar)
    {
        if (iconoInteraccion != null) iconoInteraccion.SetActive(mostrar);
        if (avisoPresionaE   != null) avisoPresionaE.SetActive(mostrar);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2.5f);
    }
}