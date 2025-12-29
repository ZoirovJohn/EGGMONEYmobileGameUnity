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
    [SerializeField] private Sprite[] numberSprites;

    [Header("Progress UI")]
    [SerializeField] private Image[] progressCards;
    [SerializeField] private Sprite grayCardSprite;
    [SerializeField] private Sprite redCardSprite;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip cardClickSound;    
    [SerializeField] private AudioClip correctSound;     
    [SerializeField] private AudioClip completeSound;     
    private AudioSource audioSource;

    [Header("Collecting Manager")]
    [SerializeField] private CollectingManager collectingManager;

    [Header("Close Button")]
    [SerializeField] private Button closeBtn;

    [Header("Full Summary")]
    [SerializeField] private FullSummaryManager fullSummaryManager;

    private List<CollectGameCard> cards = new List<CollectGameCard>();
    private int nextNumber = 1; 
    private bool isLocked = false;

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
        foreach (var img in progressCards)
            img.sprite = grayCardSprite;

        UpdateProgressText();
    }

    private void StartNewGame()
    {
        if (isWaitingForClose) return;

        foreach (Transform child in gridTransform)
            Destroy(child.gameObject);

        cards.Clear();
        nextNumber = 1; 
        isLocked = false;

        CreateNumberCards();
    }

    private void CreateNumberCards()
    {
        List<int> nums = new List<int>();
        for (int i = 1; i <= 9; i++)
            nums.Add(i);

        Shuffle(nums);

        for (int i = 0; i < 9; i++)
        {
            CollectGameCard newCard = Instantiate(cardPrefab, gridTransform);

            int assignedNumber = nums[i];
            Sprite numberSprite = numberSprites[assignedNumber - 1];
            newCard.SetIconSprite(numberSprite);
            newCard.Hide(); 
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
        if (card.IsRevealed()) return;
        PlaySound(cardClickSound);

        if (number == nextNumber)
        {
            PlaySound(correctSound);
            card.Show();
            nextNumber++;

            if (nextNumber > 9)
            {
                StartCoroutine(CompleteGame());
            }
        }
        else
        {
            StartCoroutine(WrongSelection(card));
        }
    }

    private IEnumerator WrongSelection(CollectGameCard card)
    {
        isLocked = true; 
        card.Show();
        yield return new WaitForSeconds(0.5f);

        card.Hide();

        isLocked = false;
    }

    private IEnumerator CompleteGame()
    {
        isLocked = true;
        yield return new WaitForSeconds(0.8f);

        if (gamesCompleted < totalGamesNeeded)
        {
            gamesCompleted++;
            UpdateProgress();
            
            if (gamesCompleted >= totalGamesNeeded)
            {
                PlaySound(completeSound);
                
                if (collectingManager != null)
                {
                    isWaitingForClose = true;
                    
                    collectingManager.CollectAll(
                        onSuccess: (response) => 
                        {
                            if (fullSummaryManager != null)
                            {
                                fullSummaryManager.GetFullSummary(
                                    onSuccess: (summaryResponse) => 
                                    {
                                        if (closeBtn != null)
                                        {
                                            closeBtn.onClick.Invoke();
                                        }
                                    },
                                    onError: (summaryError) => 
                                    {
                                        if (closeBtn != null)
                                        {
                                            closeBtn.onClick.Invoke();
                                        }
                                    }
                                );
                            }
                            else
                            {
                                if (closeBtn != null)
                                {
                                    closeBtn.onClick.Invoke();
                                }
                            }
                        },
                        onError: (error) => 
                        {
                            if (closeBtn != null)
                            {
                                closeBtn.onClick.Invoke();
                            }
                        }
                    );
                }
            }
        }

        if (!isWaitingForClose)
        {
            StartNewGame();
        }
    }

    private void UpdateProgress()
    {
        int filled = gamesCompleted * 10; 

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