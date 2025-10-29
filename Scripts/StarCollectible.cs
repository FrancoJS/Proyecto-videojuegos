using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StarCollectible : MonoBehaviour
{
    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; // aseguramos que sea trigger
    }

    void OnTriggerEnter(Collider other)
    {
        // detecta jugador incluso si el collider viene de un hijo
        bool isPlayer = other.CompareTag("Player") ||
                        (other.attachedRigidbody && other.attachedRigidbody.CompareTag("Player"));

        if (!isPlayer) return;

        Debug.Log("⭐ Star tocada por: " + other.name);
        GameManager.Instance?.CollectStar();
        Destroy(gameObject);
    }
}
