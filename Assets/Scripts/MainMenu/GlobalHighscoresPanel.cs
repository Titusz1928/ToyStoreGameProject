using Firebase.Firestore;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GlobalHighScoresPanel : MonoBehaviour
{
    public static GlobalHighScoresPanel Instance;

    [Header("UI References")]
    public GameObject contentRoot;          // panel root
    public Transform contentParent;         // ScrollView Content (where rows go)
    public GameObject headerPrefab;         // prefab for column names
    public GameObject rowPrefab;            // prefab for score rows

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (contentRoot == null)
            contentRoot = this.gameObject; // fallback
    }

    public async Task ShowScores(string gameCode)
    {
        if (FirebaseInit.Db == null)
        {
            Debug.LogError("Firestore not initialized yet!");
            return;
        }

        // Enable panel
        contentRoot.SetActive(true);

        // Clear old content
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Add header row
        GameObject headerRow = Instantiate(headerPrefab, contentParent);
        TextMeshProUGUI[] headerTexts = headerRow.GetComponentsInChildren<TextMeshProUGUI>();
        if (headerTexts.Length >= 5)
        {
            headerTexts[0].text = LocalizationManager.Instance.GetLocalizedValue("Pos");
            headerTexts[1].text = LocalizationManager.Instance.GetLocalizedValue("Region");
            headerTexts[2].text = LocalizationManager.Instance.GetLocalizedValue("User");
            headerTexts[3].text = LocalizationManager.Instance.GetLocalizedValue("Score");
            headerTexts[4].text = LocalizationManager.Instance.GetLocalizedValue("Time");
        }

        try
        {
            var query = FirebaseInit.Db.Collection(gameCode)
                .OrderByDescending("score")
                .Limit(10);

            var snapshot = await query.GetSnapshotAsync();

            Debug.Log($"=== Global High Scores for {gameCode} ===");

            int rank = 1;
            foreach (var doc in snapshot.Documents)
            {
                string userId = doc.GetValue<string>("userId");
                int score = doc.GetValue<int>("score");
                Timestamp ts = doc.GetValue<Timestamp>("timestamp");

                string username = doc.GetValue<string>("username");
                string region = doc.GetValue<string>("region");
                string timeStr = ts.ToDateTime().ToLocalTime().ToString("yyyy-MM-dd");

                // Debug log
                Debug.Log($"{rank}. {username} ({region}) - {score} - {timeStr}");

                // Spawn row
                GameObject row = Instantiate(rowPrefab, contentParent);
                TextMeshProUGUI[] rowTexts = row.GetComponentsInChildren<TextMeshProUGUI>();
                if (rowTexts.Length >= 5)
                {
                    rowTexts[0].text = rank.ToString();
                    rowTexts[1].text = LocalizationManager.Instance.GetLocalizedValue(region);
                    rowTexts[2].text = username;
                    rowTexts[3].text = score.ToString();
                    rowTexts[4].text = timeStr;
                }

                if (userId == FirebaseInit.User.UserId)
                {
                    var bgImage = row.GetComponent<Image>();
                    if (bgImage != null)
                        bgImage.color = new Color(0.7f, 1f, 0.7f, 1f); // light green RGBA
                }

                rank++;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading scores for {gameCode}: {e}");
        }
    }

    public void HidePanel()
    {
        if (contentRoot != null)
            contentRoot.SetActive(false);
    }
}
