using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI playerHealth;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI scoreText; // Yeni
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GameObject gameOverPanel;

    private BackgroundMusicManager musicManager;
    private GoldManager goldManager;
    private int score; // Yeni

    private void Awake()
    {
        musicManager = Object.FindFirstObjectByType<BackgroundMusicManager>();
        if (musicManager == null)
        {
            Debug.LogError("UIManager: BackgroundMusicManager bulunamadý!");
            return;
        }

        goldManager = Object.FindFirstObjectByType<GoldManager>();
        if (goldManager == null)
        {
            Debug.LogError("UIManager: GoldManager bulunamadý!");
        }


        if (playerHealth == null)
        {
            Debug.LogError("UIManager: PlayerHealth TextMeshProUGUI atanmamýþ!");
            return;
        }

        if (goldText == null)
        {
            Debug.LogError("UIManager: GoldText TextMeshProUGUI atanmamýþ!");
            return;
        }

        if (scoreText == null)
        {
            Debug.LogError("UIManager: ScoreText TextMeshProUGUI atanmamýþ!");
            return;
        }

        if (gameOverPanel == null)
        {
            Debug.LogError("UIManager: GameOverPanel atanmamýþ!");
            return;
        }
    }

    private void OnEnable()
    {
        if (goldManager != null)
        {
            GoldManager.OnGoldChanged += UpdateGoldText;
        }
        PlayerManager.OnPlayerDied += HandlePlayerDeath; // Yeni
        EnemyManager.OnEnemyDied += IncrementScore; // Yeni
    }

    private void OnDisable()
    {
        if (goldManager != null)
        {
            GoldManager.OnGoldChanged -= UpdateGoldText;
        }
        PlayerManager.OnPlayerDied -= HandlePlayerDeath; // Yeni
        EnemyManager.OnEnemyDied -= IncrementScore; // Yeni
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);

        if (goldManager != null && goldText != null)
        {
            UpdateGoldText(goldManager.GetGold());
        }

        score = 0;
        UpdateScoreText();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

   

    private void IncrementScore()
    {
        score+=100;
        UpdateScoreText();
        Debug.Log($"Skor artýrýldý: {score}");
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Skor: {score}";
        }
    }

    private void HandlePlayerDeath()
    {
        string username = PlayerPrefs.GetString("LastUsername", "Player");
        Debug.Log($"Skor kaydediliyor: {username} - {score}");
        MainMenuManager.SaveHighScore(username, score);
        ShowGameOverScreen();
    }

    public void UpdateHealthText(float health)
    {
        if (playerHealth != null)
        {
            playerHealth.text = $"Can: {health}";
        }
    }

    private void UpdateGoldText(int gold)
    {
        if (goldText != null)
        {
            goldText.text = $"Altýn: {gold}";
        }
    }

    public void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Debug.Log($"RestartGame çaðrýldý, Time.timeScale: {Time.timeScale}");
        Time.timeScale = 1f;
        try
        {
            SceneManager.LoadScene("SampleScene");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Sahne yükleme hatasý: {e.Message}");
        }
    }

    public void GoToMainMenu()
    {
        Debug.Log("MainMenu’ya geçiliyor...");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}