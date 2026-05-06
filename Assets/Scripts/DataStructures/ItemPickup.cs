using UnityEngine;

// ============================================================
// ItemPickup.cs
// ============================================================
// Va en el Prefab de cada ítem físico en el mundo.
// Cuando el jugador camina encima, OnTriggerEnter lo detecta
// y agrega el ítem a la LinkedList del Inventario.
//
// También anima el ítem: flota y rota continuamente para
// que sea fácil de ver en el mapa — efecto tipo Doom/RPG.
//
// Estructura del Prefab:
//   ItemPrefab
//   ├── MeshRenderer (el modelo 3D visual)
//   ├── SphereCollider (isTrigger = true) ← detecta al jugador
//   ├── ItemPickup.cs (este script)
//   └── El campo "itemData" apuntando al ScriptableObject
//       o configurado directamente aquí
// ============================================================
public class ItemPickup : MonoBehaviour
{
    [Header("Datos del ítem")]
    public string nombreItem;           // "Botiquín", "Boost de velocidad"
    public string descripcionItem;      // "Restaura 25 HP"
    public float valorItem;             // 25f
    public Item.TipoItem tipoItem;      // Item.TipoItem.Curacion
    public Sprite iconoItem;            // imagen 2D para el inventario

    [Header("Animación flotante")]
    public float velocidadRotacion = 90f;   // grados por segundo
    public float alturaFlotacion   = 0.3f;  // qué tan alto sube y baja
    public float velocidadFlotacion = 2f;   // qué tan rápido flota

    // Posición Y original del objeto al spawnearse
    private float posYInicial;

    // Para controlar si ya fue recogido (evita recogerlo dos veces)
    private bool yaRecogido = false;

    void Start()
    {
        // Guardamos la posición Y inicial para la animación
        posYInicial = transform.position.y;
    }

    // ----------------------------------------------------------
    // Update() — animación de rotación y flotación
    // Mathf.Sin genera una onda suave que hace flotar el objeto
    // ----------------------------------------------------------
    void Update()
    {
        // Rotación continua en el eje Y
        transform.Rotate(0f, velocidadRotacion * Time.deltaTime, 0f);

        // Flotación suave usando función seno
        // Sin(tiempo) devuelxve valores entre -1 y 1 ciclicamente
        float nuevaY = posYInicial + Mathf.Sin(Time.time * velocidadFlotacion) * alturaFlotacion;
        transform.position = new Vector3(
            transform.position.x,
            nuevaY,
            transform.position.z
        );
    }

    // ----------------------------------------------------------
    // OnTriggerEnter() — se activa cuando algo entra al collider
    // Solo reacciona si es el jugador (Tag = "Player")
    // ----------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        // Verificamos que no haya sido recogido ya
        if (yaRecogido) return;

        // Solo reaccionamos al jugador
        if (!other.CompareTag("Player")) return;

        // Marcamos como recogido para evitar doble activación
        yaRecogido = true;

        // Construimos el objeto Item con los datos de este Prefab
        Item item = new Item(
            nombreItem,
            descripcionItem,
            valorItem,
            tipoItem
        );

        // Asignamos el ícono al ítem
        item.icono = iconoItem;

        // Agregamos a la LinkedList del inventario
        if (Inventario.Instance != null)
        {
            bool agregado = Inventario.Instance.Agregar(item);

            if (agregado)
            {
                // Registramos en el Stack (acción del jugador)
                if (ActionStack.Instance != null)
                    ActionStack.Instance.Registrar("Recogiste " + nombreItem);

                Debug.Log("[PICKUP] Recogido: " + nombreItem);
            }
        }

        // Destruimos el objeto del mundo
        Destroy(gameObject);
    }

    // ----------------------------------------------------------
    // OnDrawGizmosSelected() — muestra el rango de recolección
    // en el editor para verificar el tamaño del trigger
    // ----------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}