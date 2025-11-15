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
    [SerializeField] private Image[] progressCards; // 10 card images (2 per game completion)
    [SerializeField] private Sprite grayCardSprite; // Gray progress card
    [SerializeField] private Sprite redCardSprite; // Red progress card
    [SerializeField] private TextMeshProUGUI progressText; // Shows "20%", "40%", etc.

    private List<Sprite> spritePairs;
    private Card firstSelected;
    private Card secondSelected;
    private bool isChecking = false;
    
    private int totalPairs;
    private int matchedPairs = 0;
    private int gamesCompleted = 0; // Track completed games
    private int totalGamesNeeded = 5; // 5 games = 100%

    private void Start()
    {
        InitializeProgress();
        StartNewGame();
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

    private void StartNewGame()
    {
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
        if (isChecking || card.isSelected) return;

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
            // Match found
            firstSelected.SetMatched();
            secondSelected.SetMatched();
            
            matchedPairs++;
            
            // Check if current game is complete
            if (matchedPairs >= totalPairs)
            {
                yield return new WaitForSeconds(1f);
                OnGameComplete();
            }
        }
        else
        {
            // No match
            firstSelected.Hide();
            secondSelected.Hide();
        }

        firstSelected = null;
        secondSelected = null;
        isChecking = false;
    }

    private void OnGameComplete()
    {
        // ✅ Only increment if not at max
        if (gamesCompleted < totalGamesNeeded)
        {
            gamesCompleted++;
            UpdateProgress();
        }
        
        if (gamesCompleted >= totalGamesNeeded)
        {
            Debug.Log("🎉 ALL GAMES COMPLETE! 100% Progress!");
            // Keep progress at 100%, but allow replay
        }
        
        // Start next game automatically (player can keep playing)
        StartNewGame();
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