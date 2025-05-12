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
    [SerializeField] private Button musicToggleButton;
    [SerializeField] private GameObject musicOnImage;
    [SerializeField] private GameObject musicOffImage;
    [SerializeField] private HintDoorManager hintDoorManager;

    private BackgroundMusicManager musicManager;
    private GoldManager goldManager;
    private int score;
    private bool isMusicOn = true;

    private void Awake()
    {
        musicManager = Object.FindFirstObjectByType<BackgroundMusicManager>();
        if (musicManager == null)
        {
            Debug.LogError("UIManager: BackgroundMusicManager bulunamadý!");
        }

        goldManager = Object.FindFirstObjectByType<GoldManager>();
        if (goldManager == null)
        {
            Debug.LogError("UIManager: GoldManager bulunamadý!");
        }

        if (hintDoorManager == null)
        {
            Debug.LogError("UIManager: HintDoorManager atanmamýþ!");
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

        CheckForKeyUsage();
    }

    private void CheckForKeyUsage()
    {
        if (hintDoorManager == null) return;

        RectTransform dropAreaRect = hintDoorManager.GetKeyDropArea();
        if (dropAreaRect == null) return;

        InventoryUI inventoryUI = Object.FindFirstObjectByType<InventoryUI>();
        if (inventoryUI == null) return;

        GameObject inventoryPanel = inventoryUI.gameObject;
        if (inventoryPanel == null || !inventoryPanel.activeSelf) return;

        Item keyItem = System.Array.Find(Resources.FindObjectsOfTypeAll<Item>(), i => i.itemID == "Key1");
        if (keyItem == null || !InventoryManager.Instance.HasItem(keyItem)) return;

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(dropAreaRect, Input.mousePosition, null, out localPoint))
            return;

        bool isInDropArea = dropAreaRect.rect.Contains(localPoint);
        float distance = Vector2.Distance(Input.mousePosition, dropAreaRect.position);
        bool isInDropAreaByDistance = distance < 50f; // Daha hassas mesafe  

        if ((isInDropArea || isInDropAreaByDistance) && Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (InventoryManager.Instance.RemoveItem(keyItem))
            {
                hintDoorManager.ChangeColorAndOpenWay();
                Debug.Log("Anahtar týklama ile kullanýldý, kapý açýldý!");
            }
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