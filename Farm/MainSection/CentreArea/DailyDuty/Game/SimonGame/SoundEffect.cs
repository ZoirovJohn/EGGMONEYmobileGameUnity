using UnityEngine;
using UnityEngine.UI;

public class SoundEffect : MonoBehaviour
{
    public ButtonColor buttonColor;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        MiniGameAudioManager.Instance.PlayButtonSound(buttonColor);
    }
}