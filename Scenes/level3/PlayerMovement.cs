using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;

    [Header("Looking")]
    public float rotationSpeed = 20f;    // rapidez de giro hacia el mouse
    public bool usePhysicsRaycast = false; // A=false (plano), B=true (raycast con colliders)
    public float planeHeight = 0f;         // Y del plano si usas el modo A
    public LayerMask groundMask = ~0;      // capas válidas si usas el modo B

    private Rigidbody rb;
    private Vector3 moveInputWorld; // vector final para mover

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // X/Z congeladas desde Constraints; Y libre para girar
    }

    void Update()
    {
        // 1) ROTAR HACIA EL MOUSE (igual que ya tenías)
        AimTowardsMouse();

        // 2) LEER WASD Y CONVERTIR A DIRECCIÓN RELATIVA AL JUGADOR
        // h: A(-1) D(+1)  |  v: S(-1) W(+1)
        float h = (Input.GetKey(KeyCode.A) ? -1f : 0f) + (Input.GetKey(KeyCode.D) ? 1f : 0f);
        float v = (Input.GetKey(KeyCode.S) ? -1f : 0f) + (Input.GetKey(KeyCode.W) ? 1f : 0f);

        // Direcciones LOCALES del jugador (se adaptan al giro):
        Vector3 forward = transform.forward; forward.y = 0f;
        Vector3 right   = transform.right;   right.y = 0f;

        // Combinar según WASD (W=adelante local, S=atrás, A=izq, D=der)
        moveInputWorld = (forward * v + right * h);
        if (moveInputWorld.sqrMagnitude > 1e-6f)
            moveInputWorld = moveInputWorld.normalized;
    }

    void FixedUpdate()
    {
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
            // MODO A: plano horizontal a una altura fija
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));
            if (groundPlane.Raycast(ray, out float enter))
                targetPoint = ray.GetPoint(enter);
        }
        else
        {
            // MODO B: raycast contra colliders (marcados en groundMask)
            if (Physics.Raycast(ray, out RaycastHit hit, 2000f, groundMask))
                targetPoint = hit.point;
        }

        if (targetPoint.HasValue)
        {
            Vector3 lookDir = targetPoint.Value - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(lookDir, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.deltaTime));
            }
        }
    }
}
