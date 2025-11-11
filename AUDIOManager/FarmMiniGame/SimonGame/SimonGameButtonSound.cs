using UnityEngine;
using UnityEngine.UI;

public class SimonGameButtonSound : MonoBehaviour
{
    private Button button;

    [Header("Button Sound")]
    public AudioClip buttonClip; // assign the unique clip for this button

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        if (GameAudioManager.Instance != null && buttonClip != null)
        {
            GameAudioManager.Instance.PlaySFX(buttonClip);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}
