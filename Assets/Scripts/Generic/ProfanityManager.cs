using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;

public static class ProfanityManager
{
    private static HashSet<string> bannedWords;
    private static bool isLoaded = false;

    /// <summary>
    /// Loads the combined banned word list from Firestore.
    /// </summary>
    public static async Task LoadList()
    {
        if (isLoaded)
            return; // Already loaded

        bannedWords = new HashSet<string>();

        var docRef = FirebaseInit.Db.Collection("profanity").Document("eng"); // single combined list
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            Debug.LogError("Profanity document not found in Firestore!");
            return;
        }

        List<string> words = snapshot.GetValue<List<string>>("words");

        foreach (var word in words)
        {
            string w = word.Trim().ToLower();
            if (!string.IsNullOrEmpty(w))
                bannedWords.Add(w);
        }

        isLoaded = true;
        Debug.Log($"Loaded {bannedWords.Count} banned words");
    }

    /// <summary>
    /// Checks if the given input contains profanity.
    /// </summary>
    public static bool ContainsProfanity(string input)
    {
        if (!isLoaded || bannedWords == null)
        {
            Debug.LogWarning("ProfanityManager used before loading list. Call LoadList() first!");
            return false;
        }

        string lower = input.ToLower();
        foreach (string word in bannedWords)
        {
            // stricter check: matches even if the word is inside another word
            if (lower.Contains(word))
                return true;
        }

        return false;
    }
}
