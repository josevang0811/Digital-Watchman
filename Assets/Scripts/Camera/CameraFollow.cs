using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;        // Arrastra el Player aquí en el Inspector
    public Vector3 offset = new Vector3(0f, 8f, -6f); // Posición relativa
    public float smoothSpeed = 5f;  // Suavidad del seguimiento

    void LateUpdate()
    {
        if (player == null) return;

        // Posición deseada = posición del jugador + offset
        Vector3 desiredPosition = player.position + offset;

        // Interpolamos suavemente
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // La cámara siempre mira al jugador
        transform.LookAt(player);
    }
}