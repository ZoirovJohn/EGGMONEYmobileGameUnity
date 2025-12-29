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
    [SerializeField] float delayedRefreshTime = 13f; // 13 seconds

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
                            // ✅ Refresh inventory from backend to get updated data (IMMEDIATE)
                            if (inventoryManager != null)
                            {
                                inventoryManager.GetInventory(
                                    onSuccess: (invResponse) =>
                                    {
                                        onSuccess?.Invoke(response);
                                    },
                                    onError: (err) =>
                                    {
                                        // Still call success since hatching worked
                                        onSuccess?.Invoke(response);
                                    }
                                );
                                
                                // ✅ SCHEDULE INDIVIDUAL DELAYED REFRESH for this specific hatch
                                ScheduleDelayedRefresh(hatchTime, delayedTime);
                            }
                            else
                            {
                                onSuccess?.Invoke(response);
                            }
                        }
                        else
                        {
                            // Normal or gold egg - update wallet locally (IMMEDIATE)
                            UpdateWalletAfterHatch(eggType, quantity, response);
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
                        onError?.Invoke("Hatching failed");
                    }
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Failed to parse response: {e.Message}");
                }
            }
            else
            {
                string errorMsg = $"Request failed: {request.error}";
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
    }

    /// <summary>
    /// Refreshes inventory after a delay (e.g., 13 seconds for chick growth)
    /// Each hatch gets its own independent timer
    /// </summary>
    IEnumerator DelayedInventoryRefresh(float delaySeconds, string hatchTime, string delayedTime)
    {
        yield return new WaitForSeconds(delaySeconds);
        
        if (inventoryManager != null)
        {
            string actualTime = DateTime.Now.ToString("HH:mm:ss");
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
        
        // Log final state
        int newEggs = wallet.GetItemCount(eggType);
        int newChicks = wallet.GetItemCount("chick");
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