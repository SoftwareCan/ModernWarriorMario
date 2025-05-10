using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("BackgroundMusicManager: AudioSource bulunamad�!");
        }
        Debug.Log($"AudioSource: {audioSource}, Clip: {audioSource?.clip}");
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
            Debug.Log($"M�zik ses seviyesi: {volume}");
        }
    }

    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("M�zik durduruldu!");
        }
    }

    public void PlayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log("M�zik �al�yor!");
        }
    }

    public bool IsMusicPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
}