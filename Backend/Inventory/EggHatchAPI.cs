using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class EggHatchAPI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] APIConfig config;
    
    [Header("References")]
    [SerializeField] PlayerWallet wallet;
    [SerializeField] InventoryManager inventoryManager;
    
    [Header("Optional - Auto Find")]
    [SerializeField] bool autoFind = true;
    
    [Header("Delayed Refresh Settings")]
    [SerializeField] float delayedRefreshTime = 130f; // 130 seconds

    // ✅ Track ALL scheduled delayed refreshes (one per hatch)
    private List<Coroutine> scheduledRefreshes = new List<Coroutine>();

    void Awake()
    {
        if (autoFind)
        {
            if (!wallet)
                wallet = FindAnyObjectByType<PlayerWallet>();
            
            if (!inventoryManager)
                inventoryManager = FindAnyObjectByType<InventoryManager>();
        }
    }

    /// <summary>
    /// Hatch eggs and update wallet
    /// </summary>
    /// <param name="eggType">"silver_egg", "gold_egg", "super_red_egg", or "super_blue_egg"</param>
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
        string tier = MapEggTypeToTier(eggType);
        
        Debug.Log($"🥚 Starting hatch request: {quantity}x {eggType} (tier: {tier})");
        
        StartCoroutine(HatchEggsCoroutine(tier, quantity, eggType, onSuccess, onError));
    }

    string MapEggTypeToTier(string eggType)
    {
        switch (eggType)
        {
            case "silver_egg":
                return "normal";
            case "gold_egg":
                return "gold";
            case "super_red_egg":
                return "red";
            case "super_blue_egg":
                return "blue";
            default:
                Debug.LogWarning($"Unknown egg type: {eggType}, defaulting to 'normal'");
                return "normal";
        }
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
                        // ✅ Record the hatch time
                        string hatchTime = DateTime.Now.ToString("HH:mm:ss");
                        string delayedTime = DateTime.Now.AddSeconds(delayedRefreshTime).ToString("HH:mm:ss");
                        
                        // Check if it's a super egg (blue/red)
                        bool isSuperEgg = eggType == "super_red_egg" || eggType == "super_blue_egg";
                        
                        if (isSuperEgg)
                        {
                            Debug.Log($"🎁 Super egg hatched! Reward: {response.reward}");
                            
                            // ✅ Refresh inventory from backend to get updated data (IMMEDIATE)
                            if (inventoryManager != null)
                            {
                                Debug.Log($"📦 Refreshing inventory from backend (IMMEDIATE at {hatchTime})...");
                                
                                inventoryManager.GetInventory(
                                    onSuccess: (invResponse) =>
                                    {
                                        Debug.Log($"✅ Inventory refreshed after super egg hatch at {hatchTime}");
                                        onSuccess?.Invoke(response);
                                    },
                                    onError: (err) =>
                                    {
                                        Debug.LogWarning($"⚠️ Failed to refresh inventory after hatch: {err}");
                                        // Still call success since hatching worked
                                        onSuccess?.Invoke(response);
                                    }
                                );
                                
                                // ✅ SCHEDULE INDIVIDUAL DELAYED REFRESH for this specific hatch
                                ScheduleDelayedRefresh(hatchTime, delayedTime);
                            }
                            else
                            {
                                Debug.LogWarning("⚠️ InventoryManager not found, can't refresh inventory");
                                onSuccess?.Invoke(response);
                            }
                        }
                        else
                        {
                            // Normal or gold egg - update wallet locally (IMMEDIATE)
                            UpdateWalletAfterHatch(eggType, quantity, response);
                            Debug.Log($"🐣 Successfully hatched {response.hatched} egg(s) at {hatchTime}!");
                            onSuccess?.Invoke(response);
                            
                            // ✅ SCHEDULE INDIVIDUAL DELAYED REFRESH for this specific hatch
                            if (inventoryManager != null)
                            {
                                ScheduleDelayedRefresh(hatchTime, delayedTime);
                            }
                        }
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

    /// <summary>
    /// Schedules a NEW delayed inventory refresh for this specific hatch
    /// Does NOT cancel previous ones - each hatch gets its own timer
    /// </summary>
    void ScheduleDelayedRefresh(string hatchTime, string delayedTime)
    {
        // ✅ Start a NEW delayed refresh (independent of others)
        Coroutine newRefresh = StartCoroutine(DelayedInventoryRefresh(delayedRefreshTime, hatchTime, delayedTime));
        scheduledRefreshes.Add(newRefresh);
        
        Debug.Log($"⏱️ Scheduled refresh #{scheduledRefreshes.Count}: Hatched at {hatchTime}, will refresh at {delayedTime}");
    }

    /// <summary>
    /// Refreshes inventory after a delay (e.g., 130 seconds for chick growth)
    /// Each hatch gets its own independent timer
    /// </summary>
    IEnumerator DelayedInventoryRefresh(float delaySeconds, string hatchTime, string delayedTime)
    {
        Debug.Log($"⏱️ [Hatch {hatchTime}] Waiting {delaySeconds} seconds until {delayedTime}...");
        
        yield return new WaitForSeconds(delaySeconds);
        
        if (inventoryManager != null)
        {
            string actualTime = DateTime.Now.ToString("HH:mm:ss");
            Debug.Log($"🔄 [Hatch {hatchTime}] Executing DELAYED refresh now at {actualTime} (expected {delayedTime})...");
            
            inventoryManager.GetInventory(
                onSuccess: (response) =>
                {
                    Debug.Log($"✅ [Hatch {hatchTime}] DELAYED refresh complete at {actualTime}!");
                },
                onError: (err) =>
                {
                    Debug.LogWarning($"⚠️ [Hatch {hatchTime}] Delayed refresh failed: {err}");
                }
            );
        }
        else
        {
            Debug.LogWarning($"⚠️ [Hatch {hatchTime}] InventoryManager not available for delayed refresh");
        }
        
        // Clean up completed coroutine from list
        scheduledRefreshes.Remove(StartCoroutine(DelayedInventoryRefresh(delaySeconds, hatchTime, delayedTime)));
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
        
        // For normal/gold eggs (returns chick)
        public int hatched;
        public ChickData chick;
        
        // For super eggs (returns kind or reward)
        public string kind;     // "Champ" for red eggs
        public string reward;   // "1 premium nest" for blue eggs
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