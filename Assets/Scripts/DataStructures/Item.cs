// ============================================================
// Item.cs
// ============================================================
// TAD: Item — representa cualquier objeto que el jugador
// puede recoger o usar durante el juego.
// Es el "nodo" que vivirá dentro de la LinkedList.
// ============================================================

using UnityEngine;

[System.Serializable] // Permite que Unity muestre este objeto en el Inspector
public class Item
{
    // ----------------------------------------------------------
    // Atributos del item — definen qué es y qué hace
    // ----------------------------------------------------------
    public string nombre;       // "Botiquín", "Escudo", "Boost de velocidad"
    public string descripcion;  // Texto que ve el jugador en el inventario
    public float valor;         // Efecto numérico (ej: cuánto cura un botiquín)
    public TipoItem tipo;       // Qué categoría de item es
    public Sprite icono;

    // ----------------------------------------------------------
    // Enum: define los tipos de items posibles en el juego
    // Un enum es una lista de opciones fijas — más seguro
    // que usar strings sueltos como "curacion" o "Curacion"
    // ----------------------------------------------------------
    public enum TipoItem
    {
        Curacion,       // Restaura vida al jugador
        Defensa,        // Reduce el daño recibido temporalmente
        Velocidad,      // Aumenta la velocidad de movimiento
        Puntos          // Da puntos extra al leaderboard
    }

    // Constructor — forma rápida de crear un item con todos sus datos
    public Item(string nombre, string descripcion, float valor, TipoItem tipo)
    {
        this.nombre = nombre;
        this.descripcion = descripcion;
        this.valor = valor;
        this.tipo = tipo;
    }
}