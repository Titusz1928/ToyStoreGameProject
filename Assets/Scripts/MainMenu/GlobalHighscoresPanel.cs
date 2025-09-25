using Firebase.Firestore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class GlobalHighScoresPanel : MonoBehaviour
{
    public static GlobalHighScoresPanel Instance;

    [Header("UI References")]
    public GameObject contentRoot;          // panel root
    public Transform contentParent;         // ScrollView Content (where rows go)
    public GameObject headerPrefab;         // prefab for column names
    public GameObject rowPrefab;            // prefab for score rows
    public GameObject imageRowPrefan;

    public GameObject loadingText;

    [Header("Special Rank Sprites")]
    public Sprite firstPlaceSprite;
    public Sprite secondPlaceSprite;
    public Sprite thirdPlaceSprite;

    [Header("Background")]
    public Image backgroundImage;

    private readonly List<GameObject> overallRows = new();
    private readonly List<GameObject> weeklyRows = new();

    public class ScoreRow : MonoBehaviour
    {
        public bool IsWeekly;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (contentRoot == null)
            contentRoot = this.gameObject; // fallback
    }

    public void OnShowScoresButtonClicked()
    {
        if (GlobalHighScoresPanel.Instance == null)
            GlobalHighScoresPanel.Instance = FindObjectOfType<GlobalHighScoresPanel>();

        if (GlobalHighScoresPanel.Instance != null)
            _ = GlobalHighScoresPanel.Instance.ShowScores(null);
        else
            Debug.LogError("GlobalHighScoresPanel not found in the scene!");
    }


    private async Task FillUpLeaderboard(bool isWeekly, string collectionName)
    {
        Transform parent = contentParent; // single ScrollRect content

        // Wait for end of frame so LayoutGroups/ScrollRects can update safely
        await Task.Yield();

        try
        {
            var query = FirebaseInit.Db.Collection(collectionName)
                .OrderByDescending("score")
                .Limit(10);

            var snapshot = await query.GetSnapshotAsync();

            // Hide loadingText initially
            if (loadingText != null)
                loadingText.SetActive(false);

            if (snapshot.Count == 0)
            {
                if (loadingText != null)
                {
                    loadingText.SetActive(true);
                    loadingText.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedValue("no_any_rows");
                }
                return;
            }

            int rank = 1;
            foreach (var doc in snapshot.Documents)
            {
                string userId = doc.GetValue<string>("userId");
                int score = doc.GetValue<int>("score");
                Timestamp ts = doc.GetValue<Timestamp>("timestamp");
                string username = doc.GetValue<string>("username");
                string region = doc.GetValue<string>("region");
                string timeStr = ts.ToDateTime().ToLocalTime().ToString("yyyy-MM-dd");

                GameObject row;

                // Choose prefab for top 3
                if (rank <= 3)
                {
                    row = Instantiate(imageRowPrefan, parent);

                    // Medal image
                    Image medalImage = row.transform.GetChild(0).GetComponentInChildren<Image>();
                    if (medalImage != null)
                    {
                        switch (rank)
                        {
                            case 1: medalImage.sprite = firstPlaceSprite; break;
                            case 2: medalImage.sprite = secondPlaceSprite; break;
                            case 3: medalImage.sprite = thirdPlaceSprite; break;
                        }
                    }

                    // Fill texts
                    TextMeshProUGUI[] rowTexts = row.GetComponentsInChildren<TextMeshProUGUI>();
                    if (rowTexts.Length >= 4)
                    {
                        rowTexts[0].text = LocalizationManager.Instance.GetLocalizedValue(region);
                        rowTexts[1].text = username;
                        rowTexts[2].text = score.ToString();
                        rowTexts[3].text = timeStr;
                    }
                }
                else
                {
                    row = Instantiate(rowPrefab, parent);

                    TextMeshProUGUI[] rowTexts = row.GetComponentsInChildren<TextMeshProUGUI>();
                    if (rowTexts.Length >= 5)
                    {
                        rowTexts[0].text = rank.ToString();
                        rowTexts[1].text = LocalizationManager.Instance.GetLocalizedValue(region);
                        rowTexts[2].text = username;
                        rowTexts[3].text = score.ToString();
                        rowTexts[4].text = timeStr;
                    }
                }

                // Highlight current player
                if (userId == FirebaseInit.User.UserId)
                {
                    var bgImage = row.GetComponent<Image>();
                    if (bgImage != null)
                        bgImage.color = new Color(0.7f, 1f, 0.7f, 1f);
                }

                // Mark row as weekly or overall
                var scoreRow = row.AddComponent<ScoreRow>();
                scoreRow.IsWeekly = isWeekly;

                if (isWeekly)
                    weeklyRows.Add(row);
                else
                    overallRows.Add(row);

                rank++;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading scores for {collectionName}: {e}");
        }
    }





    public async Task ShowScores(string gameCode)
    {
        if (string.IsNullOrWhiteSpace(gameCode))
        {
            gameCode = $"Gametype_{GameSettings.GridHeight}rows{GameSettings.MaxAllowed}cards";
        }

        if (FirebaseInit.Db == null)
        {
            Debug.LogError("Firestore not initialized yet!");
            return;
        }

        // Enable panel
        contentRoot.SetActive(true);

        // Destroy all rows except header
        for (int i = contentParent.childCount - 1; i >= 1; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        // Clear the cached lists
        overallRows.Clear();
        weeklyRows.Clear();

        // Load overall scores
        await FillUpLeaderboard(false, gameCode);

        // Load weekly scores
        await FillUpLeaderboard(true, "W" + gameCode);

        // Show overall by default
        ShowOverallLeaderboard();
    }

    private void checkRows(bool hasRows)
    {
        // Show "no rows" message only if no overall rows exist
        if (loadingText != null)
        {
            loadingText.SetActive(!hasRows);
            if (!hasRows)
                loadingText.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedValue("no_any_rows");
        }
    }


    public void ShowOverallLeaderboard()
    {
        bool hasRows = false;

        foreach (var row in overallRows)
        {
            if (row != null)
            {
                row.SetActive(true);
                hasRows = true;
            }
        }

        foreach (var row in weeklyRows)
            if (row != null)
                row.SetActive(false);

        if (contentParent.childCount > 0)
            contentParent.GetChild(0).gameObject.SetActive(true); // header

        if (backgroundImage != null)
            backgroundImage.color = new Color(0.7f, 1f, 0.7f, 1f);

        checkRows(hasRows);
    }


    public void ShowWeeklyLeaderboard()
    {
        bool hasRows = false;

        foreach (var row in overallRows)
            if (row != null)
                row.SetActive(false);

        foreach (var row in weeklyRows)
        {
            if (row != null)
            {
                row.SetActive(true);
                hasRows = true;
            }
        }

        if (contentParent.childCount > 0)
            contentParent.GetChild(0).gameObject.SetActive(true); // header

        if (backgroundImage != null)
            backgroundImage.color = new Color(0.7f, 0.9f, 1f, 1f);

        checkRows(hasRows);
    }

    public void HidePanel()
    {
        if (contentRoot != null)
            contentRoot.SetActive(false);
    }
}
