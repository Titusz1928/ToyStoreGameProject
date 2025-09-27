using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ProfilePanel : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown regionDropdown;
    public TMP_InputField usernameInput;
    public TMP_InputField regionInput;
    public TextMeshProUGUI warningText;
    public Image regionImage;

    public GameObject secretSection;
    private const string showSecretCode = "SHOWME6H9i2";
    private const string hideSecretCode = "HIDEME6H9i2";

    private const string PlayerPrefsRegionKey = "SelectedRegionCode";
    private const string PlayerPrefsUsernameKey = "Username";


    [System.Serializable]
    public class RegionCodeList
    {
        public string[] codes;
    }


    // Codes (stable keys, not shown in UI)
    private string[] regionCodes;

    private List<RegionOption> allRegionOptions = new List<RegionOption>();
    private List<RegionOption> currentRegionOptions = new List<RegionOption>();

    private class RegionOption
    {
        public string Code;
        public string Label;

        public RegionOption(string code, string label)
        {
            Code = code;
            Label = label;
        }
    }

    private void LoadRegionCodes()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Other/regioncodes");
        if (jsonFile != null)
        {
            RegionCodeList loaded = JsonUtility.FromJson<RegionCodeList>("{\"codes\":" + jsonFile.text + "}");
            regionCodes = loaded.codes;
        }
        else
        {
            Debug.LogError("Region codes file not found in Resources!");
            regionCodes = new string[] { }; // fallback to empty
        }
    }


    private void OnEnable()
    {
        LoadRegionCodes();
        PopulateDropdown();

        if (regionDropdown != null)
        {
            regionDropdown.onValueChanged.AddListener(OnRegionChanged);

            // Restore saved region code
            string savedCode = PlayerPrefs.GetString(PlayerPrefsRegionKey, regionCodes[0]);
            int index = System.Array.IndexOf(regionCodes, savedCode);
            if (index >= 0)
            {
                regionDropdown.SetValueWithoutNotify(index + 1); // +1 because dummy is at 0
                regionDropdown.RefreshShownValue();

                // Update placeholder to show localized region
                if (regionInput != null)
                {
                    regionInput.text = string.Empty;
                    var placeholderText = regionInput.placeholder as TextMeshProUGUI;
                    if (placeholderText != null)
                        placeholderText.text = LocalizationManager.Instance.GetLocalizedValue(savedCode);
                }
                if (regionImage != null)
                {
                    regionImage.sprite = LoadRegionSprite(savedCode);
                }
            }
        }

        if (regionInput != null)
        {
            regionInput.text = "";
            regionInput.onValueChanged.AddListener(OnRegionInputChanged);
        }

        if (usernameInput != null)
        {
            usernameInput.onEndEdit.AddListener(OnUsernameEntered);

            // Restore saved username
            string savedUsername = PlayerPrefs.GetString(PlayerPrefsUsernameKey, "");
            if (!string.IsNullOrEmpty(savedUsername))
            {
                usernameInput.placeholder.GetComponent<TextMeshProUGUI>().text = savedUsername;
            }
        }

        if (warningText != null)
            warningText.text = "";
    }

    private void OnDisable()
    {
        if (regionDropdown != null)
            regionDropdown.onValueChanged.RemoveListener(OnRegionChanged);

        if (usernameInput != null)
            usernameInput.onEndEdit.RemoveListener(OnUsernameEntered);

        if (regionInput != null)
        {
            regionInput.onValueChanged.RemoveListener(OnRegionInputChanged);
        }
    }

    private void PopulateDropdown()
    {
        if (regionDropdown == null)
        {
            Debug.LogError("Region dropdown is not assigned!");
            return;
        }

        regionDropdown.ClearOptions();
        allRegionOptions.Clear();

        foreach (string code in regionCodes)
        {
            string label = LocalizationManager.Instance.GetLocalizedValue(code);
            allRegionOptions.Add(new RegionOption(code, label));
        }

        // Add dummy option at the top
        List<string> labels = new List<string> { LocalizationManager.Instance.GetLocalizedValue("regionhelpertext") };
        labels.AddRange(allRegionOptions.ConvertAll(opt => opt.Label));

        currentRegionOptions = new List<RegionOption>(allRegionOptions);
        regionDropdown.AddOptions(labels);

        // Select dummy by default
        regionDropdown.SetValueWithoutNotify(0);
        regionDropdown.RefreshShownValue();
    }

    private void OnRegionChanged(int index)
    {
        if (index == 0) return; // dummy selected, do nothing

        int realIndex = index - 1; // adjust for dummy
        if (realIndex < 0 || realIndex >= currentRegionOptions.Count) return;

        RegionOption selected = currentRegionOptions[realIndex];
        Debug.Log($"Region changed to: {selected.Label} (code: {selected.Code})");

        // Save selected code
        PlayerPrefs.SetString(PlayerPrefsRegionKey, selected.Code);
        PlayerPrefs.Save();

        // Temporarily remove listener to prevent reopening dropdown
        if (regionInput != null)
        {
            regionInput.onValueChanged.RemoveListener(OnRegionInputChanged);

            // Clear the input field and update placeholder
            regionInput.text = string.Empty;
            string localizedLabel = LocalizationManager.Instance.GetLocalizedValue(selected.Code);
            var placeholderText = regionInput.placeholder as TextMeshProUGUI;
            if (placeholderText != null)
                placeholderText.text = localizedLabel;

            // Re-add listener
            regionInput.onValueChanged.AddListener(OnRegionInputChanged);
        }

        if (regionImage != null)
        {
            regionImage.sprite = LoadRegionSprite(selected.Code);
        }

        // Hide the dropdown menu / scrollview
        regionDropdown.Hide();
    }

    private void OnRegionInputChanged(string input)
    {
        // Filter options
        currentRegionOptions = string.IsNullOrWhiteSpace(input)
            ? new List<RegionOption>(allRegionOptions)
            : allRegionOptions.FindAll(opt => opt.Label.ToLower().Contains(input.ToLower()));

        regionDropdown.Hide();
        regionDropdown.ClearOptions();

        if (currentRegionOptions.Count > 0)
        {
            // Build labels list including dummy at top
            List<string> labels = new List<string> { LocalizationManager.Instance.GetLocalizedValue("regionhelpertext") };
            labels.AddRange(currentRegionOptions.ConvertAll(opt => opt.Label));

            regionDropdown.AddOptions(labels);

            // Reset selection to dummy so first real item is clickable
            regionDropdown.SetValueWithoutNotify(0);
            regionDropdown.RefreshShownValue();

            // Force layout rebuild
            StartCoroutine(ForceDropdownLayoutRefresh());
        }

        // Refocus input after layout rebuild
        StartCoroutine(RefocusInputNextFrame());


    }

    private IEnumerator ForceDropdownLayoutRefresh()
    {
        yield return null; // Wait one frame

        RectTransform dropdownTemplate = regionDropdown.template;
        if (dropdownTemplate != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownTemplate);

            ScrollRect scrollRect = dropdownTemplate.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
                scrollRect.verticalNormalizedPosition = 1f; // scroll to top
            }
        }

        yield return null; // Wait another frame

        regionDropdown.Show();
    }

    private IEnumerator RefocusInputNextFrame()
    {
        // Wait two frames to ensure dropdown popup is fully created
        yield return null;
        yield return null;

        regionInput.ActivateInputField();
        regionInput.caretPosition = regionInput.text.Length;
        regionInput.selectionAnchorPosition = regionInput.caretPosition;
        regionInput.selectionFocusPosition = regionInput.caretPosition;
    }

    private void OnUsernameEntered(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            // Clear the input field
            if (usernameInput != null)
                usernameInput.text = string.Empty;

            return;
        }

        // Check for secret codes first
        if (input.Equals(showSecretCode, System.StringComparison.OrdinalIgnoreCase))
        {
            if (secretSection != null)
                secretSection.SetActive(true);
            usernameInput.text = string.Empty;
            return;
        }
        else if (input.Equals(hideSecretCode, System.StringComparison.OrdinalIgnoreCase))
        {
            if (secretSection != null)
                secretSection.SetActive(false);
            usernameInput.text = string.Empty;
            return;
        }

        if (input.Length > 12)
        {
            ShowWarning("username_too_long");
            usernameInput.text = string.Empty;
            return;
        }

        if (!ProfanityManager.IsLoaded())
        {
            ShowWarning("username_check_failed");
            return;
        }

        if (ProfanityManager.ContainsProfanity(input))
        {
            ShowWarning("username_inappropriate");
            usernameInput.text = string.Empty;
            return;
        }

        Debug.Log($"Username set to: {input}");

        PlayerPrefs.SetString(PlayerPrefsUsernameKey, input);
        PlayerPrefs.Save();

        if (usernameInput != null)
        {
            // Clear the input field
            usernameInput.text = string.Empty;

            // Update placeholder immediately
            var placeholderText = usernameInput.placeholder as TextMeshProUGUI;
            if (placeholderText != null)
                placeholderText.text = input;
        }
    }

    private void ShowWarning(string localizationKey)
    {
        if (warningText != null)
        {
            warningText.text = LocalizationManager.Instance.GetLocalizedValue(localizationKey);
            StartCoroutine(ClearWarningAfterDelay(2f));
        }
    }


    private IEnumerator ClearWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (warningText != null)
            warningText.text = "";
    }

    private Sprite LoadRegionSprite(string regionCode)
    {
        Sprite sprite = Resources.Load<Sprite>($"Sprites/Regions/{regionCode}");
        if (sprite == null)
            sprite = Resources.Load<Sprite>("Sprites/Regions/R_earth");
        return sprite;
    }
}