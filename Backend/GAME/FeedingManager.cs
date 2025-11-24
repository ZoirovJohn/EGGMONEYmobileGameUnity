using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FeedingManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config; // Assign in Inspector

    /// <summary>
    /// Call this method when Simon game reaches 100% completion
    /// </summary>
    public void FeedAll(Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(FeedAllCoroutine(onSuccess, onError));
    }

    private IEnumerator FeedAllCoroutine(Action<string> onSuccess, Action<string> onError)
    {
        string accessToken = AuthStorage.GetAccessToken();

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("No access token found. Cannot feed.");
            onError?.Invoke("No access token found");
            yield break;
        }

        string url = config.baseUrl + "/food/use/all";

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
                FeedResponse feedData = JsonUtility.FromJson<FeedResponse>(response);

                if (feedData.ok)
                {
                    Debug.Log("✅ Feeding finished!");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    Debug.LogWarning("Feeding returned ok: false");
                    onError?.Invoke("Feeding failed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse feeding response: " + e.Message);
                onError?.Invoke("Parse error: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Feed API Error: " + request.error);
            onError?.Invoke(request.error);
        }
    }

    [Serializable]
    private class FeedResponse
    {
        public bool ok;
        public int fed;
        public int totalHens;
        public int skipped;
        public string message;
    }
}