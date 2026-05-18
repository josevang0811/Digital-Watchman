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
//
// ----- Compatibilidad WebGL -----
// En WebGL no existe acceso al sistema de archivos del usuario,
// así que en esa plataforma usamos PlayerPrefs (que Unity
// almacena en IndexedDB del navegador) como fallback.
// La API pública (Guardar/Cargar) sigue siendo idéntica —
// el resto del juego no nota la diferencia.
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
    // en un archivo JSON en el disco (o PlayerPrefs en WebGL)
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

        // EscribirJson encapsula File I/O / PlayerPrefs según plataforma
        EscribirJson(rutaGuardado, archivoGuardado, json);

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
        // LeerJson encapsula File I/O / PlayerPrefs según plataforma
        string json = LeerJson(rutaGuardado, archivoGuardado);

        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("No existe archivo de guardado — partida nueva");
            return null;
        }

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
    // ----------------------------------------------------------
    // GuardarLeaderboard() — recibe el identificador y puntaje
    // del jugador, los inserta en el top 5, ordena y guarda
    // ----------------------------------------------------------
    public void GuardarLeaderboard(string identificador, int puntaje)
    {
        // Cargamos el leaderboard existente
        LeaderboardEntry[] entradas = CargarLeaderboard();

        // Creamos la nueva entrada
        LeaderboardEntry nueva = new LeaderboardEntry();
        nueva.identificador = identificador;
        nueva.puntaje = puntaje;

        // Convertimos a lista para poder insertar
        System.Collections.Generic.List<LeaderboardEntry> lista =
            new System.Collections.Generic.List<LeaderboardEntry>(entradas);

        lista.Add(nueva);

        // Ordenamos de mayor a menor puntaje
        lista.Sort((a, b) => b.puntaje.CompareTo(a.puntaje));

        // Recortamos a top 5
        if (lista.Count > 5)
            lista.RemoveRange(5, lista.Count - 5);

        // Guardamos
        DatosGuardado datos = new DatosGuardado();
        datos.leaderboard = lista.ToArray();

        string json = JsonUtility.ToJson(datos, true);
        EscribirJson(rutaLeaderboard, archivoLeaderboard, json);

        Debug.Log("Leaderboard actualizado — " + identificador + ": " + puntaje);
    }

    // ----------------------------------------------------------
    // CargarLeaderboard() — retorna el top 5 actual
    // Si no existe archivo retorna array vacío
    // ----------------------------------------------------------
    public LeaderboardEntry[] CargarLeaderboard()
    {
        string json = LeerJson(rutaLeaderboard, archivoLeaderboard);

        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("No existe leaderboard — retornando vacío");
            return new LeaderboardEntry[0];
        }

        DatosGuardado datos = JsonUtility.FromJson<DatosGuardado>(json);

        if (datos.leaderboard == null)
            return new LeaderboardEntry[0];

        return datos.leaderboard;
    }

    // ----------------------------------------------------------
    // EliminarGuardado() — borra el archivo (nueva partida)
    // ----------------------------------------------------------
    public void EliminarGuardado()
    {
        BorrarJson(rutaGuardado, archivoGuardado);
        Debug.Log("Archivo de guardado eliminado");
    }

    // ============================================================
    // Helpers de persistencia — abstraen File I/O vs PlayerPrefs
    // ============================================================
    // En editor y standalone (Windows/Mac/Linux) usamos File I/O
    // real para cumplir el requisito de la rúbrica.
    // En WebGL caemos a PlayerPrefs, que Unity persiste en
    // IndexedDB del navegador. La key es el nombre del archivo.
    // ============================================================

    void EscribirJson(string ruta, string keyPrefs, string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.SetString(keyPrefs, json);
        PlayerPrefs.Save();
#else
        File.WriteAllText(ruta, json);
#endif
    }

    string LeerJson(string ruta, string keyPrefs)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!PlayerPrefs.HasKey(keyPrefs)) return null;
        return PlayerPrefs.GetString(keyPrefs);
#else
        if (!File.Exists(ruta)) return null;
        return File.ReadAllText(ruta);
#endif
    }

    void BorrarJson(string ruta, string keyPrefs)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (PlayerPrefs.HasKey(keyPrefs))
        {
            PlayerPrefs.DeleteKey(keyPrefs);
            PlayerPrefs.Save();
        }
#else
        if (File.Exists(ruta)) File.Delete(ruta);
#endif
    }
}