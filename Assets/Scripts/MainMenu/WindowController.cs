using System.Collections;
using System.Collections.Generic;
using Tabmenu.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;


public class WindowController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject newGamePanel;
    [SerializeField] private GameObject profilePanel;

    public Slider musicVolumeSlider;
    public Toggle musicVolumeToggle;

    public Slider SFXVolumeSlider;
    public Toggle SFXVolumeToggle;

    public Slider gameAreaSlider;
    public Slider cardNumberSlider;

    private void Start()
    {
        musicVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        SFXVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        if (AudioManager.Instance != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.GetCurrentVolume();
            musicVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);

            musicVolumeToggle.onValueChanged.AddListener(isChecked => AudioManager.Instance.ToggleMusic(!isChecked));
            musicVolumeToggle.isOn = !AudioManager.Instance.IsMusicOn();

            SFXVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
            SFXVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);

            SFXVolumeToggle.onValueChanged.AddListener(isChecked => AudioManager.Instance.ToggleSFX(!isChecked));
            SFXVolumeToggle.isOn = !AudioManager.Instance.IsSFXOn();

        }

        gameAreaSlider.onValueChanged.AddListener(NewGameManager.Instance.SetGameArea);
        cardNumberSlider.onValueChanged.AddListener(NewGameManager.Instance.SetCardNumber);

        _ = InitializeProfanityCheck();

    }

    private async Task InitializeProfanityCheck()
    {
        await ProfanityManager.LoadList();

        // Check current username in PlayerPrefs
        string currentUsername = PlayerPrefs.GetString("Username", "Guest");
        if (ProfanityManager.ContainsProfanity(currentUsername))
        {
            Debug.Log($"❌ Existing username '{currentUsername}' contains profanity. Resetting to default.");
            PlayerPrefs.SetString("Username", "Username"); // or "Guest"
            PlayerPrefs.Save();
        }
    }

    public void OpenSettingsPanel()
    {
        infoPanel.SetActive(false);
        newGamePanel.SetActive(false);
        highScorePanel.SetActive(false);
        settingsPanel.SetActive(true);

    }

    public void OpenHighScoresPanel()
    {
        infoPanel.SetActive(false);
        settingsPanel.SetActive(false);
        newGamePanel.SetActive(false);
        highScorePanel.SetActive(true);

    }

    public void OpenInfoPanel()
    {
        settingsPanel.SetActive(false);
        newGamePanel.SetActive(false);
        highScorePanel.SetActive(false);
        infoPanel.SetActive(true);
    }

    public void OpenProfilePanel()
    {
        profilePanel.SetActive(true);
    }

    public void OpenNewGamePanel()
    {
        infoPanel.SetActive(false);
        highScorePanel.SetActive(false);
        settingsPanel.SetActive(false);
        newGamePanel.SetActive(true);

        GameSettings.GridWidth = Mathf.RoundToInt(7);
        GameSettings.GridHeight = Mathf.RoundToInt(5);

        // Force tab index 0 to be active
        var tabMenu = newGamePanel.GetComponentInChildren<TabMenu>();
        if (tabMenu != null)
        {
            tabMenu.JumpToPage(0);
        }
    }

    public void OpenMainMenuPanel()
    {
        infoPanel.SetActive(false);
        highScorePanel.SetActive(false);
        settingsPanel.SetActive(false);
        newGamePanel.SetActive(false);
        profilePanel.SetActive(false);
    }

    public void OpenContactWebsite()
    {
        string url = "https://www.linkedin.com/in/titusz-boros-970a9725a/"; // replace with your link
        Application.OpenURL(url);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();

        #if UNITY_EDITOR
                // For unity editor
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

}
