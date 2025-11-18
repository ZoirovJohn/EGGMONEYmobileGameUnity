using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class RegisterReferral : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField referralCodeInput;
    public Button registerButton;
    public TMP_Text infoMessage;

    [Header("Config")]
    public APIConfig config; // assign in Inspector

    private void Start()
    {
        if (registerButton != null)
        {
            registerButton.onClick.AddListener(OnRegisterButtonClicked);
        }
    }

    private void OnRegisterButtonClicked()
    {
        if (referralCodeInput == null)
        {
            Debug.LogError("Referral code input field is not assigned");
            return;
        }

        string code = referralCodeInput.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            ShowMessage("Please enter a referral code");
            return;
        }

        StartCoroutine(RegisterReferralCode(code));
    }

    private IEnumerator RegisterReferralCode(string referralCode)
    {
        // Get the access token
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            ShowMessage("Please login first");
            Debug.LogError("No access token found");
            yield break;
        }

        // Disable button during request
        if (registerButton != null)
        {
            registerButton.interactable = false;
        }

        // Create JSON body
        string jsonBody = JsonUtility.ToJson(new ReferralCodeRequest { referralCode = referralCode });

        // Create request
        using (UnityWebRequest request = new UnityWebRequest(config.baseUrl + "/referrals/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            // Send request
            yield return request.SendWebRequest();

            // Re-enable button
            if (registerButton != null)
            {
                registerButton.interactable = true;
            }

            // Handle response
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Referral code registered successfully: {referralCode}");
                
                // Try to parse success response
                try
                {
                    ReferralSuccessResponse success = JsonUtility.FromJson<ReferralSuccessResponse>(request.downloadHandler.text);
                    ShowMessage(success.message);
                }
                catch
                {
                    ShowMessage("Referral code registered successfully!");
                }
                
                // Clear input field on success
                if (referralCodeInput != null)
                {
                    referralCodeInput.text = "";
                }
            }
            else
            {
                // Parse error response
                try
                {
                    ReferralErrorResponse error = JsonUtility.FromJson<ReferralErrorResponse>(request.downloadHandler.text);
                    ShowMessage(error.message);
                    Debug.LogWarning($"❌ {error.message}");
                }
                catch
                {
                    ShowMessage("Failed to register referral code");
                    Debug.LogError($"Failed to register referral code: {request.error}");
                }
            }
        }
    }

    private void ShowMessage(string message)
    {
        if (infoMessage != null)
        {
            infoMessage.text = message;
        }
    }

    private void OnDestroy()
    {
        if (registerButton != null)
        {
            registerButton.onClick.RemoveListener(OnRegisterButtonClicked);
        }
    }

    [System.Serializable]
    private class ReferralCodeRequest
    {
        public string referralCode;
    }

    [System.Serializable]
    private class ReferralSuccessResponse
    {
        public string message;
    }

    [System.Serializable]
    private class ReferralErrorResponse
    {
        public string message;
        public string error;
        public int statusCode;
    }
}