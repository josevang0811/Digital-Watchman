// ============================================================
// Mision.cs
// ============================================================
// TAD: Misión — define la estructura de cada misión del juego.
// Cada misión es un nodo dentro de la LinkedList de misiones
// que maneja el MissionManager.
// ============================================================

[System.Serializable]
public class Mision
{
    // ----------------------------------------------------------
    // Identificación de la misión
    // ----------------------------------------------------------
    public int id;                  // Número único de la misión (1, 2, 3)
    public string titulo;           // "Protege a Carlos"
    public string descripcion;      // Texto que ve el jugador en pantalla
    public string objetivo;         // "Derrota a los 3 agresores"

    // ----------------------------------------------------------
    // Progreso de la misión
    // ----------------------------------------------------------
    public int enemigosRequeridos;  // Cuántos enemigos hay que derrotar
    public int enemigosEliminados;  // Cuántos llevamos eliminados

    // ----------------------------------------------------------
    // Recompensa al completar
    // ----------------------------------------------------------
    public int puntosRecompensa;    // Puntos que gana el jugador
    public float curacionRecompensa; // HP que recupera al completar

    // ----------------------------------------------------------
    // Estado actual de la misión
    // ----------------------------------------------------------
    public EstadoMision estado;

    public enum EstadoMision
    {
        NoIniciada,   // Todavía no ha comenzado
        Activa,       // El jugador está en esta misión ahora
        Completada,   // Ya la terminó exitosamente
        Fallida       // La falló (ej: el jugador murió)
    }

    // Constructor — crea una misión con todos sus datos
    public Mision(int id, string titulo, string descripcion,
                  string objetivo, int enemigosRequeridos,
                  int puntosRecompensa, float curacionRecompensa)
    {
        this.id = id;
        this.titulo = titulo;
        this.descripcion = descripcion;
        this.objetivo = objetivo;
        this.enemigosRequeridos = enemigosRequeridos;
        this.enemigosEliminados = 0; // Siempre empieza en 0
        this.puntosRecompensa = puntosRecompensa;
        this.curacionRecompensa = curacionRecompensa;
        this.estado = EstadoMision.NoIniciada;
    }

    // ----------------------------------------------------------
    // RegistrarEnemigo() — suma un enemigo eliminado y verifica
    // si ya se completó el objetivo de la misión
    // ----------------------------------------------------------
    public bool RegistrarEnemigo()
    {
        if (estado != EstadoMision.Activa) return false;

        enemigosEliminados++;

        // Retorna true si ya eliminamos todos los requeridos
        return enemigosEliminados >= enemigosRequeridos;
    }
}