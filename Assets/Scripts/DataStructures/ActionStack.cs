using System.Collections.Generic;
using UnityEngine;

// ============================================================
// ActionStack.cs
// ============================================================
// Estructura de datos: Stack (Pila) — LIFO
// Last In, First Out — la última acción registrada
// es la primera en mostrarse al hacer Game Over.
//
// Por qué Stack y no Queue:
// - Queue (FIFO) mostraría las acciones más antiguas primero
// - Stack (LIFO) muestra las más RECIENTES primero — ideal
//   para "qué pasó justo antes de morir"
// ============================================================

public class ActionStack : MonoBehaviour
{
    public static ActionStack Instance;

    // El Stack — LIFO
    private Stack<string> acciones = new Stack<string>();

    [SerializeField] private int maxAcciones = 50;

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

    // Push — agrega acción al tope del Stack
    // Llamado desde PlayerAttack y PlayerHealth
    public void Registrar(string accion)
    {
        if (acciones.Count >= maxAcciones)
        {
            // Reconstruimos el Stack sin el elemento más antiguo (el fondo)
            var lista = new List<string>(acciones);
            lista.RemoveAt(lista.Count - 1);
            acciones.Clear();
            for (int i = lista.Count - 1; i >= 0; i--)
                acciones.Push(lista[i]);
        }

        acciones.Push(accion);
        Debug.Log("[STACK] Push: \"" + accion + "\" | Tamaño: " + acciones.Count);
    }

    // Retorna las acciones más recientes para mostrar en Game Over
    // El Stack ya las da en orden LIFO — la más reciente primero
    public List<string> ObtenerUltimas(int cantidad)
    {
        List<string> resultado = new List<string>();

        // Copiamos el Stack para no destruirlo al leerlo
        Stack<string> copia = new Stack<string>(new Stack<string>(acciones));

        int count = 0;
        while (copia.Count > 0 && count < cantidad)
        {
            resultado.Add(copia.Pop());
            count++;
        }

        return resultado;
    }

    // Vacía el Stack — útil al reiniciar
    public void Limpiar()
    {
        acciones.Clear();
    }

    public int GetTamanio()
    {
        return acciones.Count;
    }
}