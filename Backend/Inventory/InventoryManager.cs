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
                Debug.Log("No inventory items found.");
                return;
            }

            // Process each item (PlayerWallet will handle adding/updating quantities)
            foreach (var item in inventoryData.items)
            {
                // Only process items that are stored and not assigned to a hen
                if (item.status == "stored" && string.IsNullOrEmpty(item.hen))
                {
                    string productId = MapInventoryItemToProductId(item.itemType, item.tier);
                    
                    if (!string.IsNullOrEmpty(productId))
                    {
                        playerWallet.AddItem(productId, item.quantity);
                        Debug.Log($"Added to inventory: {productId} x{item.quantity}");
                    }
                }
            }

            Debug.Log("Inventory updated successfully!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse inventory response: " + ex.Message);
        }
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
                        Debug.LogWarning($"Unknown egg tier: {tier}");
                        return null;
                }
            
            case "nest":
                return "nest";
            
            case "food":
                return "food";
            
            case "vitamin":
                return "booster";
            
            case "battery":
                return "battery";
            
            case "farmKey":
                return "farmKey";
            
            case "robot":
                return "robot";
            
            default:
                Debug.LogWarning($"Unknown itemType: {itemType}");
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
    public string hen;
    public string tier;
    public int quantity;
    public string status;
    public ItemMeta meta;
    public string source;
    public string createdAt;
}

[Serializable]
public class ItemMeta
{
    public string source;
    public string givenAt;
    public bool starter;
}