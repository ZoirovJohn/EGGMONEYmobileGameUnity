using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class VideoLoadingManager : MonoBehaviour
{
    public static VideoLoadingManager Instance;

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    private readonly List<VideoPlayer> registeredPlayers = new();
    private int preparedCount = 0;

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

    public void RegisterVideo(VideoPlayer player)
    {
        if (player == null) return;
        if (registeredPlayers.Contains(player)) return;

        registeredPlayers.Add(player);

        // ❌ DO NOT prepare disabled players
        if (!player.gameObject.activeInHierarchy || !player.enabled)
        {
            Debug.Log($"⏭ Skipping prepare (disabled): {player.name}");
            return;
        }

        if (player.isPrepared)
        {
            preparedCount++;
        }
        else
        {
            player.prepareCompleted += OnVideoPrepared;
            player.Prepare();
        }
    }


    private void Start()
    {
        StartCoroutine(FakeLoadingRoutine());
    }

    private IEnumerator FakeLoadingRoutine()
    {
        loadingPanel.SetActive(true);

        // Loading steps (not sequential)
        int[] steps = { 1, 5, 7, 14, 23, 35, 50, 68, 82, 100 };

        float totalDuration = 10f;
        float stepTime = totalDuration / steps.Length;

        int dotCount = 0;

        for (int i = 0; i < steps.Length; i++)
        {
            // 👇 dotCount cycles: 1 → 2 → 3 → 1 ...
            dotCount++;
            if (dotCount > 3) dotCount = 1;

            string dots = new string('.', dotCount);

            loadingText.text = $"Loading{dots} {steps[i]}%";

            yield return new WaitForSeconds(stepTime);
        }

        yield return StartCoroutine(WaitForVideos());

        loadingPanel.SetActive(false);
    }

    private IEnumerator WaitForVideos()
    {
        if (registeredPlayers.Count == 0)
            yield break;

        float timeout = 5f;
        float elapsed = 0f;

        while (preparedCount < registeredPlayers.Count && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        preparedCount++;
        vp.prepareCompleted -= OnVideoPrepared;
    }
}
