using UnityEngine;

namespace AltCamera
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollowTopDownAlt : MonoBehaviour
    {
        [Header("Target")]
        public Transform followTarget;

        [Header("Follow")]
        public Vector3 followOffset = new Vector3(0f, 18f, 0f);
        public float followSmoothTime = 0.12f;
        Vector3 followVelocity = Vector3.zero;

        [Header("Lookahead (opcional)")]
        public Rigidbody followTargetRb3D;
        public Rigidbody2D followTargetRb2D;
        public bool useRb2D = false;
        public float lookaheadFactor = 0.35f;
        public float lookaheadMax = 2f;

        [Header("Bounds (map limits) - define el rectángulo del mapa en XZ")]
        public bool enableBounds = true;
        public Vector2 mapMinXZ = new Vector2(-50f, -50f); // (minX, minZ)
        public Vector2 mapMaxXZ = new Vector2(50f, 50f);   // (maxX, maxZ)

        [Header("Camera Settings")]
        public bool forceTopDownRotation = true;
        public bool useOrthographic = true;
        public float orthoSizeNormal = 12f;
        public float orthoSizeZoomed = 18f;
        public float zoomLerpSpeed = 8f;

        [Header("Perspective (si no es ortográfica)")]
        public float perspFOV = 60f;
        public float perspFOVZoomed = 75f;

        [Header("Input")]
        public KeyCode zoomHoldKey = KeyCode.LeftShift;

        Camera _cameraRef;

        void Awake()
        {
            _cameraRef = GetComponent<Camera>();
            if (_cameraRef == null) _cameraRef = Camera.main;
        }

        void Start()
        {
            if (_cameraRef != null && useOrthographic)
            {
                _cameraRef.orthographic = true;
                _cameraRef.orthographicSize = orthoSizeNormal;
            }
            else if (_cameraRef != null)
            {
                _cameraRef.fieldOfView = perspFOV;
            }

            if (forceTopDownRotation)
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            if (followTarget != null)
            {
                if (followTargetRb3D == null)
                    followTargetRb3D = followTarget.GetComponent<Rigidbody>();

                if (followTargetRb2D == null)
                    followTargetRb2D = followTarget.GetComponent<Rigidbody2D>();

                if (followTargetRb2D != null)
                    useRb2D = true;
            }
        }

        void LateUpdate()
        {
            if (followTarget == null) return;

            // 1) Calcular follow base
            Vector3 basePoint = followTarget.position;

            // 2) Lookahead
            if (lookaheadFactor > 0f)
            {
                Vector3 ahead = Vector3.zero;
                if (!useRb2D && followTargetRb3D != null)
                {
                    Vector3 v = followTargetRb3D.velocity;
                    Vector3 horiz = new Vector3(v.x, 0f, v.z);
                    ahead = Vector3.ClampMagnitude(horiz * lookaheadFactor, lookaheadMax);
                }
                else if (useRb2D && followTargetRb2D != null)
                {
                    Vector2 v2 = followTargetRb2D.velocity;
                    Vector3 horiz = new Vector3(v2.x, 0f, v2.y);
                    ahead = Vector3.ClampMagnitude(horiz * lookaheadFactor, lookaheadMax);
                }
                basePoint += ahead;
            }

            // 3) Desired position (center) con offset
            Vector3 desired = basePoint + followOffset;

            // 4) Suavizado
            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref followVelocity, followSmoothTime);

            // 5) Clamp teniendo en cuenta extents del viewport (si bounds activado)
            if (enableBounds && _cameraRef != null)
            {
                if (_cameraRef.orthographic)
                {
                    float vertExtent = _cameraRef.orthographicSize;
                    float horizExtent = vertExtent * _cameraRef.aspect;

                    // limites centrales permitidos (para que los bordes no salgan)
                    float minCenterX = mapMinXZ.x + horizExtent;
                    float maxCenterX = mapMaxXZ.x - horizExtent;
                    float minCenterZ = mapMinXZ.y + vertExtent;
                    float maxCenterZ = mapMaxXZ.y - vertExtent;

                    // Si el mapa es más pequeño que la vista, centrar en el mapa
                    if (minCenterX > maxCenterX)
                    {
                        float centerX = (mapMinXZ.x + mapMaxXZ.x) * 0.5f;
                        smoothed.x = centerX;
                    }
                    else
                    {
                        smoothed.x = Mathf.Clamp(smoothed.x, minCenterX, maxCenterX);
                    }

                    if (minCenterZ > maxCenterZ)
                    {
                        float centerZ = (mapMinXZ.y + mapMaxXZ.y) * 0.5f;
                        smoothed.z = centerZ;
                    }
                    else
                    {
                        smoothed.z = Mathf.Clamp(smoothed.z, minCenterZ, maxCenterZ);
                    }
                }
                else
                {
                    // Para perspectiva, usamos una aproximación usando el ortho-equivalente vertical basado en FOV y distancia.
                    // Esto es conservador: evita que la cámara salga del mapa en la mayoría de casos top-down.
                    float camDistance = Mathf.Abs(smoothed.y - followOffset.y); // aproximación simple
                    float halfFOV = (_cameraRef.fieldOfView * 0.5f) * Mathf.Deg2Rad;
                    float vertExtent = camDistance * Mathf.Tan(halfFOV);
                    float horizExtent = vertExtent * _cameraRef.aspect;

                    float minCenterX = mapMinXZ.x + horizExtent;
                    float maxCenterX = mapMaxXZ.x - horizExtent;
                    float minCenterZ = mapMinXZ.y + vertExtent;
                    float maxCenterZ = mapMaxXZ.y - vertExtent;

                    if (minCenterX > maxCenterX)
                        smoothed.x = (mapMinXZ.x + mapMaxXZ.x) * 0.5f;
                    else
                        smoothed.x = Mathf.Clamp(smoothed.x, minCenterX, maxCenterX);

                    if (minCenterZ > maxCenterZ)
                        smoothed.z = (mapMinXZ.y + mapMaxXZ.y) * 0.5f;
                    else
                        smoothed.z = Mathf.Clamp(smoothed.z, minCenterZ, maxCenterZ);
                }
            }

            transform.position = smoothed;

            // 6) Zoom con Shift
            bool zoomPressed = Input.GetKey(zoomHoldKey) || Input.GetKey(KeyCode.RightShift);
            if (_cameraRef != null)
            {
                if (_cameraRef.orthographic)
                {
                    float targetSize = zoomPressed ? orthoSizeZoomed : orthoSizeNormal;
                    _cameraRef.orthographicSize = Mathf.Lerp(_cameraRef.orthographicSize, targetSize, Time.deltaTime * zoomLerpSpeed);
                }
                else
                {
                    float targetFOV = zoomPressed ? perspFOVZoomed : perspFOV;
                    _cameraRef.fieldOfView = Mathf.Lerp(_cameraRef.fieldOfView, targetFOV, Time.deltaTime * zoomLerpSpeed);
                }
            }
        }

        // utilidad para asignar mapa en runtime
        public void SetMapBounds(Rect mapRectXZ)
        {
            mapMinXZ = new Vector2(mapRectXZ.xMin, mapRectXZ.yMin);
            mapMaxXZ = new Vector2(mapRectXZ.xMax, mapRectXZ.yMax);
        }
    }
}
