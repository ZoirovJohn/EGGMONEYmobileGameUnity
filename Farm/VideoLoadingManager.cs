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

    // ✅ Still allowed to register videos (NO waiting)
    public void RegisterVideo(VideoPlayer player)
    {
        if (player == null) return;
        if (registeredPlayers.Contains(player)) return;

        registeredPlayers.Add(player);
    }

    private void Start()
    {
        StartCoroutine(FakeLoadingRoutine());
    }

    private IEnumerator FakeLoadingRoutine()
    {
        loadingPanel.SetActive(true);

        // Non-linear steps (visual only)
        int[] steps = { 10, 35, 50, 70, 85, 100 };

        float totalDuration = 4f; // ⏱ fixed 4 seconds
        float stepTime = totalDuration / steps.Length;

        int dotCount = 0;

        for (int i = 0; i < steps.Length; i++)
        {
            dotCount++;
            if (dotCount > 3) dotCount = 1;

            string dots = new string('.', dotCount).PadRight(4, ' ');

            loadingText.text = $"Loading{dots} {steps[i]}%";

            yield return new WaitForSeconds(stepTime);
        }

        loadingPanel.SetActive(false);
    }
}
