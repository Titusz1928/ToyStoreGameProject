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
        bannedWords = new HashSet<string>();
        isLoaded = false;

        try
        {
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
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load profanity list: {e.Message}");
        }
    }

    /// <summary>
    /// Checks if the given input contains profanity.
    /// Returns false if the list couldn't load (you might show a warning instead).
    /// </summary>
    public static bool ContainsProfanity(string input)
    {
        if (!isLoaded)
        {
            Debug.LogWarning("Profanity list not loaded. Cannot check input!");
            return false;
        }

        if (string.IsNullOrWhiteSpace(input))
            return false;

        string lower = input.ToLower();
        foreach (string word in bannedWords)
        {
            // stricter check: matches even if the word is inside another word
            if (lower.Contains(word))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns whether the profanity list is ready to use.
    /// </summary>
    public static bool IsLoaded() => isLoaded;
}
