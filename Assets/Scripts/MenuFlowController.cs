using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction; // for PokeInteractableUI if you need it

public class MenuFlowController : MonoBehaviour
{
    public GameObject mainOptions;
    public GameObject difficultyOptions;

    // buttons

    // store the selection
    private int chosenDifficulty = 0;

    void Start()
    {
        // ensure correct panels
        mainOptions.SetActive(true);
        difficultyOptions.SetActive(false);
    }

    // Called by “Play” toggle
    public void ShowDifficultyMenu()
    {
        mainOptions.SetActive(false);
        difficultyOptions.SetActive(true);
    }

    // Called by Easy/Medium/Hard toggles
    public void PickDifficulty(int diffIndex)
    {
        chosenDifficulty = diffIndex;
        // immediately launch—or you can show a “Start” toggle instead
        LaunchGame();
    }

    public void BackButton()
    {
        mainOptions.SetActive(true);
        difficultyOptions.SetActive(false);
    }

    void LaunchGame()
    {
        // map 0/1/2 to your scene names or build indices
        //string sceneName = chosenDifficulty switch
        //{
        //    0 => "Game1",
        //    1 => "Game_Medium",
        //    2 => "Game_Hard",
        //    _ => "Game_Easy"
        //};
        int sceneIndex = chosenDifficulty + 1;
        SceneManager.LoadScene(sceneIndex);
    }
}