using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform gridTransform;
    [SerializeField] private Sprite[] sprites;

    private List<Sprite> spritePairs;
    private Card firstSelected;
    private Card secondSelected;
    private bool isChecking = false; // NEW: Prevent input during check

    private void Start()
    {
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
    }

    private void CreateCards()
    {
        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card newCard = Instantiate(cardPrefab, gridTransform);
            newCard.SetIconSprite(spritePairs[i]);
            newCard.controller = this;
            newCard.Hide(); // start hidden
        }
    }

    public void SetSelected(Card card)
    {
        // NEW: Block input if checking or card already selected
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
        isChecking = true; // NEW: Lock input
        yield return new WaitForSeconds(0.5f);

        if (firstSelected.GetIconSprite() == secondSelected.GetIconSprite())
        {
            // Match found — mark as matched (stay in place)
            firstSelected.SetMatched();
            secondSelected.SetMatched();
        }
        else
        {
            // No match — flip back
            firstSelected.Hide();
            secondSelected.Hide();
        }

        firstSelected = null;
        secondSelected = null;
        isChecking = false; // NEW: Unlock input
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