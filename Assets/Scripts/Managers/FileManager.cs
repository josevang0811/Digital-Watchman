using System;
using System.IO;
using UnityEngine;

// ============================================================
// FileManager.cs
// ============================================================
// ESTRUCTURA DE DATOS OBLIGATORIA: File I/O
// ============================================================
// Maneja todo el sistema de guardado y carga del juego.
// Escribe y lee archivos JSON en el disco del jugador.
//
// ¿Dónde se guardan los archivos?
// Unity usa Application.persistentDataPath — una carpeta
// que el sistema operativo asigna específicamente a este juego.
// En Windows: C:/Users/[usuario]/AppData/LocalLow/[empresa]/[juego]
// Es una ruta segura que no requiere permisos especiales.
// ============================================================

public class FileManager : MonoBehaviour
{
    // Singleton — un solo FileManager en toda la escena
    public static FileManager Instance;

    // Nombre del archivo de guardado
    private string archivoGuardado = "guardado.json";
    private string archivoLeaderboard = "leaderboard.json";

    // Ruta completa donde se guarda el archivo
    private string rutaGuardado;
    private string rutaLeaderboard;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Construimos la ruta completa del archivo
        // Path.Combine une rutas de forma segura en cualquier OS
        rutaGuardado = Path.Combine(Application.persistentDataPath, archivoGuardado);
        rutaLeaderboard = Path.Combine(Application.persistentDataPath, archivoLeaderboard);

        Debug.Log("Archivos de guardado en: " + Application.persistentDataPath);
    }

    // ----------------------------------------------------------
    // Guardar() — toma el estado actual del juego y lo escribe
    // en un archivo JSON en el disco
    // ----------------------------------------------------------
    public void Guardar(string nombreJugador, int puntaje, float vidaActual, int misionActual)
    {
        // Creamos el objeto con todos los datos a guardar
        DatosGuardado datos = new DatosGuardado();
        datos.nombreJugador = nombreJugador;
        datos.puntaje = puntaje;
        datos.vidaActual = vidaActual;
        datos.misionActual = misionActual;
        datos.fechaGuardado = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // Recuperamos los últimos 10 eventos de la Queue
        // y los convertimos a array para poder guardarlos en JSON
        if (EventQueue.Instance != null)
        {
            var ultimosEventos = EventQueue.Instance.ObtenerUltimos(10);
            datos.ultimosEventos = ultimosEventos.ToArray();
        }

        // JsonUtility.ToJson convierte el objeto C# a texto JSON
        // El true activa el formato legible (indentado)
        string json = JsonUtility.ToJson(datos, true);

        // File.WriteAllText escribe el texto en el archivo
        // Si el archivo no existe, lo crea automáticamente
        File.WriteAllText(rutaGuardado, json);

        Debug.Log("Juego guardado correctamente:\n" + json);

        // Registramos el evento en la Queue
        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Juego guardado — Misión " + misionActual);
    }

    // ----------------------------------------------------------
    // Cargar() — lee el archivo JSON y retorna los datos
    // Retorna null si no existe archivo de guardado
    // ----------------------------------------------------------
    public DatosGuardado Cargar()
    {
        // Verificamos que el archivo existe antes de leerlo
        if (!File.Exists(rutaGuardado))
        {
            Debug.Log("No existe archivo de guardado — partida nueva");
            return null;
        }

        // File.ReadAllText lee todo el contenido del archivo
        string json = File.ReadAllText(rutaGuardado);

        // JsonUtility.FromJson convierte el texto JSON de vuelta
        // a un objeto C# de tipo DatosGuardado
        DatosGuardado datos = JsonUtility.FromJson<DatosGuardado>(json);

        Debug.Log("Juego cargado — Jugador: " + datos.nombreJugador +
                  " | Misión: " + datos.misionActual +
                  " | Puntaje: " + datos.puntaje);

        return datos;
    }

    // ----------------------------------------------------------
    // GuardarLeaderboard() — guarda el top 5 de puntajes
    // ----------------------------------------------------------
    public void GuardarLeaderboard(int[] puntajes)
    {
        DatosGuardado datos = new DatosGuardado();
        datos.leaderboard = puntajes;

        string json = JsonUtility.ToJson(datos, true);
        File.WriteAllText(rutaLeaderboard, json);

        Debug.Log("Leaderboard guardado");
    }

    // ----------------------------------------------------------
    // CargarLeaderboard() — recupera el top 5 del archivo
    // ----------------------------------------------------------
    public int[] CargarLeaderboard()
    {
        if (!File.Exists(rutaLeaderboard))
        {
            Debug.Log("No existe leaderboard — retornando vacío");
            return new int[5]; // Array de 5 ceros
        }

        string json = File.ReadAllText(rutaLeaderboard);
        DatosGuardado datos = JsonUtility.FromJson<DatosGuardado>(json);

        return datos.leaderboard;
    }

    // ----------------------------------------------------------
    // EliminarGuardado() — borra el archivo (nueva partida)
    // ----------------------------------------------------------
    public void EliminarGuardado()
    {
        if (File.Exists(rutaGuardado))
        {
            File.Delete(rutaGuardado);
            Debug.Log("Archivo de guardado eliminado");
        }
    }
}