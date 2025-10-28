using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;         // Velocidad de desplazamiento

    [Header("Looking")]
    public float rotationSpeed = 20f;    // Qué tan rápido gira hacia el mouse
    public LayerMask groundMask = ~0;    // Capa del piso (opcional, puedes dejar ~0)

    private Rigidbody rb;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // evita que se vuelque al chocar
    }

    void Update()
    {
        // WASD constantes (no dependen del giro del corazón)
        float h = (Input.GetKey(KeyCode.A) ? -1f : 0f) + (Input.GetKey(KeyCode.D) ? 1f : 0f);
        float v = (Input.GetKey(KeyCode.S) ? -1f : 0f) + (Input.GetKey(KeyCode.W) ? 1f : 0f);
        moveInput = new Vector3(h, 0f, v).normalized;

        // Rotación hacia el mouse usando raycast sobre un plano de suelo
        AimTowardsMouse();
    }

    void FixedUpdate()
    {
        // Mover por física
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    private void AimTowardsMouse()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Usamos un plano horizontal (Y=0). Si tu suelo no está en Y=0, ajusta el punto del plano.
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            // Si quieres restringir el ray al piso real en vez de un plano:
            // if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask)) { hitPoint = hit.point; }

            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(lookDir, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.deltaTime));
            }
        }
    }
}
