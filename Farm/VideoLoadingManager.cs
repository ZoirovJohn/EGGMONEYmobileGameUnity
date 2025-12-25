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

    // ✅ KEEP THIS — other scripts depend on it
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

    // ✅ REQUIRED for existing scripts
    public void RegisterVideo(VideoPlayer player)
    {
        if (player == null) return;
        if (registeredPlayers.Contains(player)) return;

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

        // --------------------
        // VIDEO SETUP
        // --------------------
        videoPlayer.Stop();
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.isLooping = true; // ✅ IMPORTANT
        videoPlayer.time = 0;

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        videoRawImage.texture = videoPlayer.texture;
        videoPlayer.Play();

        // --------------------
        // LOADING TEXT (3s)
        // --------------------
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

        // --------------------
        // CLEANUP
        // --------------------
        videoPlayer.Stop();          // stop loop
        videoRawImage.texture = null;
        loadingPanel.SetActive(false);
    }
}
