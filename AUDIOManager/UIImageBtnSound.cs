using UnityEngine;

public class UIImageBtnSound : MonoBehaviour
{
    public void PlayClickSound()
    {
        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayButtonClick();
        }
    }
}
