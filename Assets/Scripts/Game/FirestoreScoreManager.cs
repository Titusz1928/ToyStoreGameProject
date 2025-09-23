using Firebase.Firestore;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class FirestoreScoreManager : MonoBehaviour
{
    public static FirestoreScoreManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    //OLD VERSION
    /*public async Task SaveScore(int score, string gameCode)
    {
        if (FirebaseInit.Db == null || FirebaseInit.User == null)
        {
            Debug.LogError("Firebase not initialized yet!");
            return;
        }

        try
        {
            var scoreData = new
            {
                userId = FirebaseInit.User.UserId,
                score = score,
                timestamp = FieldValue.ServerTimestamp
            };

            // Save score in collection named after gameCode
            await FirebaseInit.Db.Collection(gameCode).AddAsync(scoreData);

            Debug.Log($"Score {score} saved for user {FirebaseInit.User.UserId} in {gameCode}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving score: " + e);
        }
    }*/

    public async Task SaveScore(int score, string gameCode)
    {
        if (FirebaseInit.Db == null || FirebaseInit.User == null)
        {
            Debug.LogError("Firebase not initialized yet!");
            return;
        }

        try
        {
            var collection = FirebaseInit.Db.Collection(gameCode);

            // Get total number of scores
            var allScoresSnapshot = await collection.GetSnapshotAsync();
            int scoreCount = allScoresSnapshot.Count;

            // Get the lowest score (if any exist)
            var lowestSnapshot = await collection.OrderBy("score").Limit(1).GetSnapshotAsync();

            int lowestScore = int.MinValue;
            DocumentSnapshot lowestDoc = null;

            if (lowestSnapshot.Count > 0)
            {
                lowestDoc = lowestSnapshot.Documents.FirstOrDefault();
                lowestScore = lowestDoc.GetValue<int>("score");
            }

            // If we already have 10 or more scores and this score is not better than the lowest → do nothing
            if (scoreCount >= 10 && lowestDoc != null && score <= lowestScore)
            {
                Debug.Log($"Score {score} is not higher than the lowest score {lowestScore} in {gameCode}");
                return;
            }

            // Check if this user already has a score
            var userQuery = await collection.WhereEqualTo("userId", FirebaseInit.User.UserId).GetSnapshotAsync();

            DocumentSnapshot userDoc = null;
            if (userQuery.Count > 0)
            {
                userDoc = userQuery.Documents.FirstOrDefault();
                await userDoc.Reference.DeleteAsync();
                Debug.Log($"Deleted old score for {FirebaseInit.User.UserId}");
            }

            // If user didn’t have a score, and collection is full (>= 10), delete the lowest one
            if (userDoc == null && scoreCount >= 10 && lowestDoc != null)
            {
                await lowestDoc.Reference.DeleteAsync();
                Debug.Log($"Deleted lowest score {lowestScore} from {lowestDoc.Id}");
            }

            // Add the new score
            var scoreData = new
            {
                userId = FirebaseInit.User.UserId,
                score = score,
                timestamp = FieldValue.ServerTimestamp
            };

            await collection.AddAsync(scoreData);

            Debug.Log($"New score {score} saved for user {FirebaseInit.User.UserId} in {gameCode}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving score: " + e);
        }
    }
}