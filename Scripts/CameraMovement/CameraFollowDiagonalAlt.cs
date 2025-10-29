using UnityEngine;

namespace AltCamera
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollowDiagonalAlt : MonoBehaviour
    {
        [Header("Target")]
        public Transform followTarget;

        [Header("Follow")]
        public Vector3 followOffset = new Vector3(0f, 18f, -6f); // Y = altura, Z = alejamiento (para vista diagonal)
        public float followSmoothTime = 0.12f;
        Vector3 followVelocity = Vector3.zero;

        [Header("Lookahead (opcional)")]
        public Rigidbody followTargetRb3D;
        public Rigidbody2D followTargetRb2D;
        public bool useRb2D = false;
        public float lookaheadFactor = 0.35f;
        public float lookaheadMax = 2f;

        [Header("Map bounds (X,Z)")]
        public bool enableBounds = true;
        public Vector2 mapMinXZ = new Vector2(-50f, -50f);
        public Vector2 mapMaxXZ = new Vector2(50f, 50f);

        [Header("Camera orientation")]
        [Tooltip("Euler angles de la cámara. Por ejemplo X = 45, Y = 30 para diagonal.")]
        public Vector3 cameraEuler = new Vector3(45f, 30f, 0f);
        [Tooltip("Altura del plano 'suelo' (Y) donde se proyectan los rays para calcular extents).")]
        public float groundPlaneY = 0f;

        [Header("Camera Settings")]
        public bool useOrthographic = true;
        public float orthoSizeNormal = 12f;
        public float orthoSizeZoomed = 18f;
        public float zoomLerpSpeed = 8f;

        [Header("Perspective (if not orthographic)")]
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
            // Aplicar rotación diagonal que definas en el Inspector
            transform.rotation = Quaternion.Euler(cameraEuler);

            if (_cameraRef != null)
            {
                if (useOrthographic)
                {
                    _cameraRef.orthographic = true;
                    _cameraRef.orthographicSize = orthoSizeNormal;
                }
                else
                {
                    _cameraRef.fieldOfView = perspFOV;
                }
            }

            // Autoget rigidbodies si hay target
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

            // 1) base a seguir
            Vector3 basePoint = followTarget.position;

            // 2) lookahead
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

            // 3) desired (center) world position for camera before smoothing
            Vector3 desiredCenter = basePoint + followOffset;

            // 4) smooth damp toward desired
            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desiredCenter, ref followVelocity, followSmoothTime);

            // 5) Clamp using viewport-corners projected onto ground plane at Y = groundPlaneY
            if (enableBounds && _cameraRef != null)
            {
                // Temporarily place camera at candidate smoothed position to compute corners intersections
                Vector3 prevPos = transform.position;
                transform.position = smoothed;

                Vector3[] corners;
                bool ok = GetViewportGroundIntersections(out corners);

                if (ok && corners != null && corners.Length == 4)
                {
                    // center projection on plane (viewport center)
                    Ray centerRay = _cameraRef.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                    Plane ground = new Plane(Vector3.up, new Vector3(0f, groundPlaneY, 0f));
                    ground.Raycast(centerRay, out float centerEnter);
                    Vector3 centerGround = centerRay.GetPoint(centerEnter);

                    float minCornerX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
                    float maxCornerX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
                    float minCornerZ = Mathf.Min(corners[0].z, corners[1].z, corners[2].z, corners[3].z);
                    float maxCornerZ = Mathf.Max(corners[0].z, corners[1].z, corners[2].z, corners[3].z);

                    float leftExtent = centerGround.x - minCornerX;
                    float rightExtent = maxCornerX - centerGround.x;
                    float bottomExtent = centerGround.z - minCornerZ;
                    float topExtent = maxCornerZ - centerGround.z;

                    float minCenterX = mapMinXZ.x + leftExtent;
                    float maxCenterX = mapMaxXZ.x - rightExtent;
                    float minCenterZ = mapMinXZ.y + bottomExtent;
                    float maxCenterZ = mapMaxXZ.y - topExtent;

                    // handle map smaller than view: center instead of clamp
                    if (minCenterX > maxCenterX) smoothed.x = (mapMinXZ.x + mapMaxXZ.x) * 0.5f;
                    else smoothed.x = Mathf.Clamp(smoothed.x, minCenterX, maxCenterX);

                    if (minCenterZ > maxCenterZ) smoothed.z = (mapMinXZ.y + mapMaxXZ.y) * 0.5f;
                    else smoothed.z = Mathf.Clamp(smoothed.z, minCenterZ, maxCenterZ);
                }

                // restore camera to previous pos (we will set it again below)
                transform.position = prevPos;
            }

            // 6) finally apply smoothed (clamped) position
            transform.position = smoothed;

            // 7) zoom handling
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

        // Devuelve true si todas las esquinas intersectaron el plano del suelo
        bool GetViewportGroundIntersections(out Vector3[] outCorners)
        {
            outCorners = new Vector3[4];
            Plane ground = new Plane(Vector3.up, new Vector3(0f, groundPlaneY, 0f));

            Vector3[] vp = new Vector3[4]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(1f, 1f, 0f)
            };

            for (int i = 0; i < 4; i++)
            {
                Ray r = _cameraRef.ViewportPointToRay(vp[i]);
                if (!ground.Raycast(r, out float enter))
                {
                    // si una esquina no choca con el plano (cam muy inclinada), fallamos
                    return false;
                }
                outCorners[i] = r.GetPoint(enter);
            }

            return true;
        }

        // utilidad para asignar mapa en runtime
        public void SetMapBounds(Rect mapRectXZ)
        {
            mapMinXZ = new Vector2(mapRectXZ.xMin, mapRectXZ.yMin);
            mapMaxXZ = new Vector2(mapRectXZ.xMax, mapRectXZ.yMax);
        }
    }
}

