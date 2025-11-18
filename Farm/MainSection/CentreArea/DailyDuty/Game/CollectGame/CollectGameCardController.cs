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

    private List<CollectGameCard> cards = new List<CollectGameCard>();
    private int nextNumber = 1; // User must click this number next
    private bool isLocked = false;

    private int gamesCompleted = 0;
    private int totalGamesNeeded = 1;

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
            }
        }

        if (gamesCompleted >= totalGamesNeeded)
        {
            Debug.Log("🎉 ALL GAMES COMPLETE (100%)");
            // Keep progress at 100%, but allow replay
        }

        StartNewGame();
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