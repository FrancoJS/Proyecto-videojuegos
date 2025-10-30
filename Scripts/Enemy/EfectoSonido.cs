using UnityEngine;

public class EfectoSonido : MonoBehaviour
{
    [Header("Sonido que se reproducirá al colisionar o al entrar en trigger")]
    public AudioClip sonido;
    [Range(0f, 1f)] public float volumen = 1f;

    private AudioSource fuenteAudio;
    private bool sonidoReproducido = false; // 👈 evita que se repita

    void Start()
    {
        // Usa un AudioSource existente o crea uno nuevo
        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio == null)
            fuenteAudio = gameObject.AddComponent<AudioSource>();

        fuenteAudio.playOnAwake = false;
    }

    // Para colisiones físicas
    void OnCollisionEnter(Collision col)
    {
        if (!sonidoReproducido && sonido != null)
        {
            fuenteAudio.PlayOneShot(sonido, volumen);
            sonidoReproducido = true; // 🔒 solo una vez
        }
    }

    // Para triggers (IsTrigger = true)
    void OnTriggerEnter(Collider other)
    {
        if (!sonidoReproducido && sonido != null)
        {
            fuenteAudio.PlayOneShot(sonido, volumen);
            sonidoReproducido = true; // 🔒 solo una vez
        }
    }
}
