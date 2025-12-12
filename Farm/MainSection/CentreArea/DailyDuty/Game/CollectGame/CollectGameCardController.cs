using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectGameCardController : MonoBehaviour
{
    [Header("Card Setup")]
    [SerializeField] private CollectGameCard cardPrefab;
    [SerializeField] private Transform gridTransform;

    [Header("Sprites for numbers 1-9")]
    [SerializeField] private Sprite[] numberSprites; // index 0 = 1, index 1 = 2, ...

    [Header("Progress UI")]
    [SerializeField] private Image[] progressCards;
    [SerializeField] private Sprite grayCardSprite;
    [SerializeField] private Sprite redCardSprite;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip cardClickSound;    // Sound when any card is clicked
    [SerializeField] private AudioClip correctSound;      // Sound when correct number is found
    [SerializeField] private AudioClip completeSound;     // Sound when 100% complete
    private AudioSource audioSource;

    [Header("Collecting Manager")]
    [SerializeField] private CollectingManager collectingManager;

    [Header("Close Button")]
    [SerializeField] private Button closeBtn;

    private List<CollectGameCard> cards = new List<CollectGameCard>();
    private int nextNumber = 1; // User must click this number next
    private bool isLocked = false;

    private int gamesCompleted = 0;
    private int totalGamesNeeded = 1;
    
    private bool isWaitingForClose = false;

    private void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        InitializeProgress();
        StartNewGame();
    }

    private void InitializeProgress()
    {
        foreach (var img in progressCards)
            img.sprite = grayCardSprite;

        UpdateProgressText();
    }

    private void StartNewGame()
    {
        // Don't start new game if waiting for close button
        if (isWaitingForClose) return;

        // Clear previous cards
        foreach (Transform child in gridTransform)
            Destroy(child.gameObject);

        cards.Clear();
        nextNumber = 1; // Reset to start from 1
        isLocked = false;

        CreateNumberCards();
    }

    private void CreateNumberCards()
    {
        // Create shuffled numbers 1-9
        List<int> nums = new List<int>();
        for (int i = 1; i <= 9; i++)
            nums.Add(i);

        Shuffle(nums);

        // Create 9 cards with random positions
        for (int i = 0; i < 9; i++)
        {
            CollectGameCard newCard = Instantiate(cardPrefab, gridTransform);

            int assignedNumber = nums[i];
            Sprite numberSprite = numberSprites[assignedNumber - 1];
            newCard.SetIconSprite(numberSprite);

            newCard.Hide(); // ✅ Start hidden (face down)

            cards.Add(newCard);

            AddClickListener(newCard, assignedNumber);
        }
    }

    private void AddClickListener(CollectGameCard card, int number)
    {
        Button btn = card.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => OnCardClicked(card, number));
        }
    }

    private void OnCardClicked(CollectGameCard card, int number)
    {
        if (isLocked) return;
        if (isWaitingForClose) return;

        // ✅ Don't click already revealed cards
        if (card.IsRevealed()) return;

        // 🔊 Play card click sound
        PlaySound(cardClickSound);

        // ✅ Correct: clicked the next number in sequence (1→2→3→...→9)
        if (number == nextNumber)
        {
            // 🔊 Play correct number found sound
            PlaySound(correctSound);

            card.Show(); // ✅ OPEN and KEEP IT OPEN
            nextNumber++;

            // Check if all 9 numbers are opened (1 through 9)
            if (nextNumber > 9)
            {
                StartCoroutine(CompleteGame());
            }
        }
        // ❌ Wrong: clicked out of order
        else
        {
            StartCoroutine(WrongSelection(card));
        }
    }

    private IEnumerator WrongSelection(CollectGameCard card)
    {
        isLocked = true; // ✅ Lock IMMEDIATELY before showing

        // ❌ Show the wrong card briefly
        card.Show();
        yield return new WaitForSeconds(0.5f);

        // Close it again
        card.Hide();
        
        isLocked = false; // ✅ Unlock after hiding
    }

    private IEnumerator CompleteGame()
    {
        isLocked = true;
        yield return new WaitForSeconds(0.8f);

        // ✅ Only increment if not at max
        if (gamesCompleted < totalGamesNeeded)
        {
            gamesCompleted++;
            UpdateProgress();
            
            // 🔊 Play complete sound when reaching 100%
            if (gamesCompleted >= totalGamesNeeded)
            {
                PlaySound(completeSound);
                
                Debug.Log("🎉 100% COMPLETE!");
                
                // 🥚 Call collecting API when 100% complete
                if (collectingManager != null)
                {
                    isWaitingForClose = true;
                    
                    collectingManager.CollectAll(
                        onSuccess: (response) => 
                        {
                            Debug.Log($"✅ Collection successful! Collected: {response.collected} eggs, Basket total: {response.basketEggCount}");
                            // Click the close button after collecting
                            if (closeBtn != null)
                            {
                                closeBtn.onClick.Invoke();
                            }
                        },
                        onError: (error) => 
                        {
                            Debug.LogError("Collection failed: " + error);
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

        // Only start new game if not waiting for close button
        if (!isWaitingForClose)
        {
            StartNewGame();
        }
    }

    private void UpdateProgress()
    {
        int filled = gamesCompleted * 10; // 1 game = 10 cards (100%)

        for (int i = 0; i < progressCards.Length; i++)
        {
            progressCards[i].sprite = (i < filled) ? redCardSprite : grayCardSprite;
        }

        UpdateProgressText();
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = (gamesCompleted * 100) + "%";
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            int tmp = list[i];
            list[i] = list[rand];
            list[rand] = tmp;
        }
    }
}