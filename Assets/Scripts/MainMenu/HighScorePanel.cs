using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class HighScoreTable : MonoBehaviour
{
    [Header("UI References")]
    public GameObject highScoreTitlePrefab;
    public GameObject highScoreRowPrefab;
    //public Button globalRecordsButton;



    public Transform contentParent;       // Assign the ScrollView/Content GameObject
    public Slider rowFilterSlider;        // Slider (values 3–7)
    public TextMeshProUGUI filterLabel;   // Label that shows "X Rows"
    public Image rowPreviewImage;         // Preview image
    public Sprite[] rowPreviewSprites;    // Assign 5 sprites (index 0 = 3 rows, index 4 = 7 rows)

    public GlobalHighScoresPanel globalPanel;

    // Maps height -> width
    private Dictionary<int, int> heightToWidth = new Dictionary<int, int>
    {
        { 3, 5 },
        { 4, 6 },
        { 5, 7 },
        { 6, 9 },
        { 7, 12 }
    };

    private void OnEnable()
    {
        if (rowFilterSlider != null)
            rowFilterSlider.onValueChanged.AddListener(OnFilterChanged);

        OnFilterChanged(rowFilterSlider.value); // initialize UI
    }

    private void OnDisable()
    {
        if (rowFilterSlider != null)
            rowFilterSlider.onValueChanged.RemoveListener(OnFilterChanged);
    }

    private void OnFilterChanged(float value)
    {
        int rows = Mathf.RoundToInt(value);

        if (!heightToWidth.TryGetValue(rows, out int width))
        {
            //Debug.LogWarning($"Unsupported height value: {height}");
            return;
        }

        if (filterLabel != null)
            filterLabel.text = $" {rows} x {width}";


        // Update preview image
        int index = rows - 3; // maps 3 → 0, 7 → 4
        if (rowPreviewImage != null && index >= 0 && index < rowPreviewSprites.Length)
            rowPreviewImage.sprite = rowPreviewSprites[index];

        // Refresh table
        PopulateTable(rows);
    }

    void PopulateTable(int rows)
    {
        // Clear existing rows
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Header row
        GameObject headerRow = Instantiate(highScoreTitlePrefab, contentParent);
        TextMeshProUGUI[] headerTexts = headerRow.GetComponentsInChildren<TextMeshProUGUI>();
        if (headerTexts.Length >= 2)
        {
            headerTexts[0].text = LocalizationManager.Instance.GetLocalizedValue("cardslabel");  // "Kártyák száma" / "Cards"
            headerTexts[1].text = LocalizationManager.Instance.GetLocalizedValue("score");       // "Pontszám" / "Score"
            headerTexts[2].text = LocalizationManager.Instance.GetLocalizedValue("globalhighscorestext");
        }

        // Data rows
        for (int cards = 3; cards <= 8; cards++)
        {
            string gameCode = $"{rows}rows{cards}cards";
            int score = PlayerPrefs.GetInt("Gametype_" + gameCode, 0);

            GameObject dataRow = Instantiate(highScoreRowPrefab, contentParent);
            TextMeshProUGUI[] rowTexts = dataRow.GetComponentsInChildren<TextMeshProUGUI>();
            if (rowTexts.Length >= 2)
            {
                rowTexts[0].text = $"{cards}";          // or $"{cards} Kártya"
                rowTexts[1].text = score.ToString();
            }

            Button rowButton = dataRow.GetComponentInChildren<Button>();
            if (rowButton != null)
            {
                // Remove existing listeners first
                rowButton.onClick.RemoveAllListeners();

                // Capture variables for the closure
                int capturedRows = rows;
                int capturedCards = cards;

                // Assign the click behavior
                rowButton.onClick.AddListener(async () =>
                {
                    string gameCode = $"Gametype_{capturedRows}rows{capturedCards}cards";
                    Debug.Log($"Loading global highscores for {gameCode}");

                    if (globalPanel != null)
                    {
                        globalPanel.gameObject.SetActive(true); // activate panel GameObject
                        await globalPanel.ShowScores(gameCode);
                    }
                    else
                    {
                        Debug.LogError("GlobalHighScoresPanel reference not assigned!");
                    }
                });
            }
        }

        GameObject footerRow = Instantiate(highScoreTitlePrefab, contentParent);
        TextMeshProUGUI[] footerTexts = footerRow.GetComponentsInChildren<TextMeshProUGUI>();
        if (footerTexts.Length >= 2)
        {
            footerTexts[0].text = LocalizationManager.Instance.GetLocalizedValue("empty");  // "Kártyák száma" / "Cards"
            footerTexts[1].text = LocalizationManager.Instance.GetLocalizedValue("empty");       // "Pontszám" / "Score"
            footerTexts[2].text = LocalizationManager.Instance.GetLocalizedValue("empty");
        }
    }

}
