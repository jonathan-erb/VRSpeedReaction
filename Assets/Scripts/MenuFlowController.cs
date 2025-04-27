using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction; // for PokeInteractableUI if you need it
using TMPro;
using System.Text;
using System.Linq;

public class MenuFlowController : MonoBehaviour
{
    public GameObject mainOptions;
    public GameObject difficultyOptions;
    public GameObject scoreBack;
    // buttons
    public GameObject scorePanel;
    public TextMeshProUGUI scoresText;

    // store the selection
    private int chosenDifficulty = 0;

    void Start()
    {
        // ensure correct panels
        mainOptions.SetActive(true);
        difficultyOptions.SetActive(false);
        scorePanel.SetActive(false);
        scoreBack.SetActive(false);
    }

    // Called by “Play” toggle
    public void ShowDifficultyMenu()
    {
        mainOptions.SetActive(false);
        difficultyOptions.SetActive(true);
        scorePanel.SetActive(false);
        scoreBack.SetActive(false);
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

        var highs = ScoreManager.LoadHighScores();   // static helper
        var sb = new StringBuilder();
        foreach (var s in highs)
            sb.AppendLine($"• {s}");
        scoresText.text = sb.ToString();
    }

    public void HideHighScores()
    {
        scorePanel.SetActive(false);
        mainOptions.SetActive(true);
        scoreBack.SetActive(false);
    }

    public void BackButton()
    {
        mainOptions.SetActive(true);
        difficultyOptions.SetActive(false);
        scorePanel.SetActive(false);
    }

    void LaunchGame()
    {
        int sceneIndex = chosenDifficulty + 1;
        SceneManager.LoadScene(sceneIndex);
    }
}