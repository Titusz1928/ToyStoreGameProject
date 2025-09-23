using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGameManager : MonoBehaviour
{
    public static NewGameManager Instance;

    [Header("UI References")]
    public Slider heightSlider;
    public TextMeshProUGUI gameAreaText;
    public TextMeshProUGUI cardNumberText;

    [Header("Preview GameAreaImage")]
    public Image gameAreaPreview;         // The UI Image where the preview will show
    public Sprite[] gameAreaSprites;

    [Header("Preview CardNumberImage")]
    public Image cardNumberPreview;         // The UI Image where the preview will show
    public Sprite[] cardNumberSprites;

    // Maps height -> width
    private Dictionary<int, int> heightToWidth = new Dictionary<int, int>
    {
        { 3, 5 },
        { 4, 6 },
        { 5, 7 },
        { 6, 9 },
        { 7, 12 }
    };

    private void Awake()
    {
        Instance = this;
    }


    public void SetGameArea(float heightFloat)
    {
        int height = Mathf.RoundToInt(heightFloat);

        if (!heightToWidth.TryGetValue(height, out int width))
        {
            //Debug.LogWarning($"Unsupported height value: {height}");
            return;
        }

        GameSettings.GridHeight = height;
        GameSettings.GridWidth = width;

        Debug.Log("gamesettings: " + GameSettings.GridHeight + " " + GameSettings.GridWidth+" "+GameSettings.MaxAllowed);

        gameAreaText.text = $" {height} x {width}";

        int index = height - 3;
        if (index >= 0 && index < gameAreaSprites.Length && gameAreaPreview != null)
        {
            gameAreaPreview.sprite = gameAreaSprites[index];
        }
    }

    public void SetCardNumber(float numberOfCards)
    {
        GameSettings.MaxAllowed = (int)numberOfCards;

        int number = Mathf.RoundToInt(numberOfCards);

        cardNumberText.text = $" {numberOfCards}";
        int index = number - 3;
        if (index >= 0 && index < cardNumberSprites.Length && cardNumberPreview != null)
        {
            cardNumberPreview.sprite = cardNumberSprites[index];
        }
    }
}
