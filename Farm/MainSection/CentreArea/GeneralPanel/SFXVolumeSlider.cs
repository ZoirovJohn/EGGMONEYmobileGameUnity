using UnityEngine;
using UnityEngine.UI;

public class SFXVolumeSlider : MonoBehaviour
{
    [Header("UI Components")]
    public Scrollbar sfxScrollbar;
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
            Debug.LogError("GameAudioManager.Instance is null!");
            return;
        }

        sfxScrollbar.value = GameAudioManager.Instance.GetSFXVolume();
        sfxScrollbar.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(sfxScrollbar.value);
    }

    private void OnValueChanged(float value)
    {
        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.SetSFXVolume(value);
        }

        if (fillImage != null)
            fillImage.fillAmount = value;
    }

    private void OnDestroy()
    {
        if (sfxScrollbar != null)
            sfxScrollbar.onValueChanged.RemoveListener(OnValueChanged);
    }
}