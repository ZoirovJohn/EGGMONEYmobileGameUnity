using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    // This runs when the button is clicked
    private void OnButtonClick()
    {
        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayButtonClick();
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
