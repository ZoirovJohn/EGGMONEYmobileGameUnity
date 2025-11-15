using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class BasketManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config; // assign in Inspector

    // =====================
    // GET BASKET (Fetch Eggs in Basket)
    // =====================
    public void GetBasket(Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(GetBasketCoroutine(onSuccess, onError));
    }

    private IEnumerator GetBasketCoroutine(Action<string> onSuccess, Action<string> onError)
    {
        // Get the access token
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(config.baseUrl + "/basket");
        
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