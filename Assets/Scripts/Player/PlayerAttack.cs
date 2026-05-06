using UnityEngine;

// ============================================================
// PlayerAttack.cs — actualizado
// Cambio: TryAttack() registra el ataque en el ActionStack
// ============================================================
public class PlayerAttack : MonoBehaviour
{
    [Header("Configuración de ataque")]
    public float attackRange = 2f;
    public float attackDamage = 25f;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer;

    private Animator animator;
    private float lastAttackTime = 0f;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsDialogueActive())
            TryAttack();
    }

    bool IsDialogueActive()
    {
        return DialogueSystem.Instance != null && DialogueSystem.Instance.dialogoActivo;
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Atacar");

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attackRange,
            enemyLayer
        );

        if (hits.Length > 0)
        {
            // Registramos en el Stack solo si golpeamos a alguien
            foreach (Collider hit in hits)
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                    Debug.Log("Golpeaste a: " + hit.name);

                    // Push al Stack — acción real registrada
                    if (ActionStack.Instance != null)
                        ActionStack.Instance.Registrar("Atacaste a " + hit.name + " (-" + attackDamage + " HP)");
                }
            }
        }
        else
        {
            // También registramos el ataque fallido — es información útil
            if (ActionStack.Instance != null)
                ActionStack.Instance.Registrar("Ataque al aire (sin impacto)");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}