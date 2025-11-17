using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class EggHatchAPI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] APIConfig config;
    
    [Header("References")]
    [SerializeField] PlayerWallet wallet;
    
    [Header("Optional - Auto Find")]
    [SerializeField] bool autoFind = true;

    void Awake()
    {
        if (autoFind && !wallet)
        {
            wallet = FindAnyObjectByType<PlayerWallet>();
        }
    }

    /// <summary>
    /// Hatch eggs and update wallet
    /// </summary>
    /// <param name="eggType">"silver_egg" or "gold_egg"</param>
    /// <param name="quantity">Number of eggs to hatch</param>
    /// <param name="onSuccess">Called when hatching succeeds</param>
    /// <param name="onError">Called when hatching fails</param>
    public void HatchEggs(string eggType, int quantity, Action<HatchResponse> onSuccess, Action<string> onError)
    {
        if (!config)
        {
            onError?.Invoke("APIConfig reference is missing!");
            return;
        }

        if (!wallet)
        {
            onError?.Invoke("PlayerWallet reference is missing!");
            return;
        }

        if (quantity <= 0)
        {
            onError?.Invoke("Quantity must be greater than 0");
            return;
        }

        // Determine tier based on egg type
        string tier = eggType == "gold_egg" ? "gold" : "normal";
        
        Debug.Log($"🥚 Starting hatch request: {quantity}x {eggType} (tier: {tier})");
        
        StartCoroutine(HatchEggsCoroutine(tier, quantity, eggType, onSuccess, onError));
    }

    IEnumerator HatchEggsCoroutine(string tier, int quantity, string eggType, Action<HatchResponse> onSuccess, Action<string> onError)
    {
        // Get the access token
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        string url = $"{config.baseUrl}/inventory/hatch";
        
        // Create request body
        HatchRequest requestBody = new HatchRequest
        {
            tier = tier,
            quantity = quantity
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log($"📤 POST {url} - Body: {jsonBody}");
        
        // Create POST request
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            
            // Send request
            yield return request.SendWebRequest();
            
            // Handle response
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"✅ Hatch response: {responseText}");
                
                try
                {
                    HatchResponse response = JsonUtility.FromJson<HatchResponse>(responseText);
                    
                    if (response.ok)
                    {
                        // Update wallet - add chicks and remove eggs
                        UpdateWalletAfterHatch(eggType, quantity, response);
                        
                        Debug.Log($"🐣 Successfully hatched {response.hatched} egg(s)!");
                        onSuccess?.Invoke(response);
                    }
                    else
                    {
                        Debug.LogError("❌ Hatch failed: ok=false in response");
                        onError?.Invoke("Hatching failed");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"❌ Failed to parse hatch response: {e.Message}");
                    onError?.Invoke($"Failed to parse response: {e.Message}");
                }
            }
            else
            {
                string errorMsg = $"Request failed: {request.error}";
                Debug.LogError($"❌ {errorMsg}");
                onError?.Invoke(errorMsg);
            }
        }
    }

    void UpdateWalletAfterHatch(string eggType, int quantity, HatchResponse response)
    {
        if (!wallet) return;
        
        // Get current counts
        int currentEggs = wallet.GetItemCount(eggType);
        int currentChicks = wallet.GetItemCount("chick");
        
        Debug.Log($"📊 Before hatch - Eggs: {currentEggs}, Chicks: {currentChicks}");
        
        // Remove eggs (based on hatched count from server)
        int eggsToRemove = response.hatched;
        if (wallet.TryConsumeItem(eggType, eggsToRemove))
        {
            Debug.Log($"✅ Removed {eggsToRemove}x {eggType}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Failed to remove eggs (might not have enough)");
        }
        
        // Add chicks (based on hatched count)
        wallet.AddItem("chick", response.hatched);
        Debug.Log($"✅ Added {response.hatched} chick(s)");
        
        // Log final state
        int newEggs = wallet.GetItemCount(eggType);
        int newChicks = wallet.GetItemCount("chick");
        Debug.Log($"📊 After hatch - Eggs: {newEggs}, Chicks: {newChicks}");
    }

    // =========================
    // Data Classes
    // =========================
    [Serializable]
    public class HatchRequest
    {
        public string tier;
        public int quantity;
    }

    [Serializable]
    public class HatchResponse
    {
        public bool ok;
        public int hatched;
        public ChickData chick;
    }

    [Serializable]
    public class ChickData
    {
        public string id;
        public string userId;
        public string itemType;
        public string stage;
        public string kind;
        public int lifetimeDaysRemaining;
        public float baseSpeed;
        public string hatchedAt;
        public string grows_at;
        public bool foodGiven;
        public bool cleaned;
        public bool isChampion;
        public string tier;
        public int quantity;
        public string status;
        public string meta;
        public string source;
        public string createdAt;
    }
}