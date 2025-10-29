using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // si usas TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TMP_Text starsText;  // arrastra el texto desde el Canvas
    public GameObject winText;
    public string nextSceneName; // nombre de la siguiente escena (opcional)

    int collected = 0;
    int total = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Cuenta todas las estrellas en la escena
        total = GameObject.FindGameObjectsWithTag("Star").Length;
        UpdateUI();
    }

    public void CollectStar()
    {
        collected++;
        UpdateUI();

    if (collected >= total)
    {
        Debug.Log("¡Nivel completado! Todas las estrellas recogidas.");
        if (winText) winText.SetActive(true);
    }

    }

    void UpdateUI()
    {
        if (starsText != null)
            starsText.text = "Estrellas: " + collected + " / " + total;
    }
}
