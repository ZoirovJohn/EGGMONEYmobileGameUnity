using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CleanupManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config; // Assign in Inspector

    /// <summary>
    /// Call this method when a game reaches 100% completion
    /// </summary>
    public void CleanAll(Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(CleanAllCoroutine(onSuccess, onError));
    }

    private IEnumerator CleanAllCoroutine(Action<string> onSuccess, Action<string> onError)
    {
        string accessToken = AuthStorage.GetAccessToken();

        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        string url = config.baseUrl + "/clean/all";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(new byte[0]); // Empty body
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;

            try
            {
                CleanResponse cleanData = JsonUtility.FromJson<CleanResponse>(response);

                if (cleanData.ok)
                {
                    onSuccess?.Invoke(response);
                }
                else
                {
                    onError?.Invoke("Cleanup failed");
                }
            }
            catch (Exception e)
            {
                onError?.Invoke("Parse error: " + e.Message);
            }
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    [Serializable]
    private class CleanResponse
    {
        public bool ok;
        public int cleaned;
        public string reason;
    }
}