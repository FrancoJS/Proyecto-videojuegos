using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Playermovement : MonoBehaviour
{
    [Header("Input (arrastra la acción Move)")]
    public InputActionReference moveAction;

    [Header("Movimiento suave")]
    public Transform orientation;     // arrastra la cámara si quieres que W sea donde mira
    public float maxSpeed = 6f;       // velocidad máxima
    public float acceleration = 12f;  // qué tan rápido acelera
    public float deceleration = 16f;  // qué tan rápido frena
    public float turnSmooth = 0.08f;  // suaviza cambios de dirección (0 = inmediato)

    private Rigidbody rb;
    private Vector3 velSmoothDamp;    // buffer interno para SmoothDamp

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        if (!orientation) orientation = transform;
    }

    void OnEnable()  { if (moveAction) moveAction.action.Enable(); }
    void OnDisable() { if (moveAction) moveAction.action.Disable(); }

    void FixedUpdate()
    {
        Vector2 input = moveAction ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        // Dirección deseada en XZ según orientación/cámara
        Vector3 desiredDir = (orientation.right * input.x + orientation.forward * input.y);
        desiredDir.y = 0f;
        float inputMag = Mathf.Clamp01(desiredDir.magnitude);
        if (inputMag > 0f) desiredDir.Normalize();

        // Velocidad objetivo (con suavizado de giro)
        Vector3 currentFlat = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 targetFlat = desiredDir * (maxSpeed * inputMag);

        // Elegimos tasa de cambio (acelera si hay input, desacelera si no)
        float rate = (inputMag > 0f ? acceleration : deceleration);

        // Suaviza el giro + la progresión hacia la velocidad objetivo
        Vector3 smoothTarget = Vector3.SmoothDamp(currentFlat, targetFlat, ref velSmoothDamp, turnSmooth, Mathf.Infinity, Time.fixedDeltaTime);
        Vector3 newFlat = Vector3.MoveTowards(currentFlat, smoothTarget, rate * Time.fixedDeltaTime);

        rb.velocity = new Vector3(newFlat.x, rb.velocity.y, newFlat.z);
    }
}
