using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameMusic;

    private AudioSource musicSource;

    private float currentVolume = 1f;
    private float sfxVolume = 1f;
    private bool isMusicOn = true; // tracks if music is enabled
    private bool isSFXOn = true;
    private bool hasStarted = false;

    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        LoadSettings();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void PlaySoundEffect(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip != null && isSFXOn)
        {
            float finalVolume = Mathf.Clamp01(sfxVolume * volumeMultiplier);
            sfxSource.PlayOneShot(clip, finalVolume);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenuScene":
                if (mainMenuMusic != null)
                {
                    if (!hasStarted)
                    {
                        musicSource.clip = mainMenuMusic;
                        musicSource.volume = currentVolume;
                        musicSource.Play();
                        hasStarted = true;
                    }
                    else
                    {
                        StartCoroutine(FadeToNewTrack(mainMenuMusic));
                    }
                }
                break;

            case "gameScene":
                if (gameMusic != null)
                    StartCoroutine(FadeToNewTrack(gameMusic));
                break;
        }
    }

    private IEnumerator FadeToNewTrack(AudioClip newClip)
    {
        float fadeDuration = 0.5f;
        float startVolume = musicSource.volume;

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;

        // Apply mute state
        musicSource.mute = !isMusicOn;

        musicSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, currentVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = currentVolume;
    }

    public void SetMusicVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = currentVolume;
        }
        SaveSettings();

        //Debug.Log($"Slider Value: {volume}, CurrentVolume: {currentVolume}, MusicSource.volume: {musicSource.volume}, IsMuted: {musicSource.mute}");

    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        // Optional: Apply volume to existing looped SFX (not relevant for OneShots)
        SaveSettings();
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public void ToggleSFX(bool isOn)
    {
        isSFXOn = isOn;
        SaveSettings();

    }

    public bool IsSFXOn()
    {
        return isSFXOn;
    }


    public void ToggleMusic(bool isOn)
    {
        Debug.Log(isOn);
        isMusicOn = isOn;
        if (musicSource != null)
            musicSource.mute = !isOn;

        SaveSettings();
    }

    public float GetCurrentVolume()
    {
        return currentVolume;
    }

    public bool IsMusicOn()
    {
        return isMusicOn;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //SAVING SETTINGS

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", currentVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("IsMusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.SetInt("IsSFXOn", isSFXOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        currentVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        isMusicOn = PlayerPrefs.GetInt("IsMusicOn", 1) == 1;
        isSFXOn = PlayerPrefs.GetInt("IsSFXOn", 1) == 1;

        if (musicSource != null)
        {
            musicSource.volume = currentVolume;
            musicSource.mute = !isMusicOn;
        }
    }

}
