using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StarCollectible : MonoBehaviour
{
    [Header("Sonido al recolectar")]
    public AudioClip PickupStar;  // 🎵 Asigna el clip en el Inspector
    [Range(0f, 1f)] public float volumen = 1f;

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

        // 🔊 Reproducir sonido independiente (NO se corta aunque se destruya)
        if (PickupStar)
        {
            AudioSource.PlayClipAtPoint(PickupStar, transform.position, volumen);
        }

        // 🟡 Notificar al GameManager
        GameManager.Instance?.CollectStar();

        // 💥 Destruir la estrella
        Destroy(gameObject);
    }
}
