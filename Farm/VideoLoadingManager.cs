using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;

public class VideoLoadingManager : MonoBehaviour
{
    public static VideoLoadingManager Instance;

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Video")]
    [SerializeField] private RawImage videoRawImage;
    [SerializeField] private VideoPlayer videoPlayer;

    // Required for other scripts
    private readonly List<VideoPlayer> registeredPlayers = new();

    private const float LOADING_DURATION = 3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Required by other scripts
    public void RegisterVideo(VideoPlayer player)
    {
        if (player == null) return;
        if (!registeredPlayers.Contains(player))
            registeredPlayers.Add(player);
    }

    private void Start()
    {
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        loadingPanel.SetActive(true);
        videoRawImage.gameObject.SetActive(true);

        // VIDEO SETUP (RenderTexture driven by Inspector)
        videoPlayer.Stop();
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.isLooping = false;
        videoPlayer.time = 0;

        videoPlayer.Play();

        // LOADING TEXT (exact 3 seconds)
        int[] steps = { 15, 40, 65, 85, 100 };
        float stepTime = LOADING_DURATION / steps.Length;

        int dotCount = 0;

        for (int i = 0; i < steps.Length; i++)
        {
            dotCount = (dotCount % 3) + 1;
            string dots = new string('.', dotCount).PadRight(4, ' ');
            loadingText.text = $"Loading{dots} {steps[i]}%";

            yield return new WaitForSeconds(stepTime);
        }

        // CLEANUP
        videoPlayer.Stop();
        loadingPanel.SetActive(false);
    }


}
