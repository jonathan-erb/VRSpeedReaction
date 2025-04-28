using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MediumReactionGameManager : MonoBehaviour
{
    [Header("Button References")]
    public PokeInteractable[] buttons;
    public PokeInteractable startButton;
    public PokeInteractable backButton;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI gameOverText;

    [Header("Medium Difficulty Settings")]
    public float roundTime = 20f;
    public int maxSimultaneousButtons = 2;
    public float buttonLifetime = 1.5f;

    private float timer;
    private bool isGameActive;
    private bool isTimerRunning;
    private int score;

    private List<PokeInteractable> activeButtons = new List<PokeInteractable>();
    // Keep track of each button's callback so we can unsubscribe
    private Dictionary<PokeInteractable, System.Action<InteractableStateChangeArgs>> callbacks
        = new Dictionary<PokeInteractable, System.Action<InteractableStateChangeArgs>>();

    void Start()
    {
        // 1) Hide all game buttons immediately
        foreach (var b in buttons)
            b.gameObject.SetActive(false);

        // 2) Hide the Game Over text and Back button
        // gameOverText.gameObject.SetActive(false);
        backButton.gameObject.SetActive(true);

        // 3) Show only the Start button
        startButton.gameObject.SetActive(true);

        // 4) Initialize UI values
        timer = roundTime;
        UpdateScoreText();
        UpdateTimerDisplay(timer);

        // 5) Wire up Start button to begin the round
        if (startButton.TryGetComponent<IInteractableView>(out var sv))
        {
            sv.WhenStateChanged += OnStartButtonPressed;
        }

        // 6) Wire up Back button to return home
        if (backButton.TryGetComponent<IInteractableView>(out var bv))
        {
            bv.WhenStateChanged += OnBackButtonPressed;
        }
    }

    void Update()
    {
        if (!isTimerRunning) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            isTimerRunning = false;
            EndRound();
        }
        UpdateTimerDisplay(timer);
    }

    private void BeginRound()
    {
        isGameActive = true;
        isTimerRunning = true;
        timer = roundTime;
        score = 0;
        UpdateScoreText();

        startButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        gameOverText.text = " ";

        // Spawn initial batch
        for (int i = 0; i < maxSimultaneousButtons; i++)
            SpawnButton();
    }

    private void OnStartButtonPressed(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select && !isGameActive)
            BeginRound();
    }

    private void OnBackButtonPressed(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Select)
            SceneManager.LoadScene(0);
    }

    private void SpawnButton()
    {
        // Choose a button that isn't active
        var choices = new List<PokeInteractable>(buttons);
        choices.RemoveAll(b => activeButtons.Contains(b));
        if (choices.Count == 0) return;

        var pick = choices[Random.Range(0, choices.Count)];
        activeButtons.Add(pick);
        pick.gameObject.SetActive(true);

        // Create and store a callback for this button
        System.Action<InteractableStateChangeArgs> cb = null;
        cb = args =>
        {
            if (args.NewState == InteractableState.Select && isGameActive)
                DespawnButton(pick, scored: true);
        };

        if (pick.TryGetComponent<IInteractableView>(out var iv))
        {
            iv.WhenStateChanged += cb;
            callbacks[pick] = cb;
        }

        // Start its auto‐despawn timer
        StartCoroutine(DespawnAfterDelay(pick, buttonLifetime));
    }

    private IEnumerator DespawnAfterDelay(PokeInteractable btn, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isGameActive && activeButtons.Contains(btn))
            DespawnButton(btn, scored: false);
    }

    private void DespawnButton(PokeInteractable btn, bool scored)
    {
        // Unsubscribe this button's callback
        if (btn.TryGetComponent<IInteractableView>(out var iv)
            && callbacks.TryGetValue(btn, out var cb))
        {
            iv.WhenStateChanged -= cb;
            callbacks.Remove(btn);
        }

        // Hide & remove from active list
        btn.gameObject.SetActive(false);
        activeButtons.Remove(btn);

        if (scored)
        {
            score++;
            UpdateScoreText();
        }

        // Spawn a new one if the round is still on
        if (isGameActive)
            SpawnButton();
    }

    private void EndRound()
    {
        isGameActive = false;
        isTimerRunning = false;

        // Clear any leftovers

        // Show Game Over UI
        gameOverText.text = $"GAME OVER\nScore: {score}";
        //gameOverText.gameObject.SetActive(true);
        ScoreManager.SaveScore(score);
        startButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);

        foreach (var b in new List<PokeInteractable>(activeButtons))
            DespawnButton(b, scored: false);
    }

    private void UpdateScoreText() =>
        scoreText.text = $"Score: {score}";

    private void UpdateTimerDisplay(float t)
    {
        int m = Mathf.FloorToInt(t / 60);
        int s = Mathf.FloorToInt(t % 60);
        timerText.text = $"{m:00}:{s:00}";
    }
}