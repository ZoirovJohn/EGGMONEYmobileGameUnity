using UnityEngine;

public class MiniGameAudioManager : MonoBehaviour
{
    public static MiniGameAudioManager Instance;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Button Sounds")]
    public AudioClip redButtonSound;
    public AudioClip greenButtonSound;
    public AudioClip yellowButtonSound;
    public AudioClip blueButtonSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButtonSound(ButtonColor color)
    {
        AudioClip clip = null;

        switch (color)
        {
            case ButtonColor.Red:
                clip = redButtonSound;
                break;
            case ButtonColor.Green:
                clip = greenButtonSound;
                break;
            case ButtonColor.Yellow:
                clip = yellowButtonSound;
                break;
            case ButtonColor.Blue:
                clip = blueButtonSound;
                break;
        }

        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}

public enum ButtonColor
{
    Red,
    Green,
    Yellow,
    Blue
}