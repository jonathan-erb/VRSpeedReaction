using System;
using System.Collections.Generic;
using UnityEngine;

public static class ScoreManager
{
    private const string HighScoresKey = "HighScores";
    private const int MaxEntries = 10;


    [Serializable]
    private class IntArrayWrapper
    {
        public int[] scores = new int[0];
    }

    public static List<int> LoadHighScores()
    {
        // Create a valid default JSON object if nothing is saved yet
        string defaultJson = JsonUtility.ToJson(new IntArrayWrapper());
        string json = PlayerPrefs.GetString(HighScoresKey, defaultJson);

        // Parse into our wrapper
        IntArrayWrapper wrapper = null;
        try
        {
            wrapper = JsonUtility.FromJson<IntArrayWrapper>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ScoreManager] Failed to parse saved scores JSON: {ex}");
            // fall back to empty
            wrapper = new IntArrayWrapper();
        }

        return new List<int>(wrapper.scores);
    }


    public static void SaveScore(int score)
    {
        if (score <= 0)
        {
            // Don't record zero or negative runs
            return;
        }

        try
        {
            // Load existing
            List<int> scores = LoadHighScores();

            // Add, sort descending, clamp count
            scores.Add(score);
            scores.Sort((a, b) => b.CompareTo(a));
            if (scores.Count > MaxEntries)
                scores.RemoveRange(MaxEntries, scores.Count - MaxEntries);

            // Serialize and save
            var wrapper = new IntArrayWrapper { scores = scores.ToArray() };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(HighScoresKey, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ScoreManager] Failed to save high scores: {ex}");
        }
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(HighScoresKey);
        PlayerPrefs.Save();
    }
}