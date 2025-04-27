using System.Collections.Generic;
using UnityEngine;

public static class ScoreManager
{
    const string HighScoresKey = "HighScores";
    const int MaxEntries = 10;

    // Load the list of saved scores (desc order)
    public static List<int> LoadHighScores()
    {
        string json = PlayerPrefs.GetString(HighScoresKey, "[]");
        var arr = JsonUtility.FromJson<IntArrayWrapper>(json).scores;
        var list = new List<int>(arr);
        return list;
    }

    // Save a new score, keep top MaxEntries
    public static void SaveScore(int score)
    {
        var scores = LoadHighScores();
        scores.Add(score);
        scores.Sort((a, b) => b.CompareTo(a));
        if (scores.Count > MaxEntries)
            scores.RemoveRange(MaxEntries, scores.Count - MaxEntries);

        var wrapper = new IntArrayWrapper { scores = scores.ToArray() };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(HighScoresKey, json);
        PlayerPrefs.Save();
    }

    // helper for JSON
    [System.Serializable]
    class IntArrayWrapper { public int[] scores; }
}