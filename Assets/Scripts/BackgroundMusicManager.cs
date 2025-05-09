using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    private static BackgroundMusicManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton kontrol�: Sadece bir tane BackgroundMusicManager olsun
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Obje sahneler aras�nda yok olmas�n
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource bile�enini al
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("BackgroundMusicManager: AudioSource bile�eni eksik!");
        }
        else if (audioSource.clip == null)
        {
            Debug.LogError("BackgroundMusicManager: AudioClip atanmam��!");
        }
    }

    // Ses seviyesini ayarla
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume); // 0-1 aras�nda s�n�rla
        }
    }
}
