using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour
{
    [Header("References")]
    public Image cardImage;  // the card itself (swapped)
    public Image iconImage;  // center icon

    [Header("Sprites")]
    public Sprite hiddenSprite;   // your black/back image
    public Sprite cardBgSprite;   // front card background

    [HideInInspector] public CardController controller;
    [HideInInspector] public bool isSelected = false;

    private Sprite iconSprite;
    private bool isFlipping = false;
    private bool isMatched = false;
    private float flipDuration = 0.3f;

    public void SetIconSprite(Sprite sp)
    {
        iconSprite = sp;
        iconImage.sprite = sp;
    }

    public Sprite GetIconSprite() => iconSprite;

    public void OnCardClick()
    {
        if (!isFlipping && !isMatched)
            controller.SetSelected(this);
    }

    public void Show()
    {
        if (isFlipping || isMatched) return;
        StartCoroutine(FlipCard(true));
        isSelected = true;
    }

    public void Hide()
    {
        if (isFlipping || isMatched) return;
        StartCoroutine(FlipCard(false));
        isSelected = false;
    }

    public void SetMatched()
    {
        isMatched = true;
        StartCoroutine(FadeOutCard());
    }

    private IEnumerator FlipCard(bool revealed)
    {
        isFlipping = true;

        Quaternion startRot = transform.rotation;
        Quaternion midRot = Quaternion.Euler(0, 90, 0);
        Quaternion endRot = Quaternion.Euler(0, 0, 0);
        float halfDuration = flipDuration / 2f;
        float t = 0f;

        // Rotate to 90°
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, midRot, t / halfDuration);
            yield return null;
        }
        transform.rotation = midRot;

        // Swap sprite
        cardImage.sprite = revealed ? cardBgSprite : hiddenSprite;
        iconImage.enabled = revealed;

        // Rotate back
        t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(midRot, endRot, t / halfDuration);
            yield return null;
        }
        transform.rotation = endRot;

        isFlipping = false;
    }

    private IEnumerator FadeOutCard()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        float t = 0f;
        float duration = 0.2f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }
        cg.alpha = 0f;
        cg.blocksRaycasts = false; // prevent clicks
    }
}
