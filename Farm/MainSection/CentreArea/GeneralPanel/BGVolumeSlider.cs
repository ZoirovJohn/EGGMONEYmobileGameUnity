using UnityEngine;
using UnityEngine.UI;

public class BGVolumeSlider : MonoBehaviour
{
    public Scrollbar volumeScrollbar; // the interactive scrollbar
    public Image fillImage;           // visual fill bar behind handle

    private void Start()
    {
        volumeScrollbar.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(volumeScrollbar.value); // initialize
    }

    private void OnValueChanged(float value)
    {
        // Update music volume
        GameAudioManager.Instance.musicSource.volume = value;

        // Update visual fill bar
        if (fillImage != null)
            fillImage.fillAmount = value; // left-to-right fill
    }
}
