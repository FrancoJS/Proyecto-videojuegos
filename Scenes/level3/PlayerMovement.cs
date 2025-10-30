using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // para corrutinas

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;

    [Header("Looking")]
    public float rotationSpeed = 20f;    // qué tan rápido mira hacia el mouse
    public bool usePhysicsRaycast = false;
    public float planeHeight = 0f;
    public LayerMask groundMask = ~0;

    [Header("Muerte")]
    public float delayReinicio = 1f; // ⏱️ Cambiado a 1 segundo
    private bool muriendo = false;

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

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (muriendo) return;

        AimTowardsMouse();

        float h = (Input.GetKey(KeyCode.A) ? -1f : 0f) + (Input.GetKey(KeyCode.D) ? 1f : 0f);
        float v = (Input.GetKey(KeyCode.S) ? -1f : 0f) + (Input.GetKey(KeyCode.W) ? 1f : 0f);

        Camera cam = Camera.main;
        Vector3 camFwd = Vector3.forward;
        Vector3 camRight = Vector3.right;

        if (cam != null)
        {
            camRight = cam.transform.right; camRight.y = 0f;
            if (camRight.sqrMagnitude > 0.0001f) camRight.Normalize();
            else camRight = Vector3.right;

            camFwd = cam.transform.forward; camFwd.y = 0f;
            if (camFwd.sqrMagnitude > 0.0001f)
            {
                camFwd.Normalize();
            }
            else
            {
                float yaw = cam.transform.eulerAngles.y;
                camFwd = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                camFwd.Normalize();
            }
        }

        moveInputWorld = (camRight * h + camFwd * v);
        if (moveInputWorld.sqrMagnitude > 1e-6f)
            moveInputWorld.Normalize();
    }

    void FixedUpdate()
    {
        if (muriendo) return;

        Vector3 delta = moveInputWorld * moveSpeed * Time.fixedDeltaTime;
        if (delta.sqrMagnitude < 1e-8f)
            return;

        float skin = 0.02f;
        if (rb.SweepTest(delta.normalized, out RaycastHit hit, delta.magnitude + skin, QueryTriggerInteraction.Ignore))
        {
            Vector3 alongWall = Vector3.ProjectOnPlane(delta, hit.normal);
            if (alongWall.sqrMagnitude > 1e-8f &&
                rb.SweepTest(alongWall.normalized, out RaycastHit hit2, alongWall.magnitude + skin))
            {
                alongWall = alongWall.normalized * Mathf.Max(0f, hit2.distance - skin);
            }
            rb.MovePosition(rb.position + alongWall);
        }
        else
        {
            rb.MovePosition(rb.position + delta);
        }
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

    void OnTriggerEnter(Collider other)
    {
        if (muriendo) return;

        if (other.CompareTag("Enemy") ||
            (other.attachedRigidbody && other.attachedRigidbody.CompareTag("Enemy")))
        {
            StartCoroutine(EsperarYReiniciar());
        }
    }

    IEnumerator EsperarYReiniciar()
    {
        muriendo = true;

        // Espera 1 segundo antes de reiniciar
        yield return new WaitForSeconds(delayReinicio);

        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
    }
}
