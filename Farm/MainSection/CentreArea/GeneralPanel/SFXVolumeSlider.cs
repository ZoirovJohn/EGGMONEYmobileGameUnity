using UnityEngine;
using UnityEngine.UI;

public class SFXVolumeSlider : MonoBehaviour
{
    [Header("UI Components")]
    public Scrollbar sfxScrollbar;   // Interactive scrollbar
    public Image fillImage;          // Fill image for visual effect

    private void Start()
    {
        // Initialize the scrollbar value based on current SFX volume
        if (GameAudioManager.Instance != null)
            sfxScrollbar.value = GameAudioManager.Instance.sfxSource.volume;

        // Add listener to handle value changes
        sfxScrollbar.onValueChanged.AddListener(OnValueChanged);

        // Update fill image immediately
        OnValueChanged(sfxScrollbar.value);
    }

    private void OnValueChanged(float value)
    {
        // Update SFX volume in GameAudioManager
        if (GameAudioManager.Instance != null && GameAudioManager.Instance.sfxSource != null)
            GameAudioManager.Instance.sfxSource.volume = value;

        // Update visual fill image
        if (fillImage != null)
            fillImage.fillAmount = value; // Left-to-right fill
    }
}
