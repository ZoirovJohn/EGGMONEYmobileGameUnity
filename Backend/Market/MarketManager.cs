using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class MarketManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config; // assign in Inspector

    // =====================
    // PURCHASE (Buy item from market)
    // =====================
    public void Purchase(PurchaseData data, Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(PurchaseCoroutine(data, onSuccess, onError));
    }

    private IEnumerator PurchaseCoroutine(PurchaseData data, Action<string> onSuccess, Action<string> onError)
    {
        // Get the access token
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(config.baseUrl + "/market/purchase", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        // ✅ Add Bearer token authorization
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error);
    }
}

// =====================
// ENUMS
// =====================
public enum ItemType
{
    egg,
    nest,
    food,
    vitamin,
    battery,
    farmKey,
    robot
}

public enum EggTier
{
    normal,
    gold,
    blue,
    red
}

// =====================
// DATA CLASSES
// =====================
[Serializable]
public class PurchaseData
{
    public string itemType;
    public string tier;
    public int quantity;
    
    // Constructor for eggs with specific tier
    public PurchaseData(ItemType itemType, EggTier eggTier, int quantity = 1)
    {
        this.itemType = itemType.ToString();
        this.tier = eggTier.ToString();
        this.quantity = quantity;
    }
    
    // Constructor for non-egg items (tier defaults to "normal")
    public PurchaseData(ItemType itemType, int quantity = 1)
    {
        this.itemType = itemType.ToString();
        this.tier = "normal";
        this.quantity = quantity;
    }
}