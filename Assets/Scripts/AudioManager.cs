using UnityEngine;

// This script manages the music and sound effects of the game.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("Sound Effects")]
    public AudioClip buttonClickSound;
    public AudioClip activateSound;
    public AudioClip doorOpenSound;
    public AudioClip doorLockedSound;
    public AudioClip levelCompleteSound;
    public AudioClip gameOverSound;

    private void Awake()
    {
        // Keep only one AudioManager in the game.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep the AudioManager when changing scenes.
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null)
        {
            return;
        }

        // Avoid restarting the same music again.
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = 0.25f;
        musicSource.Play();
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void PlayActivate()
    {
        PlaySFX(activateSound);
    }

    public void PlayDoorOpen()
    {
        PlaySFX(doorOpenSound);
    }

    public void PlayDoorLocked()
    {
        PlaySFX(doorLockedSound);
    }

    public void PlayLevelComplete()
    {
        PlaySFX(levelCompleteSound);
    }

    public void PlayGameOver()
    {
        PlaySFX(gameOverSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.volume = 0.6f;
        sfxSource.PlayOneShot(clip);
    }
}