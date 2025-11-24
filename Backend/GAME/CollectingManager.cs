using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CollectingManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config; // Assign in Inspector

    /// <summary>
    /// Call this method when your next game reaches 100% completion
    /// </summary>
    public void CollectAll(Action<CollectResponse> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(CollectAllCoroutine(onSuccess, onError));
    }

    private IEnumerator CollectAllCoroutine(Action<CollectResponse> onSuccess, Action<string> onError)
    {
        string accessToken = AuthStorage.GetAccessToken();

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("No access token found. Cannot collect eggs.");
            onError?.Invoke("No access token found");
            yield break;
        }

        string url = config.baseUrl + "/farms/collect-all";

        // Empty JSON object as body
        string jsonBody = "{}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            try
            {
                CollectResponse collectData = JsonUtility.FromJson<CollectResponse>(response);

                Debug.Log($"✅ Collection finished! Collected: {collectData.collected}, Basket Count: {collectData.basketEggCount}");
                onSuccess?.Invoke(collectData);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse collection response: " + e.Message);
                onError?.Invoke("Parse error: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Collect API Error: " + request.error);
            onError?.Invoke(request.error);
        }
    }

    [Serializable]
    public class CollectResponse
    {
        public int collected;
        public int basketEggCount;
    }
}