using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardController : MonoBehaviour
{
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform gridTransform;
    [SerializeField] private Sprite[] sprites;
    
    [Header("Progress Bar")]
    [SerializeField] private Image[] progressCards;
    [SerializeField] private Sprite grayCardSprite;
    [SerializeField] private Sprite redCardSprite;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip cardFlipSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip completeSound;
    private AudioSource audioSource;

    [Header("Cleanup Manager")]
    [SerializeField] private CleanupManager cleanupManager;

    [Header("Close Button")]
    [SerializeField] private Button closeBtn;

    [Header("Full Summary")]
    [SerializeField] private FullSummaryManager fullSummaryManager;

    private List<Sprite> spritePairs;
    private Card firstSelected;
    private Card secondSelected;
    private bool isChecking = false;
    
    private int totalPairs;
    private int matchedPairs = 0;
    private int gamesCompleted = 0;
    private int totalGamesNeeded = 1;
    
    private bool isWaitingForClose = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        InitializeProgress();
        StartNewGame();
    }

    private void InitializeProgress()
    {
        foreach (Image card in progressCards)
        {
            card.sprite = grayCardSprite;
        }
        
        UpdateProgressText();
    }

    private void StartNewGame()
    {
        // Don't start new game if waiting for close button
        if (isWaitingForClose) return;

        // Clear existing cards
        foreach (Transform child in gridTransform)
        {
            Destroy(child.gameObject);
        }

        matchedPairs = 0;
        firstSelected = null;
        secondSelected = null;
        isChecking = false;

        PrepareSprites();
        CreateCards();
    }

    private void PrepareSprites()
    {
        spritePairs = new List<Sprite>();

        // Make pairs
        foreach (Sprite sp in sprites)
        {
            spritePairs.Add(sp);
            spritePairs.Add(sp);
        }

        ShuffleSprites(spritePairs);
        totalPairs = spritePairs.Count / 2;
    }

    private void CreateCards()
    {
        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card newCard = Instantiate(cardPrefab, gridTransform);
            newCard.SetIconSprite(spritePairs[i]);
            newCard.controller = this;
            newCard.Hide();
        }
    }

    public void SetSelected(Card card)
    {
        if (isChecking || card.isSelected || isWaitingForClose) return;

        PlaySound(cardFlipSound);

        card.Show();

        if (firstSelected == null)
        {
            firstSelected = card;
        }
        else if (secondSelected == null)
        {
            secondSelected = card;
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        isChecking = true;
        yield return new WaitForSeconds(0.5f);

        if (firstSelected.GetIconSprite() == secondSelected.GetIconSprite())
        {
            PlaySound(matchSound);

            firstSelected.SetMatched();
            secondSelected.SetMatched();
            
            matchedPairs++;
            
            if (matchedPairs >= totalPairs)
            {
                yield return new WaitForSeconds(1f);
                OnGameComplete();
            }
        }
        else
        {
            firstSelected.Hide();
            secondSelected.Hide();
        }

        firstSelected = null;
        secondSelected = null;
        isChecking = false;
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
                
                // 🧹 Call cleanup API when 100% complete
                if (cleanupManager != null)
                {
                    isWaitingForClose = true;
                    
                    cleanupManager.CleanAll(
                        onSuccess: (response) => 
                        {
                            Debug.Log("Cleanup successful after game completion");
                            // Click the close button after cleanup
                            if (closeBtn != null)
                            {
                                closeBtn.onClick.Invoke();
                            }
                        },
                        onError: (error) => 
                        {
                            Debug.LogError("Cleanup failed: " + error);
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
        
        if (gamesCompleted >= totalGamesNeeded && !isWaitingForClose)
        {
            Debug.Log("🎉 ALL GAMES COMPLETE! 100% Progress!");
        }
        
        // Only start new game if not waiting for close button
        if (!isWaitingForClose)
        {
            StartNewGame();
        }
    }

    private void UpdateProgress()
    {
        int cardsToFill = gamesCompleted * 10;
        
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

    private void ShuffleSprites(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Sprite temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}