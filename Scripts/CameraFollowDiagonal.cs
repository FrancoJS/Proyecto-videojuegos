using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowDiagonal : MonoBehaviour
{
    [Header("Target")]
    public Transform player;         // Asigna tu jugador aqu�
    public float playerHeight = 1f;  // Altura a la que mira la c�mara (centro del jugador)

    [Header("Camera Settings")]
    public float distanceBack = 15f; // Qu� tan alejada est� la c�mara
    public float smoothTime = 0.3f;  // Suavizado del seguimiento

    private Vector3 velocity = Vector3.zero;

    // Rotaci�n fija
    private readonly Vector3 fixedRotation = new Vector3(77.0047226f, 0.107593283f, 359.807007f);

    void LateUpdate()
    {
        if (player == null) return;

        // Convertimos la rotaci�n a quaternion
        Quaternion rotationQuat = Quaternion.Euler(fixedRotation);

        // Posici�n deseada: centrada en el jugador + altura + alejada seg�n la rotaci�n
        Vector3 targetPos = player.position + Vector3.up * playerHeight + rotationQuat * new Vector3(0f, 0f, -distanceBack);

        // Suavizado del movimiento
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        // Aplicar rotaci�n fija
        transform.rotation = rotationQuat;
    }
}
