using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction;
using TMPro;
using System.Text;
using System.Linq;

public class MenuFlowController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainOptions;
    public GameObject difficultyOptions;
    public GameObject scoreBack;
    public GameObject clearScores;
    public GameObject helpPanel;
    // buttons
    public GameObject scorePanel;
    public TextMeshProUGUI scoresText;

    // store the selection
    private int chosenDifficulty = 0;

    void Start()
    {
        // ensure correct panels
        BackButton();
    }

    // Called by “Play” toggle
    public void ShowDifficultyMenu()
    {
        mainOptions.SetActive(false);
        difficultyOptions.SetActive(true);
        scorePanel.SetActive(false);
        scoreBack.SetActive(false);
        clearScores.SetActive(false);
        helpPanel.SetActive(false);
    }

    // Called by Easy/Medium/Hard toggles
    public void PickDifficulty(int diffIndex)
    {
        chosenDifficulty = diffIndex;
        // immediately launch—or you can show a “Start” toggle instead
        LaunchGame();
    }

    public void ShowHighScores()
    {
        mainOptions.SetActive(false);
        difficultyOptions.SetActive(false);
        scoreBack.SetActive(true);
        scorePanel.SetActive(true);
        clearScores.SetActive(true);
        helpPanel.SetActive(false);

        var highs = ScoreManager.LoadHighScores();   // static helper
        var sb = new StringBuilder();
        if (highs == null) { scoresText.text = " "; }
        foreach (var s in highs)
            sb.AppendLine($"• {s}");
        scoresText.text = sb.ToString();
    }

    public void HideHighScores()
    {
        scorePanel.SetActive(false);
        mainOptions.SetActive(true);
        scoreBack.SetActive(false);
        clearScores.SetActive(false);
        helpPanel.SetActive(false);
    }

    public void BackButton()
    {
        mainOptions.SetActive(true);
        difficultyOptions.SetActive(false);
        scorePanel.SetActive(false);
        scoreBack.SetActive(false);
        clearScores.SetActive(false);
        helpPanel.SetActive(false);
    }

    public void ClearHighScores()
    {
        ScoreManager.ClearAll();
        ShowHighScores();
    }

    public void viewControls()
    {
        mainOptions.SetActive(false);
        difficultyOptions.SetActive(false);
        scorePanel.SetActive(false);
        scoreBack.SetActive(true);
        clearScores.SetActive(false);
        helpPanel.SetActive(true);
    }
    void LaunchGame()
    {
        int sceneIndex = chosenDifficulty + 1;
        SceneManager.LoadScene(sceneIndex);
    }
}