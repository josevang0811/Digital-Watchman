using UnityEngine;

// ============================================================
// EnemyHealth.cs — actualizado
// Cambio: Die() ya no crea el ítem directamente.
// Ahora delega el drop al ItemDropper, que decide
// qué cae con probabilidades aleatorias.
// ============================================================
public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    // Referencia al ItemDropper — está en el mismo objeto
    private ItemDropper itemDropper;

    void Start()
    {
        currentHealth = maxHealth;
        // Buscamos el ItemDropper en este mismo objeto
        itemDropper = GetComponent<ItemDropper>();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log(name + " HP: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        // 1. Registramos en la Queue
        if (EventQueue.Instance != null)
            EventQueue.Instance.Registrar("Enemigo eliminado: " + gameObject.name);

        // 2. El ItemDropper decide qué dropear con aleatoriedad
        //    Le pasamos la posición actual del enemigo
        if (itemDropper != null)
            itemDropper.Dropear(transform.position);

        // 3. Notificamos al MissionManager
        if (MissionManager.Instance != null)
            MissionManager.Instance.NotificarEnemigoEliminado();

        // 4. Destruimos — siempre al final
        Destroy(gameObject);
    }
}