using UnityEngine;
using UnityEngine.UI;

public class SwitchToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RectTransform handle;
    [SerializeField] Image background;

    [Header("Sprites")]
    [SerializeField] Sprite onSprite;
    [SerializeField] Sprite offSprite;

    Toggle toggle;
    Vector2 originalPos;

    void Awake()
    {
        toggle = GetComponent<Toggle>();

        originalPos = handle.anchoredPosition;

        toggle.onValueChanged.AddListener(OnSwitch);

        // Apply starting state
        OnSwitch(toggle.isOn);
    }

    void OnSwitch(bool on)
    {
        // Move handle
        handle.anchoredPosition = on 
            ? new Vector2(-originalPos.x, originalPos.y)
            : originalPos;

        // Change background sprite if assigned (optional)
        if (background)
            background.sprite = on ? onSprite : offSprite;
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnSwitch);
    }
}
