using System.Collections.Generic;
using UnityEngine;

// ============================================================
// MissionManager.cs
// ============================================================
// Administra la cadena de misiones usando LinkedList.
// Usa una Queue para spawnear enemigos uno por uno —
// así la Queue tiene un rol REAL en el gameplay:
// controla cuántos enemigos quedan por aparecer.
//
// Estructuras de datos en uso:
// - LinkedList  → cadena de misiones (nodo.Next = siguiente misión)
// - Queue       → enemigos pendientes de spawnear en la misión activa
// - File I/O    → guarda progreso al completar cada misión
// ============================================================

public class MissionManager : MonoBehaviour
{
    // ----------------------------------------------------------
    // Singleton
    // ----------------------------------------------------------
    public static MissionManager Instance;

    // ----------------------------------------------------------
    // LinkedList de misiones
    // ----------------------------------------------------------
    private LinkedList<Mision> misiones = new LinkedList<Mision>();
    private LinkedListNode<Mision> misionActualNodo;

    // ----------------------------------------------------------
    // Queue de spawn — ESTRUCTURA DE DATOS REAL EN EL GAMEPLAY
    // Cada misión carga aquí los enemigos que faltan spawnear.
    // Al matar uno, se hace Dequeue y aparece el siguiente.
    // ----------------------------------------------------------
    private Queue<int> colaDeSpawn = new Queue<int>();

    // ----------------------------------------------------------
    // Prefab del enemigo — asígnalo desde el Inspector
    // arrastrando Assets/Prefabs/Enemy.prefab aquí
    // ----------------------------------------------------------
    [SerializeField] private GameObject enemigoPrefab;

    // ----------------------------------------------------------
    // Puntos de spawn — posiciones donde pueden aparecer enemigos
    // Crea objetos vacíos en la escena y asígnalos aquí,
    // O déjalo vacío y usará posiciones aleatorias automáticas.
    // ----------------------------------------------------------
    [SerializeField] private Transform[] puntosDeSpawn;

    // Puntaje total acumulado del jugador
    private int puntajeTotal = 0;

    // Cuántos enemigos están vivos en escena ahora mismo
    private int enemigosVivosenEscena = 0;

    // ----------------------------------------------------------
    // Awake — inicializa el Singleton y las misiones
    // ----------------------------------------------------------
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

        InicializarMisiones();
    }

    // ----------------------------------------------------------
    // Start — inicia la primera misión al arrancar el juego
    // ----------------------------------------------------------
    void Start()
    {
        IniciarMisionActual();
    }

    // ----------------------------------------------------------
    // InicializarMisiones() — construye la LinkedList
    // ----------------------------------------------------------
    void InicializarMisiones()
    {
        misiones.AddLast(new Mision(
            id: 1,
            titulo: "¿Que esta pasando Rony?",
            descripcion: "Encuentra y desactiva los bots agresores en la guarida",
            objetivo: "Derrota a los 4 robots agresores",
            enemigosRequeridos: 4,
            puntosRecompensa: 100,
            curacionRecompensa: 30f
        ));

        misiones.AddLast(new Mision(
            id: 2,
            titulo: "Guarida",
            descripcion: "Los agresores tienen un líder. Encuéntralo y detén su operación.",
            objetivo: "Derrota a los 8 robots agresores",
            enemigosRequeridos: 8,
            puntosRecompensa: 200,
            curacionRecompensa: 20f
        ));

        misiones.AddLast(new Mision(
            id: 3,
            titulo: "Confrontación final",
            descripcion: "Derrota al boss principal — VOID_K3RN3L — y salva el día.",
            objetivo: "Derrota al boss final",
            enemigosRequeridos: 1,
            puntosRecompensa: 500,
            curacionRecompensa: 50f
        ));

        misionActualNodo = misiones.First;
        Debug.Log("Sistema de misiones inicializado — " + misiones.Count + " misiones cargadas");
    }

    // ----------------------------------------------------------
    // IniciarMisionActual() — activa la misión y carga la Queue
    // ----------------------------------------------------------
    public void IniciarMisionActual()
    {
        if (misionActualNodo == null)
        {
            Debug.Log("¡JUEGO COMPLETADO!");
            return;
        }

        Mision mision = misionActualNodo.Value;
        mision.estado = Mision.EstadoMision.Activa;

        // -------------------------------------------------------
        // QUEUE EN ACCIÓN: cargamos tantos "tokens" como enemigos
        // requiere la misión. Cada token = un enemigo por spawnear.
        // -------------------------------------------------------
        colaDeSpawn.Clear();
        for (int i = 0; i < mision.enemigosRequeridos; i++)
        {
            colaDeSpawn.Enqueue(i + 1); // el número identifica al enemigo
        }

        Debug.Log("=== MISIÓN " + mision.id + " INICIADA ===");
        Debug.Log("Objetivo: " + mision.objetivo);
        Debug.Log("Queue cargada con " + colaDeSpawn.Count + " enemigos por spawnear");

        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Misión " + mision.id + " iniciada: " + mision.titulo);

        // Spawneamos el primer enemigo inmediatamente
        SpawnearSiguienteEnemigo();
    }

    // ----------------------------------------------------------
    // SpawnearSiguienteEnemigo() — saca un token de la Queue
    // y crea un enemigo en escena
    // ----------------------------------------------------------
    void SpawnearSiguienteEnemigo()
    {
        // Si la Queue está vacía, no hay más enemigos que spawnear
        if (colaDeSpawn.Count == 0)
        {
            Debug.Log("Queue vacía — todos los enemigos ya fueron spawneados");
            return;
        }

        // Si ya hay un enemigo vivo, esperamos a que muera
        if (enemigosVivosenEscena > 0)
        {
            Debug.Log("Hay " + enemigosVivosenEscena + " enemigo(s) vivo(s) — esperando...");
            return;
        }

        if (enemigoPrefab == null)
        {
            Debug.LogError("¡Falta asignar el Prefab de enemigo en el Inspector del GameManager!");
            return;
        }

        // Dequeue — sacamos el siguiente token de la cola
        int numeroEnemigo = colaDeSpawn.Dequeue();

        // Calculamos posición de spawn
        Vector3 posicion = CalcularPosicionSpawn();

        // Instanciamos el enemigo en la escena
        Instantiate(enemigoPrefab, posicion, Quaternion.identity);
        enemigosVivosenEscena++;

        Debug.Log("Enemigo #" + numeroEnemigo + " spawneado en " + posicion +
                  " | Restantes en Queue: " + colaDeSpawn.Count);

        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Enemigo #" + numeroEnemigo + " apareció — " +
                                          colaDeSpawn.Count + " en cola");
    }

    // ----------------------------------------------------------
    // CalcularPosicionSpawn() — devuelve posición de spawn
    // Usa los puntos asignados en el Inspector, o posición
    // aleatoria si no hay puntos configurados
    // ----------------------------------------------------------
    Vector3 CalcularPosicionSpawn()
    {
        // Si hay puntos de spawn configurados, usamos uno al azar
        if (puntosDeSpawn != null && puntosDeSpawn.Length > 0)
        {
            int indice = Random.Range(0, puntosDeSpawn.Length);
            return puntosDeSpawn[indice].position;
        }

        // Si no hay puntos, spawn aleatorio alrededor del centro
        float x = Random.Range(-8f, 8f);
        float z = Random.Range(-8f, 8f);
        return new Vector3(x, 0f, z);
    }

    // ----------------------------------------------------------
    // NotificarEnemigoEliminado() — se llama desde EnemyHealth.Die()
    // Actualiza el progreso y spawne el siguiente enemigo
    // ----------------------------------------------------------
    public void NotificarEnemigoEliminado()
    {
        if (misionActualNodo == null) return;

        Mision mision = misionActualNodo.Value;

        puntajeTotal += 25;

        // Un enemigo menos en escena
        enemigosVivosenEscena--;
        if (enemigosVivosenEscena < 0) enemigosVivosenEscena = 0;

        // Registramos el kill en la misión
        bool misionCompletada = mision.RegistrarEnemigo();

        Debug.Log("Kill registrado — Progreso: " +
                  mision.enemigosEliminados + "/" + mision.enemigosRequeridos +
                  " | Queue restante: " + colaDeSpawn.Count);

        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Enemigo eliminado — " +
                mision.enemigosEliminados + "/" + mision.enemigosRequeridos);

        if (misionCompletada)
        {
            // Misión terminada — no spawneamos más
            CompletarMision();
        }
        else
        {
            // Todavía quedan enemigos — spawneamos el siguiente de la Queue
            SpawnearSiguienteEnemigo();
        }
    }

    // ----------------------------------------------------------
    // CompletarMision() — recompensas y avance de LinkedList
    // ----------------------------------------------------------
    void CompletarMision()
    {
        Mision mision = misionActualNodo.Value;
        mision.estado = Mision.EstadoMision.Completada;

        puntajeTotal += mision.puntosRecompensa;

        Debug.Log("=== MISIÓN " + mision.id + " COMPLETADA ===");
        Debug.Log("+" + mision.puntosRecompensa + " puntos | Total: " + puntajeTotal);

        // Curamos al jugador
        PlayerHealth ph = FindAnyObjectByType<PlayerHealth>();
        if (ph != null)
            ph.Curar(mision.curacionRecompensa);

        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("¡Misión " + mision.id +
                " completada! +" + mision.puntosRecompensa + " puntos");

        // Guardamos progreso con File I/O
        if (FileManager.Instance != null)
        {
            string nombre = PlayerPrefs.GetString("IdentificadorActual", "Jugador");
            FileManager.Instance.Guardar(nombre, puntajeTotal, ph != null ? ph.GetVidaActual() : 100f, mision.id);
            FileManager.Instance.GuardarLeaderboard(nombre, puntajeTotal);
        }

        // -------------------------------------------------------
        // LINKEDLIST EN ACCIÓN: avanzamos al siguiente nodo
        // .Next nos da la siguiente misión en la cadena
        // -------------------------------------------------------
        misionActualNodo = misionActualNodo.Next;
    }

    // ----------------------------------------------------------
    // Getters para la UI
    // ----------------------------------------------------------
    public Mision GetMisionActual()
    {
        return misionActualNodo?.Value;
    }

    public int GetPuntaje()
    {
        return puntajeTotal;
    }

    public int GetEnemigosEnCola()
    {
        return colaDeSpawn.Count;
    }
    public void SumarPuntaje(int cantidad)
    {
        puntajeTotal += cantidad;
        Debug.Log("[MISIONES] Puntaje sumado: +" + cantidad + " | Total: " + puntajeTotal);
    }
}
