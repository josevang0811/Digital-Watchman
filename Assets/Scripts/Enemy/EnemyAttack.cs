using UnityEngine;

// ============================================================
// EnemyAttack.cs — actualizado
// Cambio: player y playerHealth se buscan automáticamente
// por Tag al spawnearse, igual que EnemyMovement.
// ============================================================
public class EnemyAttack : MonoBehaviour
{
    [Header("Configuración de ataque")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float attackRange = 1.5f;

    [Header("Referencias")]
    public Transform player;

    private float lastAttackTime = 0f;
    private PlayerHealth playerHealth;

    void Start()
    {
        // Busca al jugador automáticamente por Tag
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
            else
            {
                Debug.LogWarning("EnemyAttack: No se encontró objeto con Tag 'Player'.");
            }
        }
        else
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        playerHealth.TakeDamage(attackDamage);
        Debug.Log("Enemigo atacó al jugador — Daño: " + attackDamage);
    }
}