using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HardReactionGameManager : MonoBehaviour
{
    [Header("Button References")]
    public PokeInteractable[] buttons;
    public PokeInteractable startButton;
    public PokeInteractable backButton;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI gameOverText;

    [Header("Hard Mode Settings")]
    public float roundTime = 15f;
    public int maxSimultaneousButtons = 3;
    public float buttonLifetime = 1f;
    [Range(0f, 1f)]
    public float targetProbability = 0.5f;

    // ──────────────── internal state ────────────────
    private float timer;
    private bool isGameActive;
    private bool isTimerRunning;
    private int score;

    // track which buttons are currently live
    private List<PokeInteractable> activeButtons = new List<PokeInteractable>();
    // track each button’s color role (true=green target, false=red decoy)
    private Dictionary<PokeInteractable, bool> isTarget = new Dictionary<PokeInteractable, bool>();
    // store each button’s callback so we can unsubscribe cleanly
    private Dictionary<PokeInteractable, System.Action<InteractableStateChangeArgs>> callbacks = new Dictionary<PokeInteractable, System.Action<InteractableStateChangeArgs>>();

    void Start()
    {
        // 1) Hide everything until Start is pressed
        foreach (var b in buttons)
            b.gameObject.SetActive(false);

        startButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);

        // 2) Init UI
        timer = roundTime;

        //BeginRound();
        // 3) Wire Start button
        if (startButton.TryGetComponent<IInteractableView>(out var sv))
            sv.WhenStateChanged += OnStartButtonPressed;

        // 4) Wire Back button
        if (backButton.TryGetComponent<IInteractableView>(out var bv))
            bv.WhenStateChanged += OnBackButtonPressed;

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

    private void BeginRound()
    {
        Debug.Log($"   roundTime={roundTime}, maxButtons={maxSimultaneousButtons}, poolSize={buttons.Length}");
        isGameActive = true;
        isTimerRunning = true;
        timer = roundTime;
        score = 0;
        UpdateScoreUI();

        startButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);

        // spawn initial buttons
        for (int i = 0; i < maxSimultaneousButtons; i++)
            SpawnButton();
    }

    void Update()
    {
        if (!isTimerRunning) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            isTimerRunning = false;
            GameOver();
        }
        timerText.text = $"{(int)timer / 60:00}:{(int)timer % 60:00}";
    }

    private void SpawnButton()
    {
        // 1) Debug log so you can see it firing
        Debug.Log($"[SpawnButton] active={activeButtons.Count}, pool={buttons?.Length}");

        // 2) Pick an unused button from your array
        var choices = new List<PokeInteractable>(buttons);
        choices.RemoveAll(b => activeButtons.Contains(b));
        if (choices.Count == 0) return;

        var pick = choices[Random.Range(0, choices.Count)];
        bool isGreen = Random.value < targetProbability;
        isTarget[pick] = isGreen;

        // 3) Find the color‐tweener on the child, guard against null
        var vis = pick.GetComponentInChildren<InteractableColorVisual>();
        if (vis == null)
        {
            Debug.LogWarning($"SpawnButton: {pick.name} has no InteractableColorVisual in children");
            return;
        }

        // 4) Inject Normal & Hover states so it’s immediately green or red
        var cs = new InteractableColorVisual.ColorState
        {
            Color = isGreen ? Color.green : Color.red
        };
        vis.InjectOptionalNormalColorState(cs);
        vis.InjectOptionalHoverColorState(cs);
        vis.enabled = false;  // toggle to force an OnEnable→UpdateVisual()
        vis.enabled = true;

        // 5) Activate & track it
        activeButtons.Add(pick);
        pick.gameObject.SetActive(true);

        // 6) Subscribe to poke‐events on the child view
        var iv = pick.GetComponentInChildren<IInteractableView>();
        if (iv != null)
        {
            System.Action<InteractableStateChangeArgs> cb = null;
            cb = (args) =>
            {
                if (args.NewState == InteractableState.Select && isGameActive)
                    DespawnButton(pick, pressed: true);
            };
            iv.WhenStateChanged += cb;
            callbacks[pick] = cb;
        }
        else
        {
            Debug.LogWarning($"SpawnButton: {pick.name} has no IInteractableView in children");
        }

        // 7) Schedule auto‐despawn if untouched
        StartCoroutine(AutoDespawn(pick, buttonLifetime));
    }

    private IEnumerator AutoDespawn(PokeInteractable btn, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isGameActive && activeButtons.Contains(btn))
            DespawnButton(btn, pressed: false);
    }

    private void DespawnButton(PokeInteractable btn, bool pressed)
    {
        // unsubscribe exactly the callback we added
        if (callbacks.TryGetValue(btn, out var cb))
        {
            btn.GetComponent<IInteractableView>().WhenStateChanged -= cb;
            callbacks.Remove(btn);
        }

        btn.gameObject.SetActive(false);
        activeButtons.Remove(btn);

        // scoring: +1 for green, -1 for red when pressed
        if (pressed)
        {
            score += isTarget[btn] ? +1 : -1;
            UpdateScoreUI();
        }

        // spawn replacement if still running
        if (isGameActive)
            SpawnButton();
    }

    private void GameOver()
    {
        isGameActive = false;

        // clear any buttons still active
        foreach (var b in new List<PokeInteractable>(activeButtons))
            DespawnButton(b, pressed: false);

        // show final score
        ScoreManager.SaveScore(score);
        gameOverText.text = $"GAME OVER\nScore: {score}";
        gameOverText.gameObject.SetActive(true);

        // reveal Back button
        backButton.gameObject.SetActive(true);

        // persist
    }

    private void UpdateScoreUI()
    {
        scoreText.text = $"Score: {score}";
        timerText.text = $"{(int)timer / 60:00}:{(int)timer % 60:00}";
    }
}