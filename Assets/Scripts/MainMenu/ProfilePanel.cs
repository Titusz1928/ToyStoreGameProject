using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProfilePanel : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown regionDropdown;
    public TMP_InputField usernameInput;
    public TextMeshProUGUI warningText;

    public GameObject secretSection;
    private const string showSecretCode = "SHOWME6H9i2";
    private const string hideSecretCode = "HIDEME6H9i2";

    private const string PlayerPrefsRegionKey = "SelectedRegionCode";
    private const string PlayerPrefsUsernameKey = "Username";

    // Codes (stable keys, not shown in UI)
    private readonly string[] regionCodes =
    {
        "R_hungary",
        "R_europe",
        "R_asia",
        "R_africa",
        "R_namerica",
        "R_samerica",
        "R_australia"
    };

    private void OnEnable()
    {
        PopulateDropdown();

        if (regionDropdown != null)
        {
            regionDropdown.onValueChanged.AddListener(OnRegionChanged);

            // Restore saved region code
            string savedCode = PlayerPrefs.GetString(PlayerPrefsRegionKey, regionCodes[0]);
            int index = System.Array.IndexOf(regionCodes, savedCode);
            if (index >= 0)
            {
                regionDropdown.value = index;
                regionDropdown.RefreshShownValue();
            }
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
    }

    private void PopulateDropdown()
    {
        if (regionDropdown == null)
        {
            Debug.LogError("Region dropdown is not assigned in ProfilePanel!");
            return;
        }

        regionDropdown.ClearOptions();

        // Convert codes to localized labels
        List<string> options = new List<string>();
        foreach (string code in regionCodes)
        {
            options.Add(LocalizationManager.Instance.GetLocalizedValue(code));
        }

        regionDropdown.AddOptions(options);
    }

    private void OnRegionChanged(int index)
    {
        if (index < 0 || index >= regionCodes.Length) return;

        string selectedCode = regionCodes[index];
        string selectedLabel = regionDropdown.options[index].text;

        Debug.Log($"Region changed to: {selectedLabel} (code: {selectedCode})");

        PlayerPrefs.SetString(PlayerPrefsRegionKey, selectedCode);
        PlayerPrefs.Save();
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
            Debug.Log("Username too long!");
            if (warningText != null)
            {
                warningText.text = LocalizationManager.Instance.GetLocalizedValue("username_too_long");
                StartCoroutine(ClearWarningAfterDelay(2f));
            }

            // Clear the input field
            if (usernameInput != null)
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


    private IEnumerator ClearWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (warningText != null)
            warningText.text = "";
    }
}
