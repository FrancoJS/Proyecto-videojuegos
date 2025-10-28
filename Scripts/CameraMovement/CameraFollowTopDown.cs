using UnityEngine;

public class CameraFollowTopDown : MonoBehaviour
{
    [Header("Target")]
    public Transform target;              // Jugador (el verde)

    [Header("Follow")]
    public Vector3 offset = new Vector3(0f, 12f, -10f); // Altura y “hacia atrás”
    public float smoothTime = 0.20f;      // 0.15–0.35 se siente suave
    private Vector3 _velocity = Vector3.zero;

    [Header("Lookahead (opcional)")]
    public Rigidbody targetRb;            // Asigna el Rigidbody del jugador si quieres “anticipación”
    public float lookaheadMultiplier = 0.35f; // 0 desactiva el lookahead
    public float maxLookahead = 2.0f;     // límite del empuje por velocidad

    [Header("Bounds (opcional)")]
    public bool useBounds = false;
    public Vector2 minXmaxX = new Vector2(-50f, 50f);
    public Vector2 minZmaxZ = new Vector2(-50f, 50f);

    void LateUpdate()
    {
        if (!target) return;

        // Punto base a seguir
        Vector3 followPoint = target.position;

        // Lookahead opcional según velocidad del jugador
        if (targetRb && lookaheadMultiplier > 0f)
        {
            Vector3 v = targetRb.velocity;
            Vector3 horizV = new Vector3(v.x, 0f, v.z);
            Vector3 ahead = Vector3.ClampMagnitude(horizV * lookaheadMultiplier, maxLookahead);
            followPoint += ahead;
        }

        // Posición deseada manteniendo el offset (cámara inclinada fija)
        Vector3 desired = followPoint + offset;

        // Suavizado real (critically damped-ish)
        Vector3 newPos = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);

        // Limitar dentro del mapa (opcional)
        if (useBounds)
        {
            newPos.x = Mathf.Clamp(newPos.x, minXmaxX.x, minXmaxX.y);
            newPos.z = Mathf.Clamp(newPos.z, minZmaxZ.x, minZmaxZ.y);
        }

        transform.position = newPos;
        // Mantén la rotación fija desde el Editor (no usar LookAt)
    }
}
