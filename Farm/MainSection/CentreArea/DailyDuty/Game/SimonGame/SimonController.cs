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

    [Header("Sound Effects")]
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip completeSound;
    private AudioSource audioSource;

    [Header("Feeding Manager")]
    [SerializeField] private FeedingManager feedingManager;

    [Header("Close Button")]
    [SerializeField] private Button closeBtn;

    [Header("Full Summary")]
    [SerializeField] private FullSummaryManager fullSummaryManager;

    private List<int> sequence = new List<int>();
    private List<int> playerInput = new List<int>();
    private bool isPlayerTurn = false;
    private bool isPlayingSequence = false;

    private int gamesCompleted = 0;
    private int totalGamesNeeded = 1;
    private int currentSequenceLength = 4;
    private int[] sequenceLengths = new int[] { 4, 4, 5, 5, 6 };
    
    private bool isWaitingForClose = false;

    private void Start()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

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
        // Don't start new round if waiting for close button
        if (isWaitingForClose) yield break;

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
            Button btn = GetButton(index);
            if (btn != null)
            {
                SetButtonSprite(index, true);
                btn.onClick.Invoke();
                
                yield return new WaitForSeconds(flashTime);
                
                SetButtonSprite(index, false);
            }
            
            yield return new WaitForSeconds(delayBetweenFlashes);
        }
    }

    private void OnButtonPressed(int index)
    {
        if (isPlayingSequence) return;
        if (!isPlayerTurn) return;
        if (isWaitingForClose) return;

        playerInput.Add(index);
        StartCoroutine(HandleButtonPress(index));
    }

    private IEnumerator HandleButtonPress(int index)
    {
        SetButtonSprite(index, true);
        yield return new WaitForSeconds(flashTime);
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
            PlaySound(matchSound);

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
        if (gamesCompleted < totalGamesNeeded)
        {
            gamesCompleted++;
            UpdateProgress();
            
            if (gamesCompleted >= totalGamesNeeded)
            {
                PlaySound(completeSound);
                
                if (statusText != null)
                    statusText.text = "🎉 100% COMPLETE!";
                
                // 🍗 Call feeding API when 100% complete
                if (feedingManager != null)
                {
                    isWaitingForClose = true;
                    
                    feedingManager.FeedAll(
                        onSuccess: (response) => 
                        {
                            Debug.Log("Feeding successful after game completion");
                            
                            // Refresh FullSummary to update UI immediately
                            if (fullSummaryManager != null)
                            {
                                fullSummaryManager.GetFullSummary(
                                    onSuccess: (summaryResponse) => 
                                    {
                                        Debug.Log("✅ Full Summary refreshed after feeding");
                                        // Click the close button after summary refresh
                                        if (closeBtn != null)
                                        {
                                            closeBtn.onClick.Invoke();
                                        }
                                    },
                                    onError: (summaryError) => 
                                    {
                                        Debug.LogError($"Failed to refresh summary: {summaryError}");
                                        // Click close button anyway
                                        if (closeBtn != null)
                                        {
                                            closeBtn.onClick.Invoke();
                                        }
                                    }
                                );
                            }
                            else
                            {
                                // No summary manager, just click close button
                                if (closeBtn != null)
                                {
                                    closeBtn.onClick.Invoke();
                                }
                            }
                        },
                        onError: (error) => 
                        {
                            Debug.LogError("Feeding failed: " + error);
                            // Click the close button even on error
                            if (closeBtn != null)
                            {
                                closeBtn.onClick.Invoke();
                            }
                        }
                    );
                }
            }
        }
        else
        {
            if (statusText != null)
                statusText.text = "🎉 100% COMPLETE!";
        }
        
        // Only start new round if not waiting for close button
        if (!isWaitingForClose)
        {
            StartCoroutine(StartRound());
        }
    }

    private void UpdateProgress()
    {
        int cardsToFill = gamesCompleted * 10;
        for (int i = 0; i < progressCards.Length; i++)
            progressCards[i].sprite = i < cardsToFill ? redCardSprite : grayCardSprite;

        UpdateProgressText();
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            int percent = gamesCompleted * 100;
            progressText.text = percent + "%";
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
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
            case 0:
                targetSprite = isOn ? redOnSprite : redOffSprite;
                break;
            case 1:
                targetSprite = isOn ? greenOnSprite : greenOffSprite;
                break;
            case 2:
                targetSprite = isOn ? blueOnSprite : blueOffSprite;
                break;
            case 3:
                targetSprite = isOn ? yellowOnSprite : yellowOffSprite;
                break;
        }

        if (targetSprite != null)
            img.sprite = targetSprite;
    }
}