using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using MiniJSON;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Tooltip("Folder inside Resources where language json files live (e.g. Resources/Languages)")]
    public string resourcesFolder = "Languages";

    [Tooltip("Language codes, must match filenames inside Resources/Languages (without extension). Order should match your dropdown indices.")]
    public string[] languageCodes = new string[] { "hun", "eng" };

    public string prefsKey = "languageIndex"; // saved dropdown index

    private Dictionary<string, string> localizedText = new Dictionary<string, string>();

    public static event Action OnLanguageChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // load saved language index (default 0)
            int savedIndex = PlayerPrefs.GetInt(prefsKey, 0);
            savedIndex = Mathf.Clamp(savedIndex, 0, languageCodes.Length - 1);
            LoadLanguage(languageCodes[savedIndex]);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Load language file (Resources/Languages/{languageCode}.json).
    /// The JSON is expected to be a flat object: { "title":"...", "newgame":"..." }
    /// </summary>
    public void LoadLanguage(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode))
        {
            Debug.LogError("LoadLanguage called with empty languageCode");
            return;
        }

        TextAsset txt = Resources.Load<TextAsset>($"{resourcesFolder}/{languageCode}");
        if (txt == null)
        {
            Debug.LogError($"Localization file not found: Resources/{resourcesFolder}/{languageCode}.json");
            return;
        }

        localizedText = ParseFlatJsonToDictionary(txt.text);

        OnLanguageChanged?.Invoke();
    }

    /// <summary>
    /// Called by UI dropdown (use the index that matches languageCodes array).
    /// </summary>
    public void SetLanguageIndex(int index)
    {
        if (index < 0 || index >= languageCodes.Length) return;
        PlayerPrefs.SetInt(prefsKey, index);
        PlayerPrefs.Save();
        LoadLanguage(languageCodes[index]);
    }

    public string GetLocalizedValue(string key)
    {
        if (localizedText != null && localizedText.TryGetValue(key, out string value))
            return value;
        return $"[MISSING:{key}]";
    }

    /// <summary>
    /// Very small parser for flat JSON objects of string->string pairs.
    /// It uses regex so it's not a full JSON implementation — fine for simple localization files.
    /// </summary>
    private Dictionary<string, string> ParseFlatJsonToDictionary(string json)
    {
        var dict = new Dictionary<string, string>();
        var raw = Json.Deserialize(json) as Dictionary<string, object>;
        if (raw != null)
        {
            foreach (var kvp in raw)
                dict[kvp.Key] = kvp.Value.ToString();
        }
        return dict;
    }
}
