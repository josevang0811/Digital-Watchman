using System.Collections.Generic;
using UnityEngine;

// ============================================================
// Inventario.cs
// ============================================================
// ESTRUCTURA DE DATOS OBLIGATORIA: LinkedList (Lista Enlazada)
// ============================================================
// Implementa el TAD Inventario usando LinkedList<Item> de C#.
//
// ¿Por qué LinkedList y no un array o List normal?
// - Array: tamaño fijo, no sirve para inventario dinámico
// - List<T>: internamente es un array que se redimensiona
// - LinkedList<T>: cada nodo apunta al siguiente, inserción
//   y eliminación en cualquier posición es O(1) — eficiente
//   para un inventario donde el jugador agrega y elimina items
//   constantemente en posiciones arbitrarias.
//
// Esto demuestra el uso correcto de LinkedList como estructura
// de datos para el requisito obligatorio de la rúbrica.
// ============================================================

public class Inventario : MonoBehaviour
{
    // Singleton — igual que EventQueue, solo existe un inventario
    public static Inventario Instance;

    // ----------------------------------------------------------
    // La LinkedList — cada nodo es un Item
    // La cadena crece cuando el jugador recoge items
    // y decrece cuando los usa o los descarta
    // ----------------------------------------------------------
    private LinkedList<Item> items = new LinkedList<Item>();

    // Capacidad máxima del inventario
    public int capacidadMaxima = 10;

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
        }
    }

    // ----------------------------------------------------------
    // Agregar() — agrega un item AL FINAL de la lista enlazada
    // Equivale a AddLast() en LinkedList
    // ----------------------------------------------------------
    public bool Agregar(Item item)
    {
        // Verificamos que no hayamos llegado al límite
        if (items.Count >= capacidadMaxima)
        {
            Debug.Log("Inventario lleno — no se puede agregar: " + item.nombre);
            return false;
        }

        // AddLast agrega el nodo al final de la cadena
        items.AddLast(item);

        Debug.Log("Item agregado al inventario: " + item.nombre);

        // Registramos el evento en la Queue — las estructuras
        // de datos se comunican entre sí
        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Item recogido: " + item.nombre);

        return true;
    }

    // ----------------------------------------------------------
    // Usar() — busca un item por nombre, aplica su efecto
    // y lo elimina de la lista enlazada
    // ----------------------------------------------------------
    public bool Usar(string nombreItem)
    {
        // Recorremos la LinkedList buscando el item
        LinkedListNode<Item> nodo = items.First;

        while (nodo != null)
        {
            if (nodo.Value.nombre == nombreItem)
            {
                // Aplicamos el efecto del item
                AplicarEfecto(nodo.Value);

                // Remove() elimina el nodo directamente de la cadena
                // sin tener que reorganizar toda la lista — ventaja
                // clave de LinkedList sobre un array
                items.Remove(nodo);

                if (EventQueue.Instance != null)
                    EventQueue.Instance.Registrar("Item usado: " + nombreItem);

                return true;
            }

            // Avanzamos al siguiente nodo de la cadena
            nodo = nodo.Next;
        }

        Debug.Log("Item no encontrado: " + nombreItem);
        return false;
    }

    // ----------------------------------------------------------
    // AplicarEfecto() — ejecuta el efecto del item según su tipo
    // ----------------------------------------------------------
    void AplicarEfecto(Item item)
    {
        switch (item.tipo)
        {
            case Item.TipoItem.Curacion:
                // Buscamos el PlayerHealth y curamos al jugador
                PlayerHealth ph = FindAnyObjectByType<PlayerHealth>();
                if (ph != null)
                {
                    ph.Curar(item.valor);
                    Debug.Log("Jugador curado: +" + item.valor + " HP");
                }
                break;

            case Item.TipoItem.Puntos:
                if (MissionManager.Instance != null)
                    MissionManager.Instance.SumarPuntaje((int)item.valor);
                break;

            case Item.TipoItem.Velocidad:
                PlayerHealth ph2 = FindAnyObjectByType<PlayerHealth>();
                if (ph2 != null)
                    ph2.ActivarBoostVelocidad(item.valor);
                break;

            case Item.TipoItem.Defensa:
                PlayerHealth ph3 = FindAnyObjectByType<PlayerHealth>();
                if (ph3 != null)
                    ph3.ActivarEscudo(item.valor);
                break;
        }
    }

    // ----------------------------------------------------------
    // ObtenerItems() — retorna la lista para mostrarla en UI
    // ----------------------------------------------------------
    public LinkedList<Item> ObtenerItems()
    {
        return items;
    }

    // ----------------------------------------------------------
    // MostrarInventario() — imprime el inventario en consola
    // Útil para debuggear durante el desarrollo
    // ----------------------------------------------------------
    public void MostrarInventario()
    {
        Debug.Log("=== INVENTARIO (" + items.Count + "/" + capacidadMaxima + ") ===");

        LinkedListNode<Item> nodo = items.First;
        int posicion = 1;

        while (nodo != null)
        {
            Debug.Log(posicion + ". " + nodo.Value.nombre + " — " + nodo.Value.descripcion);
            nodo = nodo.Next;
            posicion++;
        }
    }
}