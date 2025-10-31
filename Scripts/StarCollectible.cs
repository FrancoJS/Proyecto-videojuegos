using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StarCollectible : MonoBehaviour
{
    [Header("Sonido al recolectar")]
    public AudioClip PickupStar;  
    [Range(0f, 1f)] public float volumen = 1f;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; 
    }

    void OnTriggerEnter(Collider other)
    {
        
        bool isPlayer = other.CompareTag("Player") ||
                        (other.attachedRigidbody && other.attachedRigidbody.CompareTag("Player"));

        if (!isPlayer) return;

        Debug.Log("⭐ Star tocada por: " + other.name);

        if (PickupStar)
        {
            AudioSource.PlayClipAtPoint(PickupStar, transform.position, volumen);
        }

        GameManager.Instance?.CollectStar();

        Destroy(gameObject);
    }
}
