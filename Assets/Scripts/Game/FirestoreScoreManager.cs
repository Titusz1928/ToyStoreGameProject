using Firebase.Firestore;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.UI;

public class FirestoreScoreManager : MonoBehaviour
{
    public static FirestoreScoreManager Instance;

    public GameObject globalRecordsButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Fetch the max number of leaderboard rows from Firestore.
    /// Returns defaultValue if not found or error.
    /// </summary>
    private async Task<int> GetMaxRowsFromFirestore(int defaultValue = 10)
    {
        try
        {
            var settingsDoc = await FirebaseInit.Db
                .Collection("gamesettings")
                .Document("leaderboards")
                .GetSnapshotAsync();

            if (settingsDoc.Exists && settingsDoc.ContainsField("tablerows"))
            {
                return settingsDoc.GetValue<int>("tablerows");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not fetch tablerows, using default {defaultValue}. Error: {e}");
        }

        return defaultValue;
    }


    public async Task SaveScoreGeneric(int score, string gameCode, bool isWeekly = false)
    {
        if (FirebaseInit.Db == null || FirebaseInit.User == null)
        {
            Debug.LogError("Firebase not initialized yet!");
            return;
        }

        try
        {
            int maxRows = await GetMaxRowsFromFirestore(10);
            string collectionName = isWeekly ? "W" + gameCode : gameCode;
            var collection = FirebaseInit.Db.Collection(collectionName);

            // 1. Delete old weekly scores (> 1 week) only if it's a weekly leaderboard
            if (isWeekly)
            {
                var oneWeekAgo = System.DateTime.UtcNow.AddDays(-7);
                var oldScoresSnapshot = await collection
                    .WhereLessThan("timestamp", Timestamp.FromDateTime(oneWeekAgo))
                    .GetSnapshotAsync();

                foreach (var oldDoc in oldScoresSnapshot.Documents)
                {
                    await oldDoc.Reference.DeleteAsync();
                    Debug.Log($"[{collectionName}] Deleted old score {oldDoc.Id}");
                }
            }

            // 2. Check if user already has a score
            var userQuery = await collection
                .WhereEqualTo("userId", FirebaseInit.User.UserId)
                .GetSnapshotAsync();

            DocumentSnapshot userDoc = null;
            int oldScore = int.MinValue;

            if (userQuery.Count > 0)
            {
                userDoc = userQuery.Documents.FirstOrDefault();
                oldScore = userDoc.GetValue<int>("score");
            }

            // 3. If user already has a score and new score is not higher → return
            if (userDoc != null && score <= oldScore)
            {
                Debug.Log($"[{collectionName}] Existing score {oldScore} is higher than or equal to new score {score}. Not updating.");
                return;
            }

            // 4. Count scores after cleanup
            var allScoresSnapshot = await collection.GetSnapshotAsync();
            int scoreCount = allScoresSnapshot.Count;

            // 5. Find lowest score if table is full
            DocumentSnapshot lowestDoc = null;
            int lowestScore = int.MinValue;

            if (scoreCount > 0)
            {
                var lowestSnapshot = await collection.OrderBy("score").Limit(1).GetSnapshotAsync();
                lowestDoc = lowestSnapshot.Documents.FirstOrDefault();
                if (lowestDoc != null)
                    lowestScore = lowestDoc.GetValue<int>("score");
            }

            // 6. If table is full, delete the lowest (unless it's the user's old score that we'll replace)
            if (scoreCount >= maxRows)
            {
                if (lowestDoc != null && (userDoc == null || lowestDoc.Id != userDoc.Id))
                {
                    await lowestDoc.Reference.DeleteAsync();
                    Debug.Log($"[{collectionName}] Deleted lowest score {lowestScore} from {lowestDoc.Id}");
                }

                // Delete old user score if it exists and we're replacing it
                if (userDoc != null)
                {
                    await userDoc.Reference.DeleteAsync();
                    Debug.Log($"[{collectionName}] Replaced old score {oldScore} for user {FirebaseInit.User.UserId}");
                }
            }
            else if (userDoc != null)
            {
                // If table not full, just delete old user score to replace it
                await userDoc.Reference.DeleteAsync();
                Debug.Log($"[{collectionName}] Replaced old score {oldScore} for user {FirebaseInit.User.UserId}");
            }

            // 7. Add new score
            var scoreData = new
            {
                userId = FirebaseInit.User.UserId,
                username = PlayerPrefs.GetString("Username", "Guest"),
                region = PlayerPrefs.GetString("SelectedRegionCode", "R_earth"),
                score = score,
                timestamp = FieldValue.ServerTimestamp
            };

            await collection.AddAsync(scoreData);

            Debug.Log($"[{collectionName}] New score {score} saved for user {FirebaseInit.User.UserId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving score in {gameCode} (isWeekly={isWeekly}): " + e);
        }
    }





}