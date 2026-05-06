using System.Collections.Generic;
using UnityEngine;

// ============================================================
// EventQueue.cs
// ============================================================
// ESTRUCTURA DE DATOS OBLIGATORIA: Queue (Cola) — FIFO
// ============================================================
// Este script implementa el TAD Historial definido en el
// diseño del juego. Registra cronológicamente todo lo que
// ocurre durante la partida: misiones, combates, logros.
//
// FIFO significa: el primer evento registrado es el primero
// en consultarse. Igual que una fila — el primero en llegar
// es el primero en ser atendido.
//
// ¿Por qué Queue y no otro tipo de lista?
// Porque el historial tiene orden cronológico estricto.
// No necesitamos acceder al medio ni al final — solo
// registrar al final y consultar desde el principio.
// ============================================================

public class EventQueue : MonoBehaviour
{
    // ----------------------------------------------------------
    // Singleton: permite que cualquier script del juego acceda
    // al historial con EventQueue.Instance.Registrar(...)
    // sin necesidad de buscar el objeto en la escena.
    // ----------------------------------------------------------
    public static EventQueue Instance;

    // ----------------------------------------------------------
    // La Queue internamente — usamos Queue<string> de C#
    // que ya implementa la estructura FIFO por nosotros.
    // En el contexto universitario esto demuestra el uso
    // correcto de la estructura de datos.
    // ----------------------------------------------------------
    private Queue<string> historial = new Queue<string>();

    // Límite de eventos que guarda el historial
    // (para no acumular memoria infinitamente)
    public int maxEventos = 50;

    void Awake()
    {
        // Configuramos el Singleton — solo puede existir
        // una instancia del historial en toda la escena
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ----------------------------------------------------------
    // Registrar() — agrega un evento al FINAL de la cola (Enqueue)
    // Ejemplo: EventQueue.Instance.Registrar("Enemigo derrotado")
    // ----------------------------------------------------------
    public void Registrar(string evento)
    {
        // Si llegamos al límite, eliminamos el evento más antiguo
        // (el que está al FRENTE de la cola — Dequeue)
        if (historial.Count >= maxEventos)
        {
            historial.Dequeue(); // Saca el más viejo
        }

        // Agregamos timestamp para saber cuándo ocurrió
        string eventoConTiempo = $"[{Time.time:F1}s] {evento}";

        historial.Enqueue(eventoConTiempo); // Agrega al final

        // Lo mostramos en consola para verificar que funciona
        Debug.Log("HISTORIAL: " + eventoConTiempo);
    }

    // ----------------------------------------------------------
    // ObtenerHistorial() — retorna todos los eventos registrados
    // Se usa para mostrarlos en UI o guardarlos en archivo
    // ----------------------------------------------------------
    public Queue<string> ObtenerHistorial()
    {
        return historial;
    }

    // ----------------------------------------------------------
    // ObtenerUltimos() — retorna los N eventos más recientes
    // Útil para mostrar solo los últimos 5 eventos en pantalla
    // ----------------------------------------------------------
    public List<string> ObtenerUltimos(int n)
    {
        // Convertimos la Queue a lista para poder acceder
        // a los últimos N elementos fácilmente
        List<string> lista = new List<string>(historial);

        // Si pedimos más de los que hay, retornamos todos
        int inicio = Mathf.Max(0, lista.Count - n);
        return lista.GetRange(inicio, lista.Count - inicio);
    }

    // ----------------------------------------------------------
    // Limpiar() — vacía el historial completamente
    // Se usa al iniciar una nueva partida
    // ----------------------------------------------------------
    public void Limpiar()
    {
        historial.Clear();
        Debug.Log("Historial limpiado.");
    }
}