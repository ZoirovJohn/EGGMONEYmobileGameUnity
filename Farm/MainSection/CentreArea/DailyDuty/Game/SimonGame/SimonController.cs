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

    [Header("Button Sprites")]
    public Sprite redOffSprite;
    public Sprite redOnSprite;
    public Sprite greenOffSprite;
    public Sprite greenOnSprite;
    public Sprite blueOffSprite;
    public Sprite blueOnSprite;
    public Sprite yellowOffSprite;
    public Sprite yellowOnSprite;

    [Header("Progress Bar")]
    [SerializeField] private Image[] progressCards;
    [SerializeField] private Sprite grayCardSprite;
    [SerializeField] private Sprite redCardSprite;
    [SerializeField] private TextMeshProUGUI progressText;

    private List<int> sequence = new List<int>();
    private List<int> playerInput = new List<int>();
    private bool isPlayerTurn = false;
    private bool isPlayingSequence = false;
    private Coroutine[] buttonFlashCoroutines = new Coroutine[4];

    private int gamesCompleted = 0;
    private int totalGamesNeeded = 5;
    private int currentSequenceLength = 4;
    private int[] sequenceLengths = new int[] { 4, 4, 5, 5, 6 };

    private void Start()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);

        redButton.onClick.AddListener(() => OnButtonPressed(0));
        greenButton.onClick.AddListener(() => OnButtonPressed(1));
        blueButton.onClick.AddListener(() => OnButtonPressed(2));
        yellowButton.onClick.AddListener(() => OnButtonPressed(3));

        SetAllButtonsToOffState();
        InitializeProgress();
        StartCoroutine(StartRound());
    }

    private void SetAllButtonsToOffState()
    {
        SetButtonSprite(0, false);
        SetButtonSprite(1, false);
        SetButtonSprite(2, false);
        SetButtonSprite(3, false);
    }

    private void InitializeProgress()
    {
        foreach (Image card in progressCards)
            card.sprite = grayCardSprite;

        UpdateProgressText();
    }

    private IEnumerator StartRound()
    {
        if (isPlayingSequence) yield break;

        isPlayingSequence = true;
        isPlayerTurn = false;
        playerInput.Clear();

        currentSequenceLength = sequenceLengths[Mathf.Min(gamesCompleted, sequenceLengths.Length - 1)];

        if (statusText != null)
            statusText.text = "Level " + (gamesCompleted + 1);

        yield return new WaitForSeconds(0.5f);

        sequence.Clear();
        for (int i = 0; i < currentSequenceLength; i++)
            sequence.Add(Random.Range(0, 4));

        yield return StartCoroutine(PlaySequence());

        isPlayerTurn = true;
        isPlayingSequence = false;

        if (statusText != null)
            statusText.text = "YOUR TURN - " + currentSequenceLength + " colors";
    }

    private IEnumerator PlaySequence()
    {
        List<int> sequenceCopy = new List<int>(sequence);

        foreach (int index in sequenceCopy)
        {
            yield return FlashButton(index);
            yield return new WaitForSeconds(delayBetweenFlashes);
        }
    }

    private IEnumerator FlashButton(int index)
    {
        if (buttonFlashCoroutines[index] != null)
            StopCoroutine(buttonFlashCoroutines[index]);

        // Turn ON
        SetButtonSprite(index, true);
        
        // Play sound through MiniGameAudioManager
        if (MiniGameAudioManager.Instance != null)
        {
            MiniGameAudioManager.Instance.PlayButtonSound((ButtonColor)index);
        }

        yield return new WaitForSeconds(flashTime);

        // Turn OFF
        SetButtonSprite(index, false);
        
        buttonFlashCoroutines[index] = null;
    }

    private void OnButtonPressed(int index)
    {
        if (!isPlayerTurn) return;

        playerInput.Add(index);
        StartCoroutine(HandleButtonPress(index));
    }

    private IEnumerator HandleButtonPress(int index)
    {
        // Turn ON
        SetButtonSprite(index, true);
        
        // Play sound through MiniGameAudioManager
        if (MiniGameAudioManager.Instance != null)
        {
            MiniGameAudioManager.Instance.PlayButtonSound((ButtonColor)index);
        }

        yield return new WaitForSeconds(flashTime);

        // Turn OFF
        SetButtonSprite(index, false);

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
            if (statusText != null)
                statusText.text = "PERFECT!";

            yield return new WaitForSeconds(1.5f);
            OnGameComplete();
        }
        else
        {
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
            if (statusText != null)
                statusText.text = "🎉 100% COMPLETE!";
        }
        else
        {
            StartCoroutine(StartRound());
        }
    }

    private void UpdateProgress()
    {
        int cardsToFill = gamesCompleted * 2;
        for (int i = 0; i < progressCards.Length; i++)
            progressCards[i].sprite = i < cardsToFill ? redCardSprite : grayCardSprite;

        UpdateProgressText();
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            int percent = gamesCompleted * 20;
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

    private void SetButtonSprite(int index, bool isOn)
    {
        Button btn = GetButton(index);
        if (btn == null) return;

        Image img = btn.GetComponent<Image>();
        if (img == null) return;

        Sprite targetSprite = null;

        switch (index)
        {
            case 0: // Red
                targetSprite = isOn ? redOnSprite : redOffSprite;
                break;
            case 1: // Green
                targetSprite = isOn ? greenOnSprite : greenOffSprite;
                break;
            case 2: // Blue
                targetSprite = isOn ? blueOnSprite : blueOffSprite;
                break;
            case 3: // Yellow
                targetSprite = isOn ? yellowOnSprite : yellowOffSprite;
                break;
        }

        if (targetSprite != null)
            img.sprite = targetSprite;
    }
}