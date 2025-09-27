using UnityEngine;
using UnityEditor;
using Firebase.Firestore;
using System.Collections.Generic;

public class UploadProfanity : EditorWindow
{
    [MenuItem("Tools/Upload Combined Profanity List")]
    public static async void UploadCombinedList()
    {
        FirebaseFirestore db = FirebaseInit.Db;
        if (db == null)
        {
            Debug.LogError("Firebase not initialized!");
            return;
        }

        // Load English words
        List<string> combinedWords = await LoadWordsFromFile("eng");

        // Load Hungarian words and append
        List<string> hunWords = await LoadWordsFromFile("hun");
        foreach (var word in hunWords)
        {
            if (!combinedWords.Contains(word))
                combinedWords.Add(word);
        }

        // Upload combined list to Firestore under one document, e.g., "eng"
        var data = new Dictionary<string, object>
        {
            { "words", combinedWords }
        };

        await db.Collection("profanity").Document("eng").SetAsync(data);

        Debug.Log($"Uploaded combined list with {combinedWords.Count} words to Firestore");
    }

    private static async System.Threading.Tasks.Task<List<string>> LoadWordsFromFile(string languageCode)
    {
        string path = $"Profanity/{languageCode}_profanity";
        TextAsset textFile = Resources.Load<TextAsset>(path);

        if (textFile == null)
        {
            Debug.LogError($"File not found: {path}.txt");
            return new List<string>();
        }

        string[] lines = textFile.text.Split('\n');
        List<string> words = new List<string>();

        foreach (var line in lines)
        {
            string word = line.Trim().ToLower();
            if (!string.IsNullOrEmpty(word))
                words.Add(word);
        }

        Debug.Log($"Loaded {words.Count} words from {languageCode}");
        return words;
    }
}
