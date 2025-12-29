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
    [SerializeField] private APIConfig config; 

    [Header("Exchange Rate")]
    [SerializeField] private int fpPerEgg = 400; 

    /// <summary>
    /// </summary>
    /// <param name="eggAmount">Number of eggs to exchange</param>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    public void ExchangeBasketToFP(int eggAmount, Action<BasketToFPResponse> onSuccess = null, Action<string> onError = null)
    {
        if (eggAmount <= 0)
        {
            onError?.Invoke("Invalid egg amount");
            
            if (infoErrorChanger != null)
                infoErrorChanger.OpenErrorDefault("Please enter a valid amount");
            
            return;
        }

        if (wallet != null)
        {
            if (wallet.Eggs < eggAmount)
            {
                string error = $"Not enough eggs! You have {wallet.Eggs} eggs but need {eggAmount}";
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

        int previousEggs = 0;
        int previousFP = 0;
        int fpToAdd = eggAmount * fpPerEgg;
        
        if (wallet != null)
        {
            previousEggs = wallet.Eggs;
            previousFP = wallet.FP;
            
            wallet.AddEggs(-eggAmount);
            wallet.Add(fpToAdd);
        }

        string url = config.baseUrl + "/economy/basket-to-fp";

        BasketToFPRequest requestBody = new BasketToFPRequest
        {
            eggs = eggAmount
        };
        string jsonData = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                try
                {
                    BasketToFPResponse response = JsonUtility.FromJson<BasketToFPResponse>(responseText);

                    if (infoErrorChanger != null)
                    {
                        infoErrorChanger.OpenErrorDefault($"Success! Exchanged {eggAmount} eggs for {fpToAdd:N0} FP");
                    }

                    onSuccess?.Invoke(response);
                }
                catch (Exception e)
                {
                    string error = $"Failed to parse response: {e.Message}";
                    onError?.Invoke(error);
                }
            }
            else
            {
                if (wallet != null)
                {
                    wallet.SetEggs(previousEggs);
                    wallet.SetFP(previousFP);
                }
                
                string error = $"Exchange failed: {request.error}";
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
    public bool success;
    public int fpAdded;
    public int eggsUsed;
    public string message;
}