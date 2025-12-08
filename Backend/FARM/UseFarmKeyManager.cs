using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public class UseFarmKeyManager : MonoBehaviour
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
    /// Use farm key via API
    /// </summary>
    /// <param name="keyId">Key item ID (e.g., "keyFarm", "premiumfarmkey")</param>
    /// <param name="onSuccess">Callback on success with response</param>
    /// <param name="onError">Callback on error with error message</param>
    public void UseFarmKey(string keyId, Action<UseFarmKeyResponse> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(UseFarmKeyCoroutine(keyId, onSuccess, onError));
    }

    private IEnumerator UseFarmKeyCoroutine(string keyId, Action<UseFarmKeyResponse> onSuccess, Action<string> onError)
    {
        string accessToken = AuthStorage.GetAccessToken();

        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        // Determine type based on key ID
        string type = GetKeyType(keyId);

        if (string.IsNullOrEmpty(type))
        {
            onError?.Invoke($"Unknown key type: {keyId}");
            yield break;
        }

        // Create request body - always quantity 1
        UseFarmKeyRequest requestData = new UseFarmKeyRequest
        {
            type = type,
            quantity = 1
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // Build URL
        string url = $"{config.baseUrl}/farm/use-farm-key";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        Debug.Log($"🔵 POST {url}");
        Debug.Log($"📤 Body: {json}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Farm key used successfully: {request.downloadHandler.text}");
            
            try
            {
                UseFarmKeyResponse response = JsonUtility.FromJson<UseFarmKeyResponse>(request.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Failed to parse response: {e.Message}");
                onError?.Invoke($"Failed to parse response: {e.Message}");
            }
        }
        else
        {
            string errorMsg = $"Error: {request.error}";
            Debug.LogError($"❌ {errorMsg}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
            onError?.Invoke(request.error);
        }
    }

    /// <summary>
    /// Determines the key type based on key ID
    /// ✅ UPDATED: Handles all variations from inventory cells
    /// </summary>
    private string GetKeyType(string keyId)
    {
        // ✅ Generic farm keys = "normal"
        if (keyId == "key_farm" || keyId == "keyFarm" || keyId == "farmKey")
        {
            return "normal";
        }
        
        // ✅ Premium farm keys = "premium"
        if (keyId == "premiumfarmkey" || keyId == "premium_farm_key" || keyId == "premiumFarmKey")
        {
            return "premium";
        }

        // Specific farm keys (4-8) = "normal"
        switch (keyId)
        {
            case "key_farm_4":
            case "farmKey4":
            case "farm_4_key":
            case "key_farm_5":
            case "farmKey5":
            case "farm_5_key":
            case "key_farm_6":
            case "farmKey6":
            case "farm_6_key":
            case "key_farm_7":
            case "farmKey7":
            case "farm_7_key":
            case "key_farm_8":
            case "farmKey8":
            case "farm_8_key":
                return "normal";
            
            default:
                Debug.LogWarning($"⚠️ Unknown key type: {keyId}");
                return "";
        }
    }

    [Serializable]
    private class UseFarmKeyRequest
    {
        public string type;     // "normal" or "premium"
        public int quantity;    // Always 1
    }

    [Serializable]
    public class UseFarmKeyResponse
    {
        public bool ok;
        public string message;
        public FarmUnlockData farm;
    }

    [Serializable]
    public class FarmUnlockData
    {
        public string farmId;
        public string farmName;
        public int farmIndex;
        public string farmKeyType;
        // Add other fields as needed based on your API response
    }
}