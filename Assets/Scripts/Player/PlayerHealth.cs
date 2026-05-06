using System.Collections;
using UnityEngine;

// ============================================================
// PlayerHealth.cs — actualizado
// Nuevos efectos implementados:
// - Escudo temporal: reduce daño 50% por N segundos
// - Boost velocidad: aumenta speed por N segundos
// - Curación: restaura HP instantáneamente
//
// Los efectos temporales usan Coroutines — funciones que
// pueden pausarse con yield return sin congelar el juego.
// ============================================================
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de vida")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Configuración del escudo")]
    public float escudoMultiplicador = 0.5f; // 0.5 = reduce daño a la mitad

    // Estado de efectos activos
    private bool escudoActivo = false;
    private bool boostActivo  = false;

    // Referencia al PlayerMovement para el boost de velocidad
    private PlayerMovement playerMovement;

    void Start()
    {
        currentHealth  = maxHealth;
        playerMovement = GetComponent<PlayerMovement>();
    }

    // ----------------------------------------------------------
    // TakeDamage() — recibe daño con posible reducción del escudo
    // ----------------------------------------------------------
    public void TakeDamage(float damage)
    {
        // Si el escudo está activo reducimos el daño
        if (escudoActivo)
        {
            float danoOriginal = damage;
            damage *= escudoMultiplicador;
            Debug.Log("[ESCUDO] Daño reducido: " + danoOriginal + " → " + damage);
        }

        currentHealth -= damage;

        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Jugador recibió daño: " + damage + " HP");

        if (ActionStack.Instance != null)
            ActionStack.Instance.Registrar("Recibiste " + damage + " de daño");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        if (FileManager.Instance != null)
        {
            int puntaje = MissionManager.Instance != null ? MissionManager.Instance.GetPuntaje() : 0;
            int mision  = MissionManager.Instance?.GetMisionActual()?.id ?? 1;
            string nombre = PlayerPrefs.GetString("NombreJugador", "Jugador");
            FileManager.Instance.Guardar(nombre, puntaje, currentHealth, mision);
        }
    }

    // ----------------------------------------------------------
    // Curar() — restaura HP instantáneamente
    // ----------------------------------------------------------
    public void Curar(float cantidad)
    {
        currentHealth = Mathf.Min(currentHealth + cantidad, maxHealth);
        Debug.Log("Jugador curado. HP: " + currentHealth + "/" + maxHealth);

        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Jugador curado: +" + cantidad + " HP");

        if (ActionStack.Instance != null)
            ActionStack.Instance.Registrar("Te curaste +" + cantidad + " HP");
    }

    // ----------------------------------------------------------
    // ActivarEscudo() — inicia la Coroutine del escudo temporal
    // Llamado desde Inventario.AplicarEfecto()
    // ----------------------------------------------------------
    public void ActivarEscudo(float duracion)
    {
        // Si ya hay un escudo activo, lo reiniciamos
        StopCoroutine("EscudoTemporal");
        StartCoroutine(EscudoTemporal(duracion));
    }

    // ----------------------------------------------------------
    // EscudoTemporal() — Coroutine
    // yield return pausa la función sin congelar el juego.
    // El juego sigue corriendo normalmente mientras espera.
    // ----------------------------------------------------------
    IEnumerator EscudoTemporal(float duracion)
    {
        escudoActivo = true;
        Debug.Log("[ESCUDO] Activado por " + duracion + " segundos");

        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Escudo activado por " + duracion + "s");

        if (ActionStack.Instance != null)
            ActionStack.Instance.Registrar("Activaste el escudo (" + duracion + "s)");

        // Aquí Unity pausa esta función y continúa el juego
        yield return new WaitForSeconds(duracion);

        // Cuando pasan los segundos, continúa desde aquí
        escudoActivo = false;
        Debug.Log("[ESCUDO] Desactivado");

        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Escudo desactivado");
    }

    // ----------------------------------------------------------
    // ActivarBoostVelocidad() — aumenta la velocidad temporalmente
    // Llamado desde Inventario.AplicarEfecto()
    // ----------------------------------------------------------
    public void ActivarBoostVelocidad(float duracion)
    {
        StopCoroutine("BoostVelocidadTemporal");
        StartCoroutine(BoostVelocidadTemporal(duracion));
    }

    IEnumerator BoostVelocidadTemporal(float duracion)
    {
        if (playerMovement == null) yield break;

        boostActivo = true;
        float speedOriginal = playerMovement.speed;
        playerMovement.speed *= 4f; // duplicamos la velocidad

        Debug.Log("[BOOST] Velocidad aumentada a " + playerMovement.speed + " por " + duracion + "s");

        if (ActionStack.Instance != null)
            ActionStack.Instance.Registrar("Boost de velocidad activado (" + duracion + "s)");

        yield return new WaitForSeconds(duracion);

        playerMovement.speed = speedOriginal; // restauramos
        boostActivo = false;
        Debug.Log("[BOOST] Velocidad restaurada a " + playerMovement.speed);
    }

    // ----------------------------------------------------------
    // Die() — activa Game Over
    // ----------------------------------------------------------
    void Die()
    {
        Debug.Log("Game Over!");

        // Detenemos todos los efectos activos
        StopAllCoroutines();
        escudoActivo = false;
        boostActivo  = false;

        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Jugador derrotado. Game Over.");

        Mision misionActual = MissionManager.Instance?.GetMisionActual();
        if (misionActual != null)
            misionActual.estado = Mision.EstadoMision.Fallida;

        int puntaje = MissionManager.Instance != null ? MissionManager.Instance.GetPuntaje() : 0;

        if (GameOverScreen.Instance != null)
            GameOverScreen.Instance.Mostrar(puntaje);
        else
            Debug.LogWarning("PlayerHealth: No se encontró GameOverScreen.");
    }

    // ----------------------------------------------------------
    // Getters públicos
    // ----------------------------------------------------------
    public float GetVidaActual()   { return currentHealth; }
    public float GetVidaMaxima()   { return maxHealth; }
    public bool  EscudoEstaActivo(){ return escudoActivo; }
    public bool  BoostEstaActivo() { return boostActivo; }
}