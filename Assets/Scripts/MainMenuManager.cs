using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button highScoreButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button closeHighScoreButton;

    private BackgroundMusicManager musicManager;
    private const int MaxHighScores = 10;

    [System.Serializable]
    public struct HighScoreEntry
    {
        public string username;
        public int score;
    }

    private void Awake()
    {
        if (playButton == null) Debug.LogError("MainMenuManager: PlayButton atanmam��!");
        if (settingsButton == null) Debug.LogError("MainMenuManager: SettingsButton atanmam��!");
        if (quitButton == null) Debug.LogError("MainMenuManager: QuitButton atanmam��!");
        if (highScoreButton == null) Debug.LogError("MainMenuManager: HighScoreButton atanmam��!");
        if (settingsPanel == null) Debug.LogError("MainMenuManager: SettingsPanel atanmam��!");
        if (volumeSlider == null) Debug.LogError("MainMenuManager: VolumeSlider atanmam��!");
        if (closeSettingsButton == null) Debug.LogError("MainMenuManager: CloseSettingsButton atanmam��!");
        if (usernameInputField == null) Debug.LogError("MainMenuManager: UsernameInputField atanmam��!");
        if (highScorePanel == null) Debug.LogError("MainMenuManager: HighScorePanel atanmam��!");
        if (highScoreText == null) Debug.LogError("MainMenuManager: HighScoreText atanmam��!");
        if (closeHighScoreButton == null) Debug.LogError("MainMenuManager: CloseHighScoreButton atanmam��!");

        musicManager = Object.FindFirstObjectByType<BackgroundMusicManager>();
        if (musicManager == null)
        {
            Debug.LogWarning("MainMenuManager: BackgroundMusicManager bulunamad�!");
        }
    }

    private void Start()
    {
        playButton.onClick.AddListener(PlayGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitGame);
        highScoreButton.onClick.AddListener(OpenHighScore);
        closeSettingsButton.onClick.AddListener(CloseSettings);
        closeHighScoreButton.onClick.AddListener(CloseHighScore);

        if (volumeSlider != null)
        {
            volumeSlider.value = 0.5f;
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        settingsPanel.SetActive(false);
        highScorePanel.SetActive(false);

        if (usernameInputField != null)
        {
            usernameInputField.text = PlayerPrefs.GetString("LastUsername", "Player");
        }

        UpdateHighScoreDisplay();
    }

    private void PlayGame()
    {
        if (usernameInputField != null && !string.IsNullOrEmpty(usernameInputField.text))
        {
            PlayerPrefs.SetString("LastUsername", usernameInputField.text);
            PlayerPrefs.Save();
            Debug.Log($"Oyun ba�lat�l�yor, Kullan�c�: {usernameInputField.text}");
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.LogWarning("Kullan�c� ad� bo�, l�tfen bir isim girin!");
        }
    }

    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    private void OpenHighScore()
    {
        highScorePanel.SetActive(true);
        UpdateHighScoreDisplay();
    }

    private void CloseHighScore()
    {
        highScorePanel.SetActive(false);
    }

    private void SetMusicVolume(float volume)
    {
        if (musicManager != null)
        {
            musicManager.SetVolume(volume);
        }
    }

    private void QuitGame()
    {
        Debug.Log("Oyun kapat�l�yor...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void UpdateHighScoreDisplay()
    {
        if (highScoreText == null) return;

        int highScoreCount = PlayerPrefs.GetInt("HighScoreCount", 0);
        HighScoreEntry[] highScores = new HighScoreEntry[highScoreCount];

        for (int i = 0; i < highScoreCount; i++)
        {
            highScores[i] = new HighScoreEntry
            {
                username = PlayerPrefs.GetString($"HighScore_{i}_Name", "Unknown"),
                score = PlayerPrefs.GetInt($"HighScore_{i}_Score", 0)
            };
        }

        System.Array.Sort(highScores, (a, b) => b.score.CompareTo(a.score));

        string displayText = "Y�KSEK SKORLAR\n\n";
        for (int i = 0; i < highScores.Length; i++)
        {
            displayText += $"{i + 1}. {highScores[i].username}: {highScores[i].score}\n";
        }
        highScoreText.text = displayText;
    }

    public static void SaveHighScore(string username, int score)
    {
        int highScoreCount = PlayerPrefs.GetInt("HighScoreCount", 0);
        HighScoreEntry[] highScores = new HighScoreEntry[highScoreCount + 1];

        for (int i = 0; i < highScoreCount; i++)
        {
            highScores[i] = new HighScoreEntry
            {
                username = PlayerPrefs.GetString($"HighScore_{i}_Name", "Unknown"),
                score = PlayerPrefs.GetInt($"HighScore_{i}_Score", 0)
            };
        }

        highScores[highScoreCount] = new HighScoreEntry
        {
            username = username,
            score = score
        };

        System.Array.Sort(highScores, (a, b) => b.score.CompareTo(a.score));

        int newCount = Mathf.Min(highScores.Length, MaxHighScores);
        PlayerPrefs.SetInt("HighScoreCount", newCount);

        for (int i = 0; i < newCount; i++)
        {
            PlayerPrefs.SetString($"HighScore_{i}_Name", highScores[i].username);
            PlayerPrefs.SetInt($"HighScore_{i}_Score", highScores[i].score);
        }

        PlayerPrefs.Save();
        Debug.Log($"Skor kaydedildi: {username} - {score}");
    }
}