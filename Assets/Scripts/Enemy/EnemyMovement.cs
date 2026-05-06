using UnityEngine;

// ============================================================
// EnemyMovement.cs
// Controla el movimiento del enemigo hacia el jugador.
// El enemigo detecta al jugador dentro de un rango de
// "agro" y lo persigue hasta llegar a rango de ataque.
//
// El Player se busca automáticamente por Tag al spawnearse,
// lo que permite que funcione con el sistema de spawn dinámico
// del MissionManager sin necesidad de asignarlo en el Inspector.
// ============================================================
public class EnemyMovement : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    public float moveSpeed = 3f;
    public float detectionRange = 50f;  // Alto para que siempre persiga
    public float attackRange = 1.5f;

    [Header("Referencias")]
    public Transform player; // Se asigna automáticamente en Start()

    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        // Busca al jugador automáticamente por Tag.
        // Funciona aunque el enemigo sea spawneado dinámicamente.
        // Asegúrate que el objeto Player tenga Tag = "Player" en Unity.
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("EnemyMovement: No se encontró objeto con Tag 'Player'.");
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        else
        {
            if (animator != null)
                animator.SetBool("isMoving", false);
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

        if (animator != null)
            animator.SetBool("isMoving", true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
