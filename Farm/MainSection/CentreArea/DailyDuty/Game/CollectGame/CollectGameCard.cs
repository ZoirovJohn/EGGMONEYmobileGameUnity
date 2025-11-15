using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CollectGameCard : MonoBehaviour
{
    [Header("References")]
    public Image cardImage;  // the card itself (swapped)
    public Image iconImage;  // center icon

    [Header("Sprites")]
    public Sprite hiddenSprite;   // back of the card
    public Sprite cardBgSprite;   // front card background

    private Sprite iconSprite;
    private bool isFlipping = false;
    private bool isRevealed = false; // Track if card is currently showing

    public void SetIconSprite(Sprite sp)
    {
        iconSprite = sp;
        if (iconImage != null)
        {
            iconImage.sprite = sp;
            iconImage.enabled = false; // ✅ Start hidden
        }
    }

    public Sprite GetIconSprite() => iconSprite;

    public bool IsRevealed() => isRevealed;

    public void Show()
    {
        if (isFlipping || isRevealed) return; // Don't show if already revealed
        StartCoroutine(FlipCard(true));
    }

    public void Hide()
    {
        if (isFlipping || !isRevealed) return; // Don't hide if already hidden
        StartCoroutine(FlipCard(false));
    }

    private IEnumerator FlipCard(bool revealed)
    {
        isFlipping = true;

        Quaternion startRot = transform.rotation;
        Quaternion midRot = Quaternion.Euler(0, 90, 0);
        Quaternion endRot = Quaternion.Euler(0, 0, 0);
        float halfDuration = 0.15f; // Faster flip
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
        if (cardImage != null) cardImage.sprite = revealed ? cardBgSprite : hiddenSprite;
        if (iconImage != null) iconImage.enabled = revealed;
        
        isRevealed = revealed; // ✅ Update state

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
}