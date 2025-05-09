using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button closeSettingsButton;

    private BackgroundMusicManager musicManager;

    private void Awake()
    {
        // Referans kontrolleri  
        if (playButton == null) Debug.LogError("MainMenuManager: PlayButton atanmamýþ!");
        if (settingsButton == null) Debug.LogError("MainMenuManager: SettingsButton atanmamýþ!");
        if (quitButton == null) Debug.LogError("MainMenuManager: QuitButton atanmamýþ!");
        if (settingsPanel == null) Debug.LogError("MainMenuManager: SettingsPanel atanmamýþ!");
        if (volumeSlider == null) Debug.LogError("MainMenuManager: VolumeSlider atanmamýþ!");
        if (closeSettingsButton == null) Debug.LogError("MainMenuManager: CloseSettingsButton atanmamýþ!");

        // MusicManager'ý bul  
        musicManager = Object.FindFirstObjectByType<BackgroundMusicManager>();
        if (musicManager == null)
        {
            Debug.LogWarning("MainMenuManager: BackgroundMusicManager bulunamadý!");
        }
    }

    private void Start()
    {
        // Butonlara iþlev baðla
        playButton.onClick.AddListener(PlayGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);
        closeSettingsButton.onClick.AddListener(CloseSettings);

        // Ses ayarlarýný yükle
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 0.5f);
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // SettingsPanel baþlangýçta kapalý
        settingsPanel.SetActive(false);
    }

    private void PlayGame()
    {
        Debug.Log("Oyun baþlatýlýyor...");
        SceneManager.LoadScene("SampleScene");
    }

    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    private void SetMusicVolume(float volume)
    {
        Debug.Log($"Ses seviyesi: {volume}");
        if (musicManager != null)
        {
            musicManager.SetVolume(volume);
        }
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
    }

    private void QuitGame()
    {
        Debug.Log("Oyun kapatýlýyor...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}