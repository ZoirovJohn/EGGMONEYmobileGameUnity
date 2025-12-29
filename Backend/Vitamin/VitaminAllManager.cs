using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public class VitaminAllManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config;

    [Header("References")]
    [SerializeField] private FarmDatabase farmDatabase;
    [SerializeField] private PlayerWallet playerWallet;

    public FarmDatabase GetFarmDatabase()
    {
        return farmDatabase;
    }

    private void Awake()
    {
        if (farmDatabase == null)
            farmDatabase = FindAnyObjectByType<FarmDatabase>();
        
        if (playerWallet == null)
            playerWallet = FindAnyObjectByType<PlayerWallet>();
    }

    /// <summary>
    /// Apply vitamin to all hens in a farm
    /// </summary>
    /// <param name="farmNumber">Farm number (1-based index)</param>
    /// <param name="vitaminType">"vitamin" or "super_vitamin"</param>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public void ApplyVitaminToAll(int farmNumber, string vitaminType, Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(ApplyVitaminToAllCoroutine(farmNumber, vitaminType, onSuccess, onError));
    }

    /// <summary>
    /// Apply vitamin using farm index (0-based) from FarmDatabase
    /// </summary>
    public void ApplyVitaminToAllByIndex(int farmIndex, string vitaminType, Action<string> onSuccess = null, Action<string> onError = null)
    {
        if (farmDatabase == null)
        {
            onError?.Invoke("FarmDatabase not found!");
            return;
        }

        FarmData farm = farmDatabase.GetFarmByIndex(farmIndex);
        if (farm == null)
        {
            onError?.Invoke($"Farm at index {farmIndex} not found!");
            return;
        }

        // Convert 0-based index to 1-based farm number
        int farmNumber = farmIndex + 1;

        ApplyVitaminToAll(farmNumber, vitaminType, onSuccess, onError);
    }

    private IEnumerator ApplyVitaminToAllCoroutine(int farmNumber, string vitaminType, Action<string> onSuccess, Action<string> onError)
    {
        string accessToken = AuthStorage.GetAccessToken();

        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        // Determine type based on vitamin
        string type = vitaminType == "super_vitamin" ? "premium" : "normal";

        // Create request body
        VitaminAllRequest requestData = new VitaminAllRequest
        {
            type = type
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // Build URL
        string url = $"{config.baseUrl}/food/vitamin/{farmNumber}/all";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            string errorMsg = $"Error: {request.error}";
            onError?.Invoke(request.error);
        }
    }

    [Serializable]
    private class VitaminAllRequest
    {
        public string type; // "normal" or "premium"
    }

    [Serializable]
    public class VitaminAllResponse
    {
        public bool ok;
        public int hensAffected;
        public int vitaminsUsed;
        public int farmNumber;
    }
}