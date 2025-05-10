using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("BackgroundMusicManager: AudioSource bulunamadý!");
        }
        Debug.Log($"AudioSource: {audioSource}, Clip: {audioSource?.clip}");
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
            Debug.Log($"Müzik ses seviyesi: {volume}");
        }
    }

    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("Müzik durduruldu!");
        }
    }

    public void PlayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log("Müzik çalýyor!");
        }
    }

    public bool IsMusicPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
}