using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    public string key; // e.g. "title"

    private TextMeshProUGUI textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        UpdateText();
        LocalizationManager.OnLanguageChanged += UpdateText; // subscribe
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText; // unsubscribe
    }

    private void UpdateText()
    {
        if (textComponent != null && LocalizationManager.Instance != null)
        {
            textComponent.text = LocalizationManager.Instance.GetLocalizedValue(key);
        }
    }
}
