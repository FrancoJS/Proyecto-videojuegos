using System.Collections;
using UnityEngine;

public class Enemymovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float velocidad = 3f;                 // qué tan rápido se mueve
    [SerializeField] float distancia = 5f;                 // distancia total del recorrido
    [SerializeField] Vector3 direccion = Vector3.right;    // Right=horizontal, Up=vertical, Forward=profundidad

    private Vector3 puntoInicio;
    private Vector3 puntoDestino;
    private bool avanzando = true;
    private bool listo = false;

    IEnumerator Start()
    {
        // Espera aleatoria antes de comenzar (rompe sincronización)
        yield return new WaitForSeconds(Random.Range(0f, 2f));

        // Configura puntos base
        puntoInicio = transform.position;
        Vector3 dirNorm = direccion.sqrMagnitude > 0.0001f ? direccion.normalized : Vector3.right;
        puntoDestino = puntoInicio + dirNorm * distancia;

        listo = true;
    }

    void Update()
    {
        if (!listo) return;

        // Selecciona el destino actual
        Vector3 objetivo = avanzando ? puntoDestino : puntoInicio;
        transform.position = Vector3.MoveTowards(transform.position, objetivo, velocidad * Time.deltaTime);

        // Cambia de dirección al llegar al extremo
        if (Vector3.Distance(transform.position, objetivo) < 0.05f)
            avanzando = !avanzando;
    }

    // Dibuja en el editor la línea de recorrido (opcional)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 start = Application.isPlaying ? puntoInicio : transform.position;
        Vector3 end = Application.isPlaying ? puntoDestino : transform.position + (direccion.sqrMagnitude > 0.0001f ? direccion.normalized : Vector3.right) * distancia;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(start, 0.08f);
        Gizmos.DrawSphere(end, 0.08f);
    }
}
