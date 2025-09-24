using System.Collections.Generic;
using UnityEngine;

public static class ProfanityManager
{
    private static HashSet<string> bannedWords;
    private static string currentLanguage;

    // Load the correct list based on language code ("eng", "hun", etc.)
    public static void LoadList(string languageCode = "eng")
    {
        if (bannedWords != null && currentLanguage == languageCode)
            return; // Already loaded for this language

        currentLanguage = languageCode;
        bannedWords = new HashSet<string>();

        string path = $"Profanity/{languageCode}_profanity";
        TextAsset textFile = Resources.Load<TextAsset>(path);

        if (textFile == null)
        {
            Debug.LogError($"Profanity file not found at Resources/{path}.txt");
            return;
        }

        string[] lines = textFile.text.Split('\n');
        foreach (var line in lines)
        {
            string word = line.Trim().ToLower();
            if (!string.IsNullOrEmpty(word))
                bannedWords.Add(word);
        }

        Debug.Log($"Loaded {bannedWords.Count} banned words for language: {languageCode}");
    }

    public static bool ContainsProfanity(string input, string languageCode = "eng")
    {
        LoadList(languageCode);

        if (bannedWords == null || bannedWords.Count == 0)
            return false;

        string lower = input.ToLower();
        foreach (string word in bannedWords)
        {
            if (lower.Contains(word))
                return true;
        }

        return false;
    }
}
