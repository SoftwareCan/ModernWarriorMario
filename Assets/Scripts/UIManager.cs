using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI playerHealth;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button musicToggleButton; // Buton
    [SerializeField] private GameObject musicOnImage; // Açýk ses sprite’ý
    [SerializeField] private GameObject musicOffImage; // Kapalý ses sprite’ý
    private BackgroundMusicManager musicManager;
    private GoldManager goldManager;
    private int score;
    private bool isMusicOn = true; // Müzik baþlangýçta açýk

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

        if (volumeSlider == null)
        {
            Debug.LogError("UIManager: Volume Slider atanmamýþ!");
            return;
        }

        if (playerHealth == null)
        {
            Debug.LogError("UIManager: PlayerHealth TextMeshProUGUI atanmamýþ!");
            return;
        }

        if (goldText == null)
        {
            Debug.LogError("UIManager: GoldText TextMeshProUGUI atanmamýþ!");
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

        if (pauseButton == null)
        {
            Debug.LogError("UIManager: PauseButton atanmamýþ!");
            return;
        }

        if (pausePanel == null)
        {
            Debug.LogError("UIManager: PausePanel atanmamýþ!");
            return;
        }

        if (resumeButton == null)
        {
            Debug.LogError("UIManager: ResumeButton atanmamýþ!");
            return;
        }

        if (mainMenuButton == null)
        {
            Debug.LogError("UIManager: MainMenuButton atanmamýþ!");
            return;
        }

        if (musicToggleButton == null)
        {
            Debug.LogError("UIManager: MusicToggleButton atanmamýþ!");
            return;
        }

        if (musicOnImage == null)
        {
            Debug.LogError("UIManager: MusicOnImage atanmamýþ!");
            return;
        }

        if (musicOffImage == null)
        {
            Debug.LogError("UIManager: MusicOffImage atanmamýþ!");
            return;
        }
    }

    private void OnEnable()
    {
        if (goldManager != null)
        {
            GoldManager.OnGoldChanged += UpdateGoldText;
        }
        PlayerManager.OnPlayerDied += HandlePlayerDeath;
        EnemyManager.OnEnemyDied += IncrementScore;
    }

    private void OnDisable()
    {
        if (goldManager != null)
        {
            GoldManager.OnGoldChanged -= UpdateGoldText;
        }
        PlayerManager.OnPlayerDied -= HandlePlayerDeath;
        EnemyManager.OnEnemyDied -= IncrementScore;
    }

    private void Start()
    {
        
        volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        pauseButton.onClick.AddListener(PauseGame);
        resumeButton.onClick.AddListener(ResumeGame);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        musicToggleButton.onClick.AddListener(ToggleMusic);

        if (goldManager != null && goldText != null)
        {
            UpdateGoldText(goldManager.GetGold());
        }

        score = 0;
        UpdateScoreText();
        UpdateMusicButtonSprite();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void SetMusicVolume(float volume)
    {
        if (musicManager != null)
        {
            musicManager.SetVolume(volume);
            Debug.Log($"Ses seviyesi ayarlandý: {volume}");
        }
    }

    public void ToggleMusic()
    {
        if (musicManager == null)
        {
            Debug.LogError("BackgroundMusicManager null!");
            return;
        }

        if (isMusicOn)
        {
            musicManager.PauseMusic();
            isMusicOn = false;
        }
        else
        {
            musicManager.PlayMusic();
            isMusicOn = true;
        }

        UpdateMusicButtonSprite();
        Debug.Log($"Müzik durumu: {(isMusicOn ? "Açýk" : "Kapalý")}");
    }

    private void UpdateMusicButtonSprite()
    {
        if (musicOnImage != null && musicOffImage != null)
        {
            musicOnImage.SetActive(isMusicOn);
            musicOffImage.SetActive(!isMusicOn);
            Debug.Log($"Sprite güncellendi: {(isMusicOn ? "MusicOnImage aktif" : "MusicOffImage aktif")}");
        }
    }

    private void IncrementScore()
    {
        score += 100;
        UpdateScoreText();
        Debug.Log($"Skor artýrýldý: {score}");
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void HandlePlayerDeath()
    {
        string username = PlayerPrefs.GetString("LastUsername", "Player");
        Debug.Log($"Skor kaydediliyor: {username} - {score}");
        MainMenuManager.SaveHighScore(username, score);
    }

    public void UpdateHealthText(float health)
    {
        if (playerHealth != null)
        {
            playerHealth.text = $"Health: {health}";
        }
    }

    private void UpdateGoldText(int gold)
    {
        if (goldText != null)
        {
            goldText.text = $"Gold: {gold}";
        }
    }

    public void ShowGameOverScreen()
    {
        Debug.Log("ShowGameOverScreen çaðrýldý!");
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(true);
        gameOverPanel.transform.SetAsLastSibling();
        Time.timeScale = 0f;
        Debug.Log("GameOver ekraný gösterildi!");
    }

    public void PauseGame()
    {
        if (Time.timeScale == 0f) return;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        pausePanel.transform.SetAsLastSibling();
        Debug.Log("Oyun duraklatýldý!");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        Debug.Log("Oyun devam ediyor!");
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