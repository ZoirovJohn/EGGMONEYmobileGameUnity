using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup canvasGroup; // add a CanvasGroup component

    public Sprite hiddenIconSprite;

    private Sprite iconSprite;

    [HideInInspector] public bool isSelected;
    [HideInInspector] public CardController controller;

    private bool isFlipping = false;
    private bool isMatched = false;
    private float flipDuration = 0.3f;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnCardClick()
    {
        if (!isFlipping && !isMatched)
            controller.SetSelected(this);
    }

    public void SetIconSprite(Sprite sp)
    {
        iconSprite = sp;
    }

    public Sprite GetIconSprite()
    {
        return iconSprite;
    }

    public void Show()
    {
        if (isFlipping || isMatched) return;
        StartCoroutine(FlipCard(iconSprite));
        isSelected = true;
    }

    public void Hide()
    {
        if (isFlipping || isMatched) return;
        StartCoroutine(FlipCard(hiddenIconSprite));
        isSelected = false;
    }

    public void SetMatched()
    {
        isMatched = true;
        isSelected = true;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float duration = 0.2f;
        float t = 0f;
        float startAlpha = canvasGroup.alpha;

        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false; // disable clicks
    }

    private IEnumerator FlipCard(Sprite targetSprite)
    {
        isFlipping = true;

        Quaternion startRot = transform.rotation;
        Quaternion midRot = Quaternion.Euler(0f, 90f, 0f);
        Quaternion endRot = Quaternion.Euler(0f, 0f, 0f);

        float halfDuration = flipDuration / 2f;
        float t = 0f;

        // First half rotation
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, midRot, t / halfDuration);
            yield return null;
        }
        transform.rotation = midRot;

        // Swap sprite
        iconImage.sprite = targetSprite;

        // Second half rotation
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
}
