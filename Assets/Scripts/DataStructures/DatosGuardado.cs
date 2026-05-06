// ============================================================
// DatosGuardado.cs
// ============================================================
// Define QUÉ información se guarda en el archivo.
// Esta clase se serializa a JSON automáticamente con
// JsonUtility de Unity — cada campo público se convierte
// en una línea del archivo de texto.
// ============================================================

[System.Serializable] // Obligatorio para que JsonUtility funcione
public class DatosGuardado
{
    // Datos del jugador
    public string nombreJugador;
    public int puntaje;
    public float vidaActual;
    public int misionActual;

    // Misiones completadas (cuántas llevamos)
    public int misionesCompletadas;

    // Historial de los últimos eventos (de la Queue)
    // Se guarda como array porque JSON no soporta Queue directamente
    public string[] ultimosEventos;

    // Leaderboard — top 5 puntajes
    public int[] leaderboard;

    // Fecha y hora del guardado
    public string fechaGuardado;
}