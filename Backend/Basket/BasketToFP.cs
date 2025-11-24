using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class BasketToFP : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private InfoErrorChanger infoErrorChanger;

    [Header("Config")]
    [SerializeField] private APIConfig config; // Assign in Inspector

    [Header("Exchange Rate")]
    [SerializeField] private int fpPerEgg = 400; // 1 egg = 400 FP

    /// <summary>
    /// Exchange eggs from basket to FP
    /// </summary>
    /// <param name="eggAmount">Number of eggs to exchange</param>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public void ExchangeBasketToFP(int eggAmount, Action<BasketToFPResponse> onSuccess = null, Action<string> onError = null)
    {
        if (eggAmount <= 0)
        {
            Debug.LogError("❌ Egg amount must be greater than 0");
            onError?.Invoke("Invalid egg amount");
            
            if (infoErrorChanger != null)
                infoErrorChanger.OpenErrorDefault("Please enter a valid amount");
            
            return;
        }

        // Check if player has enough eggs
        if (wallet != null)
        {
            if (wallet.Eggs < eggAmount)
            {
                string error = $"Not enough eggs! You have {wallet.Eggs} eggs but need {eggAmount}";
                Debug.LogError($"❌ {error}");
                onError?.Invoke(error);
                
                if (infoErrorChanger != null)
                    infoErrorChanger.OpenErrorDefault($"Not enough eggs! You have {wallet.Eggs}");
                
                return;
            }
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerWallet reference is missing! Cannot validate eggs.");
        }

        StartCoroutine(ExchangeBasketToFPCoroutine(eggAmount, onSuccess, onError));
    }

    private IEnumerator ExchangeBasketToFPCoroutine(int eggAmount, Action<BasketToFPResponse> onSuccess, Action<string> onError)
    {
        // Get the access token
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            string error = "No access token found. Please login again.";
            Debug.LogError($"❌ {error}");
            onError?.Invoke(error);
            
            if (infoErrorChanger != null)
                infoErrorChanger.OpenErrorDefault(error);
            
            yield break;
        }

        // ✅ UPDATE WALLET FIRST (Optimistic update)
        int previousEggs = 0;
        int previousFP = 0;
        int fpToAdd = eggAmount * fpPerEgg;
        
        if (wallet != null)
        {
            previousEggs = wallet.Eggs;
            previousFP = wallet.FP;
            
            // Update wallet immediately
            wallet.AddEggs(-eggAmount);
            wallet.Add(fpToAdd);
        }

        // Build the URL
        string url = config.baseUrl + "/economy/basket-to-fp";

        // Create request body
        BasketToFPRequest requestBody = new BasketToFPRequest
        {
            eggs = eggAmount
        };
        string jsonData = JsonUtility.ToJson(requestBody);

        // Create UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // Set headers
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            request.SetRequestHeader("Content-Type", "application/json");

            // Send request
            yield return request.SendWebRequest();

            // Handle response
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                try
                {
                    BasketToFPResponse response = JsonUtility.FromJson<BasketToFPResponse>(responseText);

                    // Show success message
                    if (infoErrorChanger != null)
                    {
                        infoErrorChanger.OpenErrorDefault($"Success! Exchanged {eggAmount} eggs for {fpToAdd:N0} FP");
                    }

                    // Trigger success callback
                    onSuccess?.Invoke(response);
                }
                catch (Exception e)
                {
                    string error = $"Failed to parse response: {e.Message}";
                    Debug.LogError($"❌ {error}");
                    
                    // Backend succeeded but we couldn't parse - don't rollback wallet
                    onError?.Invoke(error);
                }
            }
            else
            {
                // ❌ ROLLBACK WALLET ON FAILURE
                if (wallet != null)
                {
                    wallet.SetEggs(previousEggs);
                    wallet.SetFP(previousFP);
                    Debug.LogWarning($"⚠️ Rolled back wallet: {previousEggs} eggs, {previousFP} FP");
                }
                
                string error = $"Exchange failed: {request.error}";
                Debug.LogError($"❌ {error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                
                onError?.Invoke(request.error);
                
                if (infoErrorChanger != null)
                    infoErrorChanger.OpenErrorDefault("Exchange failed. Please try again.");
            }
        }
    }

    /// <summary>
    /// Quick exchange with default callbacks
    /// </summary>
    public void QuickExchange(int eggAmount)
    {
        ExchangeBasketToFP(
            eggAmount,
            onSuccess: (response) => {
                Debug.Log($"✅ Quick exchange completed successfully!");
            },
            onError: (error) => {
                Debug.LogError($"❌ Quick exchange failed: {error}");
            }
        );
    }

    // Example usage from a button or other script:
    public void ExchangeOneEgg()
    {
        QuickExchange(1);
    }

    public void ExchangeFiveEggs()
    {
        QuickExchange(5);
    }

    public void ExchangeTenEggs()
    {
        QuickExchange(10);
    }
}

// ============== Data Classes ==============

[System.Serializable]
public class BasketToFPRequest
{
    public int eggs;
}

[System.Serializable]
public class BasketToFPResponse
{
    // Add any fields that your backend returns
    // For example:
    public bool success;
    public int fpAdded;
    public int eggsUsed;
    public string message;
    
    // If your backend returns different fields, adjust accordingly
}