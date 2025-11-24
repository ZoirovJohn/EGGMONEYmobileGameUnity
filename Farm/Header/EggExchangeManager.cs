using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

public class EggExchangeManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config;

    [Header("References")]
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private InfoErrorChanger infoErrorChanger;
    [SerializeField] private BasketManager basketManager;
    [SerializeField] private Button topButton; // The button that opens/closes this panel

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField eggInputField;
    [SerializeField] private Button btnLeft;
    [SerializeField] private Button btnRight;
    [SerializeField] private TMP_Text txtFP; // Shows calculated FP amount
    [SerializeField] private Button btnYes;
    [SerializeField] private Button btnNo;

    [Header("Exchange Settings")]
    [SerializeField] private int fpPerEgg = 400; // 1 egg = 400 FP

    private int availableEggs = 0;
    private int currentInputAmount = 0;
    private bool isPanelOpen = false;

    private void Start()
    {
        // Setup button listeners
        if (btnLeft != null)
            btnLeft.onClick.AddListener(OnLeftButtonClicked);
        
        if (btnRight != null)
            btnRight.onClick.AddListener(OnRightButtonClicked);
        
        if (btnYes != null)
            btnYes.onClick.AddListener(OnYesButtonClicked);
        
        if (btnNo != null)
            btnNo.onClick.AddListener(OnNoButtonClicked);
        
        if (eggInputField != null)
        {
            eggInputField.onValueChanged.AddListener(OnInputValueChanged);
            eggInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        }

        // Setup top button listener
        if (topButton != null)
        {
            topButton.onClick.AddListener(OnTopButtonClicked);
        }
    }

    private void OnTopButtonClicked()
    {
        if (isPanelOpen)
        {
            CloseExchangePanel();
        }
        else
        {
            OpenExchangePanel();
        }
    }

    private void OpenExchangePanel()
    {
        isPanelOpen = true;
        
        // Fetch available eggs from backend using BasketManager
        if (basketManager != null)
        {
            basketManager.GetBasket(
                onSuccess: OnBasketFetchSuccess,
                onError: OnBasketFetchError
            );
        }
        else
        {
            Debug.LogError("❌ BasketManager reference is missing!");
            if (infoErrorChanger != null)
                infoErrorChanger.OpenErrorDefault("System error: BasketManager not found");
            isPanelOpen = false;
        }
    }

    private void OnBasketFetchSuccess(string jsonResponse)
    {
        try
        {
            BasketResponse response = JsonUtility.FromJson<BasketResponse>(jsonResponse);
            availableEggs = response.eggCount;
            
            // Open the panel with the data
            if (infoErrorChanger != null)
                infoErrorChanger.OpenInfoExchangeFPCoin();
            
            ResetInput();
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to parse basket response: {e.Message}");
            OnBasketFetchError("Invalid response from server");
        }
    }

    private void OnBasketFetchError(string error)
    {
        Debug.LogError($"❌ Failed to fetch basket: {error}");
        
        if (infoErrorChanger != null)
            infoErrorChanger.OpenErrorDefault("Failed to load egg data. Please try again.");
        
        isPanelOpen = false;
    }

    private void CloseExchangePanel()
    {
        isPanelOpen = false;
        
        if (infoErrorChanger != null)
            infoErrorChanger.CloseAllInfoErrorMethod();
        
        ResetInput();
    }

    private void OnLeftButtonClicked()
    {
        if (currentInputAmount > 0)
        {
            currentInputAmount--;
            UpdateInputDisplay();
        }
    }

    private void OnRightButtonClicked()
    {
        if (currentInputAmount < availableEggs)
        {
            currentInputAmount++;
            UpdateInputDisplay();
        }
        else
        {
            // Optional: Show a message when user tries to go beyond available eggs
            if (infoErrorChanger != null)
                infoErrorChanger.OpenErrorDefault($"Maximum eggs available: {availableEggs}");
        }
    }

    private void OnInputValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            currentInputAmount = 0;
            UpdateFPDisplay();
            return;
        }

        if (int.TryParse(value, out int inputValue))
        {
            // Clamp to available eggs
            int clampedValue = Mathf.Clamp(inputValue, 0, availableEggs);
            
            if (clampedValue != inputValue)
            {
                // User tried to enter more than available
                if (infoErrorChanger != null)
                    infoErrorChanger.OpenErrorDefault($"You only have {availableEggs} eggs available");
            }
            
            currentInputAmount = clampedValue;
            
            // Update display if clamped
            if (currentInputAmount != inputValue)
            {
                UpdateInputDisplay();
            }
            else
            {
                UpdateFPDisplay();
            }
        }
    }

    private void UpdateInputDisplay()
    {
        if (eggInputField != null)
            eggInputField.text = currentInputAmount.ToString();
        
        UpdateFPDisplay();
    }

    private void UpdateFPDisplay()
    {
        if (txtFP != null)
        {
            long totalFP = (long)currentInputAmount * fpPerEgg;
            txtFP.text = totalFP.ToString("N0"); // Format with thousand separators
        }
    }

    private void ResetInput()
    {
        currentInputAmount = 0;
        UpdateInputDisplay();
    }

    private void OnYesButtonClicked()
    {
        if (currentInputAmount <= 0)
        {
            if (infoErrorChanger != null)
                infoErrorChanger.OpenErrorDefault("Please enter amount of eggs to exchange");
            return;
        }

        if (currentInputAmount > availableEggs)
        {
            if (infoErrorChanger != null)
                infoErrorChanger.OpenErrorDefault($"Not enough eggs! You have {availableEggs} eggs");
            return;
        }

        // ✅ IMMEDIATE IN-GAME EXCHANGE (Optimistic Update)
        int eggsToExchange = currentInputAmount;
        int fpToAdd = eggsToExchange * fpPerEgg;

        // Update wallet immediately in game
        if (wallet != null)
        {
            wallet.TrySpendEggs(eggsToExchange);  // ✅ Use this instead
            wallet.Add(fpToAdd);
        }

        // Update available eggs
        availableEggs -= eggsToExchange;

        // Show success message
        if (infoErrorChanger != null)
            infoErrorChanger.OpenErrorDefault($"Success! Exchanged {eggsToExchange} eggs for {fpToAdd:N0} FP");

        // Reset and close
        ResetInput();
        StartCoroutine(ClosePanelAfterDelay(2f));

        // ✅ NOW CALL BACKEND to sync
        StartCoroutine(SyncExchangeWithBackend(eggsToExchange));
    }

    private void OnNoButtonClicked()
    {
        CloseExchangePanel();
    }

    private IEnumerator ClosePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseExchangePanel();
    }

    // ✅ Backend sync - happens AFTER in-game exchange
    private IEnumerator SyncExchangeWithBackend(int eggAmount)
    {
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("❌ No access token - backend sync failed (but in-game exchange already happened)");
            yield break;
        }

        string url = config.baseUrl + "/economy/basket-to-fp";
        
        BasketToFPRequest requestData = new BasketToFPRequest
        {
            eggs = eggAmount
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityEngine.Networking.UnityWebRequest request = 
               new UnityEngine.Networking.UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Backend sync successful: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ Backend sync failed: {request.error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                // Note: In-game exchange already happened, so we don't rollback here
                // You may want to implement a retry mechanism or queue system
            }
        }
    }

    private void OnDestroy()
    {
        // Cleanup listeners
        if (btnLeft != null)
            btnLeft.onClick.RemoveListener(OnLeftButtonClicked);
        
        if (btnRight != null)
            btnRight.onClick.RemoveListener(OnRightButtonClicked);
        
        if (btnYes != null)
            btnYes.onClick.RemoveListener(OnYesButtonClicked);
        
        if (btnNo != null)
            btnNo.onClick.RemoveListener(OnNoButtonClicked);
        
        if (eggInputField != null)
            eggInputField.onValueChanged.RemoveListener(OnInputValueChanged);
        
        if (topButton != null)
        {
            topButton.onClick.RemoveListener(OnTopButtonClicked);
        }
    }

    [Serializable]
    public class BasketResponse
    {
        public int eggCount;
    }

}
