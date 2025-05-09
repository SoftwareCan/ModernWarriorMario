using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    private static BackgroundMusicManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton kontrolü: Sadece bir tane BackgroundMusicManager olsun
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Obje sahneler arasýnda yok olmasýn
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource bileþenini al
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("BackgroundMusicManager: AudioSource bileþeni eksik!");
        }
        else if (audioSource.clip == null)
        {
            Debug.LogError("BackgroundMusicManager: AudioClip atanmamýþ!");
        }
    }

    // Ses seviyesini ayarla
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume); // 0-1 arasýnda sýnýrla
        }
    }
}
