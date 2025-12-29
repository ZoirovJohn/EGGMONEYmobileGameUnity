using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class FullSummaryManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config;

    [Header("References")]
    [SerializeField] private PlayerWallet playerWallet;

    public void GetFullSummary(Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(GetFullSummaryCoroutine(onSuccess, onError));
    }

    private IEnumerator GetFullSummaryCoroutine(Action<string> onSuccess, Action<string> onError)
    {
        string accessToken = AuthStorage.GetAccessToken();

        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        string url = $"{config.baseUrl}/farm/full-summary";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;

                // Parse and store the data
                try
                {
                    FullSummaryResponse response = JsonUtility.FromJson<FullSummaryResponse>(responseText);

                    // Store in PlayerWallet
                    if (playerWallet != null)
                    {
                        playerWallet.SetHensWithEggReady(response.counts.hensWithEggReady);
                        playerWallet.SetHensNeedingFood(response.counts.needsFood);
                        playerWallet.SetHensNeedingClean(response.counts.needsClean);
                    }

                    onSuccess?.Invoke(responseText);
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Parse error: {e.Message}");
                }
            }
            else
            {
                string errorMsg = $"Error {request.responseCode}: {request.error}";
                onError?.Invoke(errorMsg);
            }
        }
    }

    [Serializable]
    public class FullSummaryResponse
    {
        public NestData nests;
        public FarmEgg[] farmEggs;
        public CountsData counts;
    }

    [Serializable]
    public class NestData
    {
        public int totalCapacity;
        public int occupied;
        public int available;
    }

    [Serializable]
    public class FarmEgg
    {
        // Add farm egg properties if needed
    }

    [Serializable]
    public class CountsData
    {
        public int chicks;
        public int hens;
        public int hensWithEggReady;
        public int needsFood;
        public int needsClean;
    }
}