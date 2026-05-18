// ============================================================
// DatosGuardado.cs
// ============================================================
// Define QUÉ información se guarda en el archivo de guardado.
// Esta clase se serializa a JSON automáticamente con
// JsonUtility de Unity, lo que permite almacenar y recuperar
// los datos del jugador de forma sencilla entre sesiones.
// ============================================================

[System.Serializable]
public class DatosGuardado
{
    // Nombre del jugador que inició la sesión.
    // Se usa para mostrar el nombre en menús y tablas.
    public string nombreJugador;

    // Puntaje acumulado del jugador hasta el momento.
    // Este valor puede aumentar con logros, combates o misiones.
    public int puntaje;

    // Vida actual del jugador al momento de guardar.
    // Puede usarse para restaurar el estado de salud en la siguiente sesión.
    public float vidaActual;

    // Identificador o índice de la misión actual.
    // Indica hasta qué punto avanzó el jugador en la historia.
    public int misionActual;

    // Cantidad total de misiones ya completadas.
    // Útil para estadísticas y desbloqueo de recompensas.
    public int misionesCompletadas;

    // Historial corto de eventos recientes.
    // Se guarda como arreglo de strings para mantener una cola de mensajes.
    public string[] ultimosEventos;

    // Lista de entradas del leaderboard.
    // Cada elemento guarda un nombre y su puntaje.
    public LeaderboardEntry[] leaderboard;

    // Fecha y hora exacta en que se creó el guardado.
    // Se puede mostrar al usuario o usar para comparaciones.
    public string fechaGuardado;
}

// ============================================================
// LeaderboardEntry
// ============================================================
// Cada entrada del leaderboard guarda el nombre del jugador
// y su puntaje para poder mostrar una tabla de clasificación.
// ============================================================
[System.Serializable]
public class LeaderboardEntry
{
    // Nombre o identificador de la entrada en el leaderboard.
    public string identificador;

    // Puntaje que obtuvo ese jugador en la partida guardada.
    public int puntaje;
}