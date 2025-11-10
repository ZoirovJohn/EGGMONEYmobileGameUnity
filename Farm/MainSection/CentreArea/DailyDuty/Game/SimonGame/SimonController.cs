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

    private List<int> sequence = new List<int>();
    private List<int> playerInput = new List<int>();
    private bool isPlayerTurn = false;

    private void Start()
    {
        redButton.onClick.AddListener(() => OnButtonPressed(0));
        greenButton.onClick.AddListener(() => OnButtonPressed(1));
        blueButton.onClick.AddListener(() => OnButtonPressed(2));
        yellowButton.onClick.AddListener(() => OnButtonPressed(3));

        StartCoroutine(StartRound());
    }

    private IEnumerator StartRound()
    {
        isPlayerTurn = false;
        playerInput.Clear();
        sequence.Clear();

        if (statusText != null)
            statusText.text = "WATCH";

        yield return new WaitForSeconds(0.5f); // Small delay before showing

        // Generate 4 random colors
        for (int i = 0; i < 4; i++)
            sequence.Add(Random.Range(0, 4));

        // Show sequence
        yield return StartCoroutine(PlaySequence());

        // Player's turn
        isPlayerTurn = true;
        if (statusText != null)
            statusText.text = "YOUR TURN";
    }

    private IEnumerator PlaySequence()
    {
        foreach (int index in sequence)
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

        Color originalColor = img.color;
        
        // Brighten
        Color brightColor = originalColor;
        brightColor.a = 1f;
        img.color = brightColor;

        yield return new WaitForSeconds(flashTime);

        // Always restore to original color
        img.color = originalColor;
    }

    private void OnButtonPressed(int index)
    {
        if (!isPlayerTurn) return;

        StartCoroutine(FlashButton(index));
        playerInput.Add(index);

        // Check only after player enters all 4 inputs
        if (playerInput.Count == sequence.Count)
        {
            isPlayerTurn = false; // Lock input while checking
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
            StartCoroutine(StartRound());
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