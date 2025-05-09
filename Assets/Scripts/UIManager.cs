using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Sahne y�netimi i�in

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider; // Ses kayd�r�c�s�
    [SerializeField] private TextMeshProUGUI playerHealth; // Oyuncu can metni
    [SerializeField] private TextMeshProUGUI goldText; // Alt�n metni
    [SerializeField] private PlayerManager playerManager; // Oyuncu y�neticisi
    [SerializeField] private GameObject gameOverPanel; // Oyun bitti paneli
    private BackgroundMusicManager musicManager;
    private GoldManager goldManager; 

    private void Awake()
    {
        musicManager = Object.FindFirstObjectByType<BackgroundMusicManager>(); // Arka plan m�zik y�neticisini bul
        if (musicManager == null)
        {
            Debug.LogError("UIManager: BackgroundMusicManager bulunamad�!");
            return;
        }

        goldManager = Object.FindFirstObjectByType<GoldManager>();
        if (goldManager == null)
        {
            Debug.LogError("UIManager: GoldManager bulunamad�!");
        }

        if (volumeSlider == null)
        {
            Debug.LogError("UIManager: Volume Slider atanmam��!");
            return;
        }

        if (playerHealth == null)
        {
            Debug.LogError("UIManager: PlayerHealth TextMeshProUGUI atanmam��!");
            return;
        }

        if (goldText == null)
        {
            Debug.LogError("UIManager: GoldText TextMeshProUGUI atanmam��!");
        }

        if (gameOverPanel == null)
        {
            Debug.LogError("UIManager: GameOverPanel atanmam��!!!");
            return;
        }
    }

    private void OnEnable()
    {
        // GoldManager��n event�ine abone ol
        if (goldManager != null)
        {
            GoldManager.OnGoldChanged += UpdateGoldText;
        }
    }

    private void OnDisable()
    {
        // Event aboneli�ini kald�r
        if (goldManager != null)
        {
            GoldManager.OnGoldChanged -= UpdateGoldText;
        }
    }

    private void Start()
    {
        volumeSlider.value = 0.5f; // Varsay�lan ses seviyesi
        volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        gameOverPanel.SetActive(false); // Oyun bitti panelini ba�lang��ta gizle

        // Ba�lang�� alt�n miktar�n� g�ster
        if (goldManager != null && goldText != null)
        {
            UpdateGoldText(goldManager.GetGold());
        }
    }

    private void Update()
    {
        // Fare Slider �zerinde de�ilse ve sol tu� b�rak�lm��sa, EventSystem'den se�imi kald�r
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
        }
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
            goldText.text = $"Alt�n: {gold}";
        }
    }

    public void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true); // Oyun bitti panelini g�ster
        Time.timeScale = 0f; // Oyunu duraklat
    }

    public void RestartGame()
    {
        Debug.Log($"RestartGame �a�r�ld�, Time.timeScale: {Time.timeScale}, Y�kleniyor: SampleScene");
        Time.timeScale = 1f;
        try
        {
            SceneManager.LoadScene("SampleScene");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Sahne y�kleme hatas�: {e.Message}");
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Zaman ak���n� normale d�nd�r
        SceneManager.LoadScene("MainMenu"); // Ana men� sahnesine ge�i�
    }
}
