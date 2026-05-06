using UnityEngine;

// ============================================================
// ItemDropper.cs
// ============================================================
// Decide qué ítem dropea el enemigo al morir usando
// probabilidades aleatorias — este es el componente
// aleatorio de la rúbrica, más demostrable que el spawn.
//
// Cómo funciona:
// 1. Genera un número aleatorio entre 0 y 1
// 2. Lo compara contra rangos de probabilidad
// 3. Instancia el Prefab del ítem ganador en la posición
//    donde murió el enemigo
//
// Este script va en el mismo objeto Enemy (como componente).
// EnemyHealth.Die() lo llama al morir.
// ============================================================
public class ItemDropper : MonoBehaviour
{
    [Header("Prefabs de ítems")]
    [SerializeField] private GameObject prefabBotiquinPequeno;  // esfera roja
    [SerializeField] private GameObject prefabBotiquinGrande;   // esfera roja grande
    [SerializeField] private GameObject prefabBoostVelocidad;   // esfera azul
    [SerializeField] private GameObject prefabChipDatos;        // cubo verde
    [SerializeField] private GameObject prefabEscudo;           // cubo dorado

    [Header("Probabilidades (deben sumar 1.0)")]
    [Range(0f, 1f)] public float probBotiquinPequeno = 0.50f;  // 50%
    [Range(0f, 1f)] public float probChipDatos       = 0.20f;  // 20%
    [Range(0f, 1f)] public float probBoostVelocidad  = 0.15f;  // 15%
    [Range(0f, 1f)] public float probEscudo          = 0.10f;  // 10%
    [Range(0f, 1f)] public float probBotiquinGrande  = 0.05f;  // 5%
    // Total: 1.0 = 100%

    // ----------------------------------------------------------
    // Dropear() — llamado desde EnemyHealth.Die()
    // Usa Random.Range para decidir qué cae al suelo
    // ----------------------------------------------------------
    public void Dropear(Vector3 posicion)
    {
        // Genera un número aleatorio entre 0 (inclusive) y 1 (exclusivo)
        // Este valor determinará qué ítem se dropeará usando probabilidades
        float roll = Random.Range(0f, 1f);

        Debug.Log("[DROPPER] Roll: " + roll.ToString("F2"));

        // Selecciona el prefab del ítem basándose en el valor aleatorio
        // ElegirPrefab() compara el roll contra rangos de probabilidad acumulativos
        GameObject prefabElegido = ElegirPrefab(roll);

        // Si no hay un ítem para dropear (caso extraordinario), sale sin instanciar nada
        if (prefabElegido == null)
        {
            Debug.Log("[DROPPER] Sin drop esta vez");
            return;
        }

        // Calcula la posición de spawn ligeramente arriba del enemigo
        // (+0.5 en Y) para evitar que el ítem quede enterrado en el suelo
        Vector3 posicionDrop = posicion + Vector3.up * 0.5f;

        // Instancia el ítem en la posición calculada, con rotación por defecto
        Instantiate(prefabElegido, posicionDrop, Quaternion.identity);

        // Registra qué ítem se dropeó y su ubicación exacta (útil para debugging)
        Debug.Log("[DROPPER] Dropeado: " + prefabElegido.name + " en " + posicionDrop);
    }

    // ----------------------------------------------------------
    // ElegirPrefab() — lógica de probabilidades
    // Compara el roll contra rangos acumulativos
    //
    // Ejemplo con roll = 0.73:
    //   0.00 - 0.50 → Botiquín pequeño  (no, 0.73 > 0.50)
    //   0.50 - 0.70 → Chip de datos     (no, 0.73 > 0.70)
    //   0.70 - 0.85 → Boost velocidad   (SÍ, 0.73 está aquí)
    // ----------------------------------------------------------
    GameObject ElegirPrefab(float roll)
    {
        float acumulado = 0f;

        acumulado += probBotiquinPequeno;
        if (roll < acumulado) return prefabBotiquinPequeno;

        acumulado += probChipDatos;
        if (roll < acumulado) return prefabChipDatos;

        acumulado += probBoostVelocidad;
        if (roll < acumulado) return prefabBoostVelocidad;

        acumulado += probEscudo;
        if (roll < acumulado) return prefabEscudo;

        acumulado += probBotiquinGrande;
        if (roll < acumulado) return prefabBotiquinGrande;

        // Si el roll no cayó en ningún rango (puede pasar por
        // redondeo), retornamos el botiquín pequeño por defecto
        return prefabBotiquinPequeno;
    }
}