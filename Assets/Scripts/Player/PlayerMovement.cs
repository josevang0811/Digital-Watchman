using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 5f;
    public float rotationSpeed = 10f;

    // InventarioUI se asigna automáticamente desde
    // InventarioUI.Awake() — no necesitas arrastrarlo a mano
    [HideInInspector] public InventarioUI inventarioUI;
    [SerializeField] GameObject hud;

    private Animator animator;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void OnDisable()
    {
        // Detener animación de caminar al desactivarse (ej: durante diálogo)
        if (animator != null) animator.SetBool("isMoving", false);
        if (rb != null) rb.linearVelocity = Vector3.zero;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventarioUI == null) return;

            if (inventarioUI.gameObject.activeSelf)
            {
                inventarioUI.CerrarInventario();
                if (hud != null) hud.SetActive(true);  // reactivar HUD
            }
            else
            {
                inventarioUI.AbrirInventario();
                if (hud != null) hud.SetActive(false); // ocultar HUD
            }
        }
    }
    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 direction = new Vector3(moveX, 0f, moveZ).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

            if (animator != null) animator.SetBool("isMoving", true);
        }
        else
        {
            if (animator != null) animator.SetBool("isMoving", false);
        }
    }
}