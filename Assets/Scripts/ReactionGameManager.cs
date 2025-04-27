using Oculus.Interaction;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ReactionGameManager : MonoBehaviour
{
    public PokeInteractable[] buttons; 
    public PokeInteractable startButton;
    public PokeInteractable backButton;
    public TextMeshProUGUI scoreText;  
    public TextMeshProUGUI timerText;   
    public TextMeshProUGUI gameOverText;
    public int score = 0;             

    private float timer = 30f;
    private PokeInteractable activeButton;
    private bool isGameActive = false;   
    private bool isTimerRunning = false;


    // public AudioSource buzzer;
    // public AudioSource scorer;

    void Start()
    {
        // Validate setup
        if (startButton == null)
        {
            Debug.LogError("Start button not assigned!");
            return;
        }

        
        if (startButton.TryGetComponent<IInteractableView>(out var startInteractableView))
        {
            startInteractableView.WhenStateChanged += OnStartButtonPressed;
        }
        if (backButton.TryGetComponent<IInteractableView>(out var backView))
        {
            backView.WhenStateChanged += OnBackButtonPressed;
        }
        // Deactivate buttons initially (all buttons should be inactive to start with)
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false); // Set all buttons inactive at the start
        }

        UpdateScoreText();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            // Decrease the timer by the time elapsed since the last frame
            timer -= Time.deltaTime;

            // Stop the timer if it reaches 0
            if (timer <= 0f)
            {
                timer = 0f; // Clamp the timer to 0
                isTimerRunning = false; // Stop the timer
                GameOver(); // Call your game-over logic
            }

            UpdateTimerDisplay(timer); // Update the UI
        }
    }

    void UpdateTimerDisplay(float currentTime)
    {
        // Format the timer to display minutes and seconds
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}"; // Example: "00:30"
    }

    void OnStartButtonPressed(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select && !isGameActive)
        {
            isGameActive = true; // Start the game
            startButton.gameObject.SetActive(false); // Disable the start button
            //backButton.gameObject.SetActive(false);
            gameOverText.text = " ";
            timer = 30f;
            isTimerRunning = true;

            SetRandomActiveButton(); // Activate the first game button
        }
    }

    private void OnBackButtonPressed(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select)
        {
            SceneManager.LoadScene(0); // or use the build-index: LoadScene(0)
        }
    }

    void SetRandomActiveButton()
    {
        if (!isGameActive) return;

        // Deactivate the current active button
        if (activeButton != null)
        {
            activeButton.gameObject.SetActive(false); // Disable the current button
            // Unsubscribe from button interaction
            activeButton.GetComponent<IInteractableView>().WhenStateChanged -= OnGameButtonPressed;
        }

        // Pick a random button
        int randomIndex = Random.Range(0, buttons.Length);
        activeButton = buttons[randomIndex];

        // Activate the new button
        activeButton.gameObject.SetActive(true);

        // Subscribe to button press
        if (activeButton.TryGetComponent<IInteractableView>(out var interactableView))
        {
            interactableView.WhenStateChanged += OnGameButtonPressed;
        }
    }

    void OnGameButtonPressed(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select && isGameActive)
        {
            // Increment the score
            score++;
            // scorer.PlayOneShot(scorer.clip);
            UpdateScoreText(); 

            activeButton.gameObject.SetActive(false); // Hide the button

            // Set the next random active button
            SetRandomActiveButton();
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void ResetGame()
    {
        startButton.gameObject.SetActive(true);
        score = 0;
        scoreText.text = "Score: ";
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);
        }
    }
    void GameOver()
    {
        isGameActive = false;
        // buzzer.PlayOneShot(buzzer.clip);
        ScoreManager.SaveScore(score);
        //startButton.gameObject.SetActive(true);
        gameOverText.text = $"GAME OVER\nScore: {score}";
        //gameOverText.gameObject.SetActive(true);

        //backButton.gameObject.SetActive(true);
        ResetGame();
        //Debug.Log("Game Over! Timer reached zero.");
        //gameOverText.text = "GAME OVER\n Score: " + score;
        //score = 0;
        //scoreText.text = "Score: ";
        //foreach (var button in buttons)
        //{
        //    button.gameObject.SetActive(false); 
        //}
       
    }
}
