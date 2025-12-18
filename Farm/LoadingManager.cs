using System.Collections;
using UnityEngine;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;

    public float totalLoadingTime = 10f;

    private int[] percentSteps = { 0, 25, 50, 75, 100 };
    private string[] dotSteps = { ".", "..", "...", "." };

    private void Start()
    {
        loadingPanel.SetActive(true);
        StartCoroutine(LoadingRoutine());
    }

    IEnumerator LoadingRoutine()
    {
        float stepTime = totalLoadingTime / (percentSteps.Length - 1);

        for (int i = 0; i < percentSteps.Length; i++)
        {
            string dots = dotSteps[i % dotSteps.Length];
            loadingText.text = $"Loading{dots} {percentSteps[i]}%";
            yield return new WaitForSeconds(stepTime);
        }

        loadingPanel.SetActive(false);
    }
}
