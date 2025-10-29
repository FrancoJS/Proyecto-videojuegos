using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Looking")]
    public float rotationSpeed = 20f;    // qué tan rápido mira hacia el mouse
    public bool usePhysicsRaycast = false;
    public float planeHeight = 0f;
    public LayerMask groundMask = ~0;

    private Rigidbody rb;
    private Vector3 moveInputWorld;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerMovement: no se encontró Rigidbody en el jugador.");
            enabled = false;
            return;
        }

        // Recomendado: en el Rigidbody marcar Freeze Rotation X/Z desde el Inspector
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1) Mirar al mouse (igual que antes)
        AimTowardsMouse();

        // 2) STRAFE: WASD en ejes de la cámara (NO en transform del jugador)
        float h = (Input.GetKey(KeyCode.A) ? -1f : 0f) + (Input.GetKey(KeyCode.D) ? 1f : 0f);
        float v = (Input.GetKey(KeyCode.S) ? -1f : 0f) + (Input.GetKey(KeyCode.W) ? 1f : 0f);

        // Usa la cámara para definir "arriba" y "derecha" del movimiento
        Camera cam = Camera.main;
        Vector3 camFwd = Vector3.forward;
        Vector3 camRight = Vector3.right;

        if (cam != null)
        {
            // 1) Calcular camRight proyectado a XZ (si sale casi 0 usamos Vector3.right)
            camRight = cam.transform.right;
            camRight.y = 0f;
            if (camRight.sqrMagnitude > 0.0001f) camRight.Normalize();
            else camRight = Vector3.right;

            // 2) Intentar proyectar forward a XZ. Si la proyección es casi cero (cam top-down),
            //    reconstruimos forward usando únicamente el yaw (rotación Y) de la cámara.
            camFwd = cam.transform.forward;
            camFwd.y = 0f;

            if (camFwd.sqrMagnitude > 0.0001f)
            {
                camFwd.Normalize();
            }
            else
            {
                // fallback: construir forward desde la rotación Y (yaw)
                float yaw = cam.transform.eulerAngles.y;
                camFwd = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                camFwd.Normalize();
            }
        }
        else
        {
            // si no hay cámara marcada como MainCamera, usa ejes del mundo
            camFwd = Vector3.forward;
            camRight = Vector3.right;
        }

        // Movimiento en plano XZ alineado a la cámara (o mundo si no hay cámara)
        moveInputWorld = (camRight * h + camFwd * v);
        if (moveInputWorld.sqrMagnitude > 1e-6f)
            moveInputWorld.Normalize();
    }

    void FixedUpdate()
    {
        // mover con física para evitar tunneling
        rb.MovePosition(rb.position + moveInputWorld * moveSpeed * Time.fixedDeltaTime);
    }

    private void AimTowardsMouse()
    {
        Camera cam = Camera.main;
        if (!cam) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3? targetPoint = null;

        if (!usePhysicsRaycast)
        {
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));
            if (groundPlane.Raycast(ray, out float enter))
                targetPoint = ray.GetPoint(enter);
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 2000f, groundMask))
                targetPoint = hit.point;
        }

        if (targetPoint.HasValue)
        {
            Vector3 lookDir = targetPoint.Value - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.deltaTime));
            }
        }
    }
}
