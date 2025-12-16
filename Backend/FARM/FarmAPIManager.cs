using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class FarmAPIManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config;

    [Header("References")]
    [SerializeField] private FarmDatabase farmDatabase;

    /// <summary>
    /// Fetch farm summary data from backend
    /// </summary>
    public void GetFarmSummary(
        int farmNumber,
        Action<FarmSummary> onSuccess = null,
        Action<string> onError = null)
    {
        StartCoroutine(GetFarmSummaryCoroutine(farmNumber, onSuccess, onError));
    }

    private IEnumerator GetFarmSummaryCoroutine(
        int farmNumber,
        Action<FarmSummary> onSuccess,
        Action<string> onError)
    {
        string accessToken = AuthStorage.GetAccessToken();

        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        string url = $"{config.baseUrl}/farm/{farmNumber}/summary";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string jsonResponse = request.downloadHandler.text;
                FarmSummary summary = JsonUtility.FromJson<FarmSummary>(jsonResponse);

                // 🔹 Existing callback behavior
                onSuccess?.Invoke(summary);

                // ✅ Store nest + hen lifetime data (ARRAY → LIST conversion ONLY HERE)
                if (farmDatabase != null &&
                    summary.nests != null &&
                    summary.nests.details != null)
                {
                    List<NestDetail> nestList =
                        new List<NestDetail>(summary.nests.details);

                    farmDatabase.SetFarmNestDetails(
                        farmNumber - 1,
                        nestList
                    );

                    Debug.Log($"✅ Stored nest & hen lifetime data for farm {farmNumber}");
                }
                else
                {
                    Debug.Log(
                        $"ℹ️ Hen lifetime skipped for farm {farmNumber} (will be handled by PlayerDataLoader)"
                    );
                }

                Debug.Log($"✅ Successfully loaded farm {farmNumber} summary");
            }
            catch (Exception e)
            {
                onError?.Invoke($"Failed to parse farm data: {e.Message}");
                Debug.LogError($"❌ JSON Parse Error: {e.Message}");
            }
        }
        else
        {
            onError?.Invoke(request.error);
            Debug.LogError($"❌ API Error: {request.error}");
        }
    }

    /// <summary>
    /// Load all farm summaries for a user
    /// </summary>
    public void LoadAllFarmSummaries(
        int farmCount,
        Action<FarmSummary[]> onAllLoaded = null,
        Action<string> onError = null)
    {
        StartCoroutine(LoadAllFarmSummariesCoroutine(farmCount, onAllLoaded, onError));
    }

    private IEnumerator LoadAllFarmSummariesCoroutine(
        int farmCount,
        Action<FarmSummary[]> onAllLoaded,
        Action<string> onError)
    {
        FarmSummary[] summaries = new FarmSummary[farmCount];
        int loadedCount = 0;
        bool hasError = false;

        for (int i = 0; i < farmCount; i++)
        {
            int farmNumber = i + 1;
            bool completed = false;

            GetFarmSummary(
                farmNumber,
                (summary) =>
                {
                    summaries[i] = summary;
                    loadedCount++;
                    completed = true;
                    Debug.Log($"✅ Loaded farm {farmNumber} ({loadedCount}/{farmCount})");
                },
                (error) =>
                {
                    hasError = true;
                    completed = true;
                    onError?.Invoke($"Failed to load farm {farmNumber}: {error}");
                    Debug.LogError($"❌ Failed to load farm {farmNumber}: {error}");
                }
            );

            yield return new WaitUntil(() => completed);

            if (hasError)
                yield break;
        }

        if (loadedCount == farmCount)
        {
            onAllLoaded?.Invoke(summaries);
        }
    }
}
