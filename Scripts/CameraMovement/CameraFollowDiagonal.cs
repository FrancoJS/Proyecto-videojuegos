using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowDiagonal : MonoBehaviour
{
    [Header("Target")]
    public Transform player;         
    public float playerHeight = 1f;  

    [Header("Camera Settings")]
    public float distanceBack = 15f; 
    public float smoothTime = 0.3f;  

    private Vector3 velocity = Vector3.zero;

    private readonly Vector3 fixedRotation = new Vector3(77.0047226f, 0.107593283f, 359.807007f);

    void LateUpdate()
    {
        if (player == null) return;

        Quaternion rotationQuat = Quaternion.Euler(fixedRotation);

        Vector3 targetPos = player.position + Vector3.up * playerHeight + rotationQuat * new Vector3(0f, 0f, -distanceBack);

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        transform.rotation = rotationQuat;
    }
}
