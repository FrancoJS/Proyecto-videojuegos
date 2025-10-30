using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] TMP_Text starsText;      // Asigna StarsText
    [SerializeField] GameObject winPanel;     // Asigna WinPanel
    [SerializeField] TMP_Text winText;        // Asigna WinText (el texto grande del panel)
    [SerializeField] Button nextButton;       // Asigna NextButton (opcional)

    [Header("Flujo")]
    [SerializeField] string nextSceneName = ""; // opcional, siguiente nivel explícito

    int collected = 0;
    int total = 0;
    bool isLastLevel = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        total = GameObject.FindGameObjectsWithTag("Star").Length;

        // Es último nivel si no hay nextSceneName y este es el último en Build Settings
        int idx = SceneManager.GetActiveScene().buildIndex;
        isLastLevel = string.IsNullOrEmpty(nextSceneName) &&
                      idx >= SceneManager.sceneCountInBuildSettings - 1;

        if (winPanel) winPanel.SetActive(false);
        if (nextButton) nextButton.gameObject.SetActive(false);

        UpdateUI();
    }

    public void CollectStar()
    {
        collected++;
        UpdateUI();

        if (collected >= total)
        {
            if (winPanel) winPanel.SetActive(true);

            if (isLastLevel)
            {
                if (winText) winText.text = "¡Juego superado!";
                if (nextButton) nextButton.gameObject.SetActive(false); // ocultar “Siguiente nivel”
            }
            else
            {
                if (winText) winText.text = "¡Nivel completado!";
                if (nextButton) nextButton.gameObject.SetActive(true);
            }
        }
    }

    public void LoadNextLevel()
    {
        if (isLastLevel) return; // por seguridad, no hay siguiente

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        int index = SceneManager.GetActiveScene().buildIndex;
        int next = index + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            Debug.Log("No hay más niveles.");
    }

    void UpdateUI()
    {
        if (starsText) starsText.text = $"Estrellas: {collected} / {total}";
    }
}
