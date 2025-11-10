using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimonController : MonoBehaviour
{
    [Header("Buttons")]
    public Button redButton;
    public Button greenButton;
    public Button blueButton;
    public Button yellowButton;

    [Header("UI")]
    public TextMeshProUGUI statusText;

    [Header("Flash Settings")]
    public float flashTime = 0.3f;
    public float delayBetweenFlashes = 0.25f;

    [Header("Progress Bar")]
    [SerializeField] private Image[] progressCards; // 10 card images (2 per game completion)
    [SerializeField] private Sprite grayCardSprite; // Gray progress card
    [SerializeField] private Sprite redCardSprite; // Red progress card
    [SerializeField] private TextMeshProUGUI progressText; // Shows "20%", "40%", etc.

    private List<int> sequence = new List<int>();
    private List<int> playerInput = new List<int>();
    private bool isPlayerTurn = false;
    private bool isPlayingSequence = false;
    private Coroutine[] buttonFlashCoroutines = new Coroutine[4];

    // Progress tracking
    private int gamesCompleted = 0;
    private int totalGamesNeeded = 5; // 5 games = 100%
    private int currentSequenceLength = 4; // Start with 4
    private int[] sequenceLengths = new int[] { 4, 4, 5, 5, 6 }; // Progressive difficulty

    private void Start()
    {
        // Ensure random seed is different each time
        Random.InitState((int)System.DateTime.Now.Ticks);
        
        redButton.onClick.AddListener(() => OnButtonPressed(0));
        greenButton.onClick.AddListener(() => OnButtonPressed(1));
        blueButton.onClick.AddListener(() => OnButtonPressed(2));
        yellowButton.onClick.AddListener(() => OnButtonPressed(3));

        InitializeProgress();
        StartCoroutine(StartRound());
    }

    private void InitializeProgress()
    {
        // Set all progress cards to gray sprite
        foreach (Image card in progressCards)
        {
            card.sprite = grayCardSprite;
        }
        
        UpdateProgressText();
    }

    private IEnumerator StartRound()
    {
        // Prevent starting new round while one is in progress
        if (isPlayingSequence) yield break;
        
        isPlayingSequence = true;
        isPlayerTurn = false;
        playerInput.Clear();
        
        // Set current sequence length based on games completed
        currentSequenceLength = sequenceLengths[Mathf.Min(gamesCompleted, sequenceLengths.Length - 1)];
        
        if (statusText != null)
            statusText.text = "Level " + (gamesCompleted + 1);

        yield return new WaitForSeconds(0.5f);

        // Generate random colors based on current difficulty
        sequence.Clear();
        for (int i = 0; i < currentSequenceLength; i++)
        {
            sequence.Add(Random.Range(0, 4));
        }
        
        // Debug to verify new sequence
        Debug.Log("New sequence (length " + currentSequenceLength + "): " + string.Join(", ", sequence));

        // Show sequence
        yield return StartCoroutine(PlaySequence());

        // Player's turn
        isPlayerTurn = true;
        isPlayingSequence = false;
        
        if (statusText != null)
            statusText.text = "YOUR TURN - " + currentSequenceLength + " colors";
    }

    private IEnumerator PlaySequence()
    {
        // Create a copy to avoid collection modified errors
        List<int> sequenceCopy = new List<int>(sequence);
        
        foreach (int index in sequenceCopy)
        {
            yield return FlashButton(index);
            yield return new WaitForSeconds(delayBetweenFlashes);
        }
    }

    private IEnumerator FlashButton(int index)
    {
        Button btn = GetButton(index);
        if (btn == null) yield break;
        
        Image img = btn.GetComponent<Image>();
        if (img == null) yield break;

        // Stop any existing flash on this button
        if (buttonFlashCoroutines[index] != null)
        {
            StopCoroutine(buttonFlashCoroutines[index]);
        }

        Color originalColor = img.color;
        
        // Brighten
        Color brightColor = originalColor;
        brightColor.a = 1f;
        img.color = brightColor;

        yield return new WaitForSeconds(flashTime);

        // Always restore to original color
        img.color = originalColor;
        
        buttonFlashCoroutines[index] = null; // Clear reference
    }

    private void OnButtonPressed(int index)
    {
        if (!isPlayerTurn) return;

        playerInput.Add(index);
        StartCoroutine(HandleButtonPress(index));
    }

    private IEnumerator HandleButtonPress(int index)
    {
        // Store and start the flash coroutine
        buttonFlashCoroutines[index] = StartCoroutine(FlashButton(index));
        
        // Wait for flash to complete
        yield return buttonFlashCoroutines[index];

        // Check only after player enters all inputs AND flash is done
        if (playerInput.Count == sequence.Count)
        {
            isPlayerTurn = false;
            StartCoroutine(CheckResult());
        }
    }

    private IEnumerator CheckResult()
    {
        yield return new WaitForSeconds(0.5f);

        bool isCorrect = true;
        for (int i = 0; i < sequence.Count; i++)
        {
            if (playerInput[i] != sequence[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            Debug.Log("✅ Correct!");
            if (statusText != null)
                statusText.text = "PERFECT!";
            
            yield return new WaitForSeconds(1.5f);
            
            // Game completed successfully
            OnGameComplete();
        }
        else
        {
            Debug.Log("❌ Wrong!");
            if (statusText != null)
                statusText.text = "TRY AGAIN - WATCH";
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(StartRound());
        }
    }

    private void OnGameComplete()
    {
        gamesCompleted++;
        UpdateProgress();
        
        if (gamesCompleted >= totalGamesNeeded)
        {
            // All 5 games completed - 100% progress
            Debug.Log("🎉 ALL GAMES COMPLETE! 100% Progress!");
            if (statusText != null)
                statusText.text = "🎉 100% COMPLETE!";
            
            // Add your victory logic here (show victory screen, confetti, etc.)
            // Optional: Reset everything
            // gamesCompleted = 0;
            // InitializeProgress();
            // StartCoroutine(StartRound());
        }
        else
        {
            // Start next game automatically with increased difficulty
            StartCoroutine(StartRound());
        }
    }

    private void UpdateProgress()
    {
        // Each game completion = 20% = 2 cards out of 10
        int cardsToFill = gamesCompleted * 2; // 2 cards per game (20%)
        
        // Swap sprites for progress cards
        for (int i = 0; i < progressCards.Length; i++)
        {
            progressCards[i].sprite = i < cardsToFill ? redCardSprite : grayCardSprite;
        }
        
        UpdateProgressText();
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            int percent = gamesCompleted * 20; // 20% per game
            progressText.text = percent + "%";
        }
    }

    private Button GetButton(int index)
    {
        switch (index)
        {
            case 0: return redButton;
            case 1: return greenButton;
            case 2: return blueButton;
            case 3: return yellowButton;
        }
        return null;
    }
}