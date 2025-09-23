using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public Image flagImage;           // The UI Image where the flag will be shown
    public Sprite[] flagSprites;      // Array of flags (order must match dropdown options)

    public string[] languageCodes = { "hun", "eng" }; // internal codes      // dynamically filled

    [SerializeField] private TextMeshProUGUI highscoreText;

    private string PREF_KEY = "language"; // key in PlayerPrefs
    private const string PREF_FIRST_RUN = "firstRun";

    [SerializeField] private GameObject welcomeWindow;

    void Start()
    {
        bool isFirstRun = PlayerPrefs.GetInt(PREF_FIRST_RUN, 1) == 1;

        if (isFirstRun)
        {
            if (welcomeWindow != null)
                welcomeWindow.SetActive(true);
        }
        else
        {
            Debug.Log("not first run");
            if(welcomeWindow!=null)
                welcomeWindow.SetActive(false);
            PopulateDropdown();
            int savedLang = PlayerPrefs.GetInt(PREF_KEY, 0);
            dropdown.value = savedLang;
            ApplyLanguage(savedLang);
            UpdateDropdownLabels();
        }

        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    public void SetFirstRun()
    {
        PlayerPrefs.SetInt(PREF_FIRST_RUN, 1);
        PlayerPrefs.Save();
    }


    public void SelectHungarian()
    {
        ApplyFirstRunLanguage(0);
    }

    public void SelectEnglish()
    {
        ApplyFirstRunLanguage(1);
    }

    private void ApplyFirstRunLanguage(int index)
    {

        // save both language choice and "not first run" flag
        PlayerPrefs.SetInt(PREF_KEY, index);
        PlayerPrefs.SetInt(PREF_FIRST_RUN, 0);
        PlayerPrefs.Save();

        PopulateDropdown();          
        dropdown.value = index;      
        ApplyLanguage(index);
        UpdateDropdownLabels();

        // Hide welcome window, show normal UI
        welcomeWindow.SetActive(false);
    }


    void PopulateDropdown()
    {
        dropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < languageCodes.Length; i++)
        {
            string localizedName = LocalizationManager.Instance.GetLocalizedValue(languageCodes[i]);
            options.Add(new TMP_Dropdown.OptionData(localizedName));
        }

        dropdown.AddOptions(options);
    }

    void UpdateDropdownLabels()
    {
        // Ensure we have the same number of options as language codes
        for (int i = 0; i < languageCodes.Length; i++)
        {
            string localizedName = LocalizationManager.Instance.GetLocalizedValue(languageCodes[i]);
            if (i < dropdown.options.Count)
                dropdown.options[i].text = localizedName;
        }

        // Refresh to apply the changes
        dropdown.RefreshShownValue();
    }

    private void OnLanguageChanged(int index)
    {
        PlayerPrefs.SetInt(PREF_KEY, index);
        PlayerPrefs.Save();

        ApplyLanguage(index);
        UpdateDropdownLabels();
    }

    private void ApplyLanguage(int index)
    {
        // Update flag image
        if (flagImage != null && index >= 0 && index < flagSprites.Length)
            flagImage.sprite = flagSprites[index];

        // Example: 0 = Hungarian, 1 = English
        switch (index)
        {
            case 0:
                Debug.Log("Set language to Hungarian");
                break;
            case 1:
                Debug.Log("Set language to English");
                break;
        }

        LocalizationManager.Instance.SetLanguageIndex(index);

        if (highscoreText != null)
        {
            string gameCode = "Gametype_" + GameSettings.GridHeight.ToString() + "rows" + GameSettings.MaxAllowed.ToString() + "cards";
            highscoreText.text= LocalizationManager.Instance.GetLocalizedValue("highscore") + ": " + PlayerPrefs.GetInt(gameCode, 0).ToString();
        }


        // You’ll call your text update method here
        // e.g. LocalizationManager.Instance.SetLanguage(index);
    }
}
