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
        if (playButton == null) Debug.LogError("MainMenuManager: PlayButton atanmam��!");
        if (settingsButton == null) Debug.LogError("MainMenuManager: SettingsButton atanmam��!");
        if (quitButton == null) Debug.LogError("MainMenuManager: QuitButton atanmam��!");
        if (settingsPanel == null) Debug.LogError("MainMenuManager: SettingsPanel atanmam��!");
        if (volumeSlider == null) Debug.LogError("MainMenuManager: VolumeSlider atanmam��!");
        if (closeSettingsButton == null) Debug.LogError("MainMenuManager: CloseSettingsButton atanmam��!");

        // MusicManager'� bul  
        musicManager = Object.FindFirstObjectByType<BackgroundMusicManager>();
        if (musicManager == null)
        {
            Debug.LogWarning("MainMenuManager: BackgroundMusicManager bulunamad�!");
        }
    }

    private void Start()
    {
        // Butonlara i�lev ba�la
        playButton.onClick.AddListener(PlayGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);
        closeSettingsButton.onClick.AddListener(CloseSettings);

        // Ses ayarlar�n� y�kle
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 0.5f);
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // SettingsPanel ba�lang��ta kapal�
        settingsPanel.SetActive(false);
    }

    private void PlayGame()
    {
        Debug.Log("Oyun ba�lat�l�yor...");
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
        Debug.Log("Oyun kapat�l�yor...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}