using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class InventoryManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config; // assign in Inspector

    [Header("Wallet Reference")]
    public PlayerWallet playerWallet;

    // =====================
    // GET INVENTORY
    // =====================
    public void GetInventory(Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(GetInventoryCoroutine(onSuccess, onError));
    }

    private IEnumerator GetInventoryCoroutine(Action<string> onSuccess, Action<string> onError)
    {
        // Get the access token
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(config.baseUrl + "/inventory");
        
        // Add Bearer token authorization
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            
            // Parse and update PlayerWallet
            if (playerWallet != null)
            {
                UpdatePlayerWalletInventory(response);
            }
            
            onSuccess?.Invoke(response);
        }
        else
            onError?.Invoke(request.error);
    }

    // =====================
    // UPDATE PLAYER WALLET
    // =====================
    private void UpdatePlayerWalletInventory(string jsonResponse)
    {
        try
        {
            // Wrap array in object for Unity JsonUtility
            string wrappedJson = "{\"items\":" + jsonResponse + "}";
            InventoryResponse inventoryData = JsonUtility.FromJson<InventoryResponse>(wrappedJson);

            if (inventoryData?.items == null || inventoryData.items.Length == 0)
            {
                return;
            }

            // ✅ Create a dictionary to aggregate quantities by product ID
            System.Collections.Generic.Dictionary<string, int> inventoryCounts = new System.Collections.Generic.Dictionary<string, int>();

            // ✅ Initialize all possible items to 0 (so we SET everything from backend)
            string[] allItems = {
                "chick", "whiteChick", "champChick", "silver_egg", "gold_egg",
                "super_blue_egg", "super_red_egg", "nest", "food", "vitamin",
                "battery", "farmKey", "robot", "super_nest", "super_food",
                "super_vitamin", "super_battery", "super_farm_key"
            };
            
            foreach (string item in allItems)
            {
                inventoryCounts[item] = 0;
            }

            // Process each item from backend
            foreach (var item in inventoryData.items)
            {
                // Only process items that are stored
                if (item.status == "stored")
                {
                    string productId = null;
                    
                    // ✅ Check if item has a VALID hen (not just non-null, but with actual data)
                    bool hasValidHen = item.hen != null && 
                                       !string.IsNullOrEmpty(item.hen.stage) && 
                                       !string.IsNullOrEmpty(item.hen.kind);
                    
                    if (hasValidHen)
                    {
                        // It's a hatched egg with a chicken
                        productId = MapHenToProductId(item.hen);
                    }
                    else
                    {
                        // Regular item (not hatched)
                        productId = MapInventoryItemToProductId(item.itemType, item.tier);
                    }
                    
                    if (!string.IsNullOrEmpty(productId))
                    {
                        // Aggregate quantities
                        if (inventoryCounts.ContainsKey(productId))
                        {
                            inventoryCounts[productId] += item.quantity;
                        }
                        else
                        {
                            inventoryCounts[productId] = item.quantity;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Failed to map item: {item.itemType}/{item.tier}");
                    }
                }
            }

            // ✅ Now SET ALL values in PlayerWallet (this will trigger events for each item)
            foreach (var kvp in inventoryCounts)
            {
                SetPlayerWalletItem(kvp.Key, kvp.Value);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse inventory response: " + ex.Message);
        }
    }

    // ✅ SET item value in wallet using DIRECT SETTERS (triggers events)
    private void SetPlayerWalletItem(string productId, int quantity)
    {
        if (playerWallet == null)
        {
            return;
        }

        // Use the setter methods that directly set values and trigger events
        switch (productId)
        {
            case "chick":
                playerWallet.SetChick(quantity);
                break;
                
            case "whiteChick":
                playerWallet.SetWhiteChick(quantity);
                break;
                
            case "champChick":
                playerWallet.SetChampChick(quantity);
                break;
                
            case "silver_egg":
                playerWallet.SetSilverEgg(quantity);
                break;
                
            case "gold_egg":
                playerWallet.SetGoldEgg(quantity);
                break;
                
            case "super_farm_key":
                playerWallet.SetSuperFarmKey(quantity);
                break;
                
            // ✅ For items without specific setters, use AddItem/TryConsumeItem
            case "super_blue_egg":
            case "super_red_egg":
            case "nest":
            case "food":
            case "vitamin":
            case "battery":
            case "farmKey":
            case "robot":
            case "super_nest":
            case "super_food":
            case "super_vitamin":
            case "super_battery":
                // Get current count and calculate difference
                int currentCount = playerWallet.GetItemCount(productId);
                int difference = quantity - currentCount;
                
                if (difference > 0)
                {
                    // Need to add more
                    playerWallet.AddItem(productId, difference);
                }
                else if (difference < 0)
                {
                    // Need to remove some
                    playerWallet.TryConsumeItem(productId, -difference);
                }
                // else: difference == 0, already correct amount
                break;
                
            default:
                break;
        }
    }

    // =====================
    // MAP HEN DATA TO PRODUCT ID
    // =====================
    private string MapHenToProductId(HenData hen)
    {
        // Safety check for null or empty values
        if (hen == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(hen.stage))
        {
            return null;
        }

        if (string.IsNullOrEmpty(hen.kind))
        {
            return null;
        }

        // Determine product ID based on stage and kind
        if (hen.stage == "chick")
        {
            // Both Normal and Champ chicks use the same product ID
            return "chick";
        }
        else if (hen.stage == "hen")
        {
            // Hen stage depends on kind
            if (hen.kind == "Normal")
            {
                return "whiteChick"; // Normal hen
            }
            else if (hen.kind == "Champ")
            {
                return "champChick"; // Champion hen
            }
            else
            {
                Debug.LogWarning($"Unknown hen kind: {hen.kind}");
                return null;
            }
        }
        return null;
    }

    // =====================
    // MAP API DATA TO PRODUCT IDs
    // =====================
    private string MapInventoryItemToProductId(string itemType, string tier)
    {
        switch (itemType)
        {
            case "egg":
                // Different egg tiers
                switch (tier)
                {
                    case "normal":
                        return "silver_egg";
                    case "gold":
                        return "gold_egg";
                    case "blue":
                        return "super_blue_egg";
                    case "red":
                        return "super_red_egg";
                    default:
                        return null;
                }
            
            case "nest":
                if (tier == "premium" || tier == "super")
                {
                    return "super_nest";
                }
                return "nest";
            
            case "food":
                if (tier == "premium" || tier == "super")
                {
                    return "super_food";
                }
                return "food";
            
            case "vitamin":
                if (tier == "premium" || tier == "super")
                {
                    return "super_vitamin";
                }
                return "vitamin";
            
            case "battery":
                if (tier == "premium" || tier == "super")
                {
                    return "super_battery";
                }
                return "battery";
            
            case "farmKey":
                if (tier == "premium" || tier == "super")
                {
                    return "super_farm_key";
                }
                return "farmKey";
            
            case "robot":
                return "robot";
            
            default:
                return null;
        }
    }
}

// =====================
// DATA CLASSES
// =====================
[Serializable]
public class InventoryResponse
{
    public InventoryItem[] items;
}

[Serializable]
public class InventoryItem
{
    public string id;
    public string userId;
    public string itemType;
    public HenData hen;
    public string tier;
    public int quantity;
    public string status;
    public ItemMeta meta;
    public string source;
    public string createdAt;
}

[Serializable]
public class HenData
{
    public string kind;
    public string stage;
    public bool cleaned;
    public string grows_at;
    public float baseSpeed;
    public bool foodGiven;
    public string hatchedAt;
    public bool isChampion;
    public string promotedAt;
    public int lifetimeDaysRemaining;
}

[Serializable]
public class ItemMeta
{
    public string source;
    public string givenAt;
    public bool starter;
}