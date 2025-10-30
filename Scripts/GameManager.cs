using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] TMP_Text starsText;     // Asigna StarsText
    [SerializeField] GameObject winPanel;    // Asigna WinPanel (contiene WinText y el botón)
    [SerializeField] Button nextButton;      // Asigna el botón NextButton

    [Header("Flujo")]
    [SerializeField] string nextSceneName = ""; // opcional, siguiente nivel

    int collected = 0;
    int total = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        total = GameObject.FindGameObjectsWithTag("Star").Length;

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
            Debug.Log("Nivel completado: todas las estrellas recogidas");
            if (winPanel) winPanel.SetActive(true);
            if (nextButton) nextButton.gameObject.SetActive(true);
        }
    }

    public void LoadNextLevel()
    {
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
        if (starsText)
            starsText.text = $"Estrellas: {collected} / {total}";
    }
}
