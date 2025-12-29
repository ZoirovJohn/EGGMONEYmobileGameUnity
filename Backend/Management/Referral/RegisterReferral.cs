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
    public PlayerWallet playerWallet;

    [Header("Config")]
    public APIConfig config; 

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
            return;
        }

        if (playerWallet != null)
        {
            string userLocation = playerWallet.Location;
            
            if (userLocation == "KR")
            {
                ShowMessage("Not allowed, change location!");
                return;
            }
        }
        else
        {
            Debug.LogError("PlayerWallet is not assigned!");
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
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            ShowMessage("Please login first");
            yield break;
        }

        if (registerButton != null)
        {
            registerButton.interactable = false;
        }

        string jsonBody = JsonUtility.ToJson(new ReferralCodeRequest { referralCode = referralCode });

        using (UnityWebRequest request = new UnityWebRequest(config.baseUrl + "/referrals/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return request.SendWebRequest();

            if (registerButton != null)
            {
                registerButton.interactable = true;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    ReferralSuccessResponse success = JsonUtility.FromJson<ReferralSuccessResponse>(request.downloadHandler.text);
                    ShowMessage(success.message);
                }
                catch
                {
                    ShowMessage("Referral code registered successfully!");
                }
                
                if (referralCodeInput != null)
                {
                    referralCodeInput.text = "";
                }
            }
            else
            {
                try
                {
                    ReferralErrorResponse error = JsonUtility.FromJson<ReferralErrorResponse>(request.downloadHandler.text);
                    ShowMessage(error.message);
                }
                catch
                {
                    ShowMessage("Failed to register referral code");
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