using Firebase.Firestore;
using System.Collections;
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
    public GameObject imageRowPrefab;

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

        // Store the region text so it can be reassigned when the row is reactivated
        public string RegionText;

        // Reference to the TMP object for convenience
        public TextMeshProUGUI RegionTMP;
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

                if (rank <= 3)
                    row = Instantiate(imageRowPrefab, parent);
                else
                    row = Instantiate(rowPrefab, parent);

                // Add ScoreRow and save weekly flag
                var scoreRow = row.AddComponent<ScoreRow>();
                scoreRow.IsWeekly = isWeekly;

                // Medal image for top 3
                if (rank <= 3)
                {
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
                }

                // Region image & text
                Transform regionContainer = row.transform.GetChild(1);
                Image regionImage = regionContainer.GetComponentInChildren<Image>();
                TextMeshProUGUI regionText = regionContainer.GetComponentInChildren<TextMeshProUGUI>();

                if (regionImage != null)
                {
                    regionImage.sprite = LoadRegionSprite(region);
                    regionImage.gameObject.SetActive(true);

                    // Add a Button component if not already present
                    Button btn = regionImage.GetComponent<Button>();
                    if (btn == null)
                        btn = regionImage.gameObject.AddComponent<Button>();

                    // Capture the ScoreRow for the lambda
                    ScoreRow capturedRow = scoreRow;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        // Show the text briefly
                        StartCoroutine(ShowRegionTextTemporarily(capturedRow));
                    });
                }

                if(regionText != null)
                {
                    string localizedRegion = LocalizationManager.Instance.GetLocalizedValue(region);

                    // Set TMP text to empty initially
                    regionText.text = LocalizationManager.Instance.GetLocalizedValue("empty");

                    // Add / set LocalizedText component so the key can update dynamically
                    LocalizedText localizedText = regionText.GetComponent<LocalizedText>();
                    if (localizedText == null)
                        localizedText = regionText.gameObject.AddComponent<LocalizedText>();

                    localizedText.key = "empty"; // ensures TMP sees a valid localization key

                    regionText.raycastTarget = false; // clicks pass through

                    // Save TMP reference and actual localized text for later
                    scoreRow.RegionTMP = regionText;
                    scoreRow.RegionText = localizedRegion;
                }

                // Fill other texts
                TextMeshProUGUI[] rowTexts = row.GetComponentsInChildren<TextMeshProUGUI>();
                if (rank <= 3 && rowTexts.Length >= 4)
                {
                    rowTexts[1].text = username;
                    rowTexts[2].text = score.ToString();
                    rowTexts[3].text = timeStr;
                }
                else if (rowTexts.Length >= 5)
                {
                    rowTexts[0].text = rank.ToString();
                    rowTexts[2].text = username;
                    rowTexts[3].text = score.ToString();
                    rowTexts[4].text = timeStr;
                }

                // Highlight current player
                if (userId == FirebaseInit.User.UserId)
                {
                    var bgImage = row.GetComponent<Image>();
                    if (bgImage != null)
                        bgImage.color = new Color(0.7f, 1f, 0.7f, 1f);
                }

                // Add to appropriate list
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

                // Reassign the region text
/*                var scoreRow = row.GetComponent<ScoreRow>();
                if (scoreRow != null && scoreRow.RegionTMP != null)
                {
                    scoreRow.RegionTMP.text = scoreRow.RegionText;
                }*/
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

                // Reassign the region text
/*                var scoreRow = row.GetComponent<ScoreRow>();
                if (scoreRow != null && scoreRow.RegionTMP != null)
                {
                    scoreRow.RegionTMP.text = scoreRow.RegionText;
                }*/
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

    private Sprite LoadRegionSprite(string regionCode)
    {
        Sprite sprite = Resources.Load<Sprite>($"Sprites/Regions/{regionCode}");
        if (sprite == null)
            sprite = Resources.Load<Sprite>("Sprites/notexture");
        return sprite;
    }

    private IEnumerator ShowRegionTextTemporarily(ScoreRow row)
    {
        if (row.RegionTMP == null) yield break;

        // Get the image component (assumes it is a sibling or parent of the TMP)
        Image regionImage = row.RegionTMP.transform.parent.GetComponentInChildren<Image>();
        if (regionImage == null) yield break;

        // Show text
        row.RegionTMP.text = row.RegionText;

        // Fade out the image
        float duration = 0.3f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float alpha = 1 - t / duration;
            regionImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        regionImage.color = new Color(1f, 1f, 1f, 0f);

        // Wait while text is visible
        yield return new WaitForSeconds(2f);

        // Hide text
        row.RegionTMP.text = LocalizationManager.Instance.GetLocalizedValue("empty");

        // Fade image back in
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float alpha = t / duration;
            regionImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        regionImage.color = new Color(1f, 1f, 1f, 1f);
    }




}
