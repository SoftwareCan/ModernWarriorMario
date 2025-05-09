using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Sahne yönetimi için

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider; // Ses kaydýrýcýsý
    [SerializeField] private TextMeshProUGUI playerHealth; // Oyuncu can metni
    [SerializeField] private TextMeshProUGUI goldText; // Altýn metni
    [SerializeField] private PlayerManager playerManager; // Oyuncu yöneticisi
    [SerializeField] private GameObject gameOverPanel; // Oyun bitti paneli
    private BackgroundMusicManager musicManager;
    private GoldManager goldManager; 

    private void Awake()
    {
        musicManager = Object.FindFirstObjectByType<BackgroundMusicManager>(); // Arka plan müzik yöneticisini bul
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

        if (gameOverPanel == null)
        {
            Debug.LogError("UIManager: GameOverPanel atanmamýþ!!!");
            return;
        }
    }

    private void OnEnable()
    {
        // GoldManager’ýn event’ine abone ol
        if (goldManager != null)
        {
            GoldManager.OnGoldChanged += UpdateGoldText;
        }
    }

    private void OnDisable()
    {
        // Event aboneliðini kaldýr
        if (goldManager != null)
        {
            GoldManager.OnGoldChanged -= UpdateGoldText;
        }
    }

    private void Start()
    {
        volumeSlider.value = 0.5f; // Varsayýlan ses seviyesi
        volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        gameOverPanel.SetActive(false); // Oyun bitti panelini baþlangýçta gizle

        // Baþlangýç altýn miktarýný göster
        if (goldManager != null && goldText != null)
        {
            UpdateGoldText(goldManager.GetGold());
        }
    }

    private void Update()
    {
        // Fare Slider üzerinde deðilse ve sol tuþ býrakýlmýþsa, EventSystem'den seçimi kaldýr
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
            goldText.text = $"Altýn: {gold}";
        }
    }

    public void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true); // Oyun bitti panelini göster
        Time.timeScale = 0f; // Oyunu duraklat
    }

    public void RestartGame()
    {
        Debug.Log($"RestartGame çaðrýldý, Time.timeScale: {Time.timeScale}, Yükleniyor: SampleScene");
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
        Time.timeScale = 1f; // Zaman akýþýný normale döndür
        SceneManager.LoadScene("MainMenu"); // Ana menü sahnesine geçiþ
    }
}
