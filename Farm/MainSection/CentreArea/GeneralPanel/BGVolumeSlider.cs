using UnityEngine;
using UnityEngine.UI;

public class BGVolumeSlider : MonoBehaviour
{
    public Scrollbar volumeScrollbar;
    public Image fillImage;

    private void Start()
    {
        if (GameAudioManager.Instance != null)
        {
            Initialize();
        }
        else
        {
            Invoke(nameof(Initialize), 0.1f);
        }
    }

    private void Initialize()
    {
        if (GameAudioManager.Instance == null)
        {
            return;
        }

        volumeScrollbar.value = GameAudioManager.Instance.GetMusicVolume();
        volumeScrollbar.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(volumeScrollbar.value);
    }

    private void OnValueChanged(float value)
    {
        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.SetMusicVolume(value);
        }

        if (fillImage != null)
            fillImage.fillAmount = value;
    }

    private void OnDestroy()
    {
        if (volumeScrollbar != null)
            volumeScrollbar.onValueChanged.RemoveListener(OnValueChanged);
    }
}
