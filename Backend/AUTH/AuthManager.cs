using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class AuthManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config;
    
    [Header("References")]
    [SerializeField] private PlayerWallet playerWallet;

    private void Awake()
    {
        // Auto-find PlayerWallet if not assigned
        if (playerWallet == null)
            playerWallet = FindAnyObjectByType<PlayerWallet>();
    }

    // =====================
    // SIGNUP
    // =====================
    public void Signup(SignupData data, Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(SignupCoroutine(data, onSuccess, onError));
    }

    private IEnumerator SignupCoroutine(SignupData data, Action<string> onSuccess, Action<string> onError)
    {
        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(config.baseUrl + "/auth/signup", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error);
    }

    // =====================
    // LOGIN
    // =====================
    public void Login(LoginData data, Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(LoginCoroutine(data, onSuccess, onError));
    }

    private IEnumerator LoginCoroutine(LoginData data, Action<string> onSuccess, Action<string> onError)
    {
        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(config.baseUrl + "/auth/login", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error);
    }

    // =====================
    // GET ME (Fetch Current User - String Response)
    // =====================
    public void GetMe(Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(GetMeCoroutine(onSuccess, onError));
    }

    private IEnumerator GetMeCoroutine(Action<string> onSuccess, Action<string> onError)
    {
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(config.baseUrl + "/auth/me");
        
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error);
    }

    // =====================
    // UPDATE USER (PATCH /auth/update)
    // =====================
    public void UpdateUser(string jsonData, Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(UpdateUserCoroutine(jsonData, onSuccess, onError));
    }

    private IEnumerator UpdateUserCoroutine(string jsonData, Action<string> onSuccess, Action<string> onError)
    {
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        string url = config.baseUrl + "/auth/update";
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error);
    }

    // =====================
    // GET USER PROFILE (Typed Response)
    // =====================
    /// <summary>
    /// Refresh user profile from /auth/me with typed response
    /// Updates PlayerWallet automatically with latest backend data
    /// </summary>
    public void GetUserProfile(Action<UserProfile> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(GetUserProfileCoroutine(onSuccess, onError));
    }

    private IEnumerator GetUserProfileCoroutine(Action<UserProfile> onSuccess, Action<string> onError)
    {
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token");
            yield break;
        }

        string url = $"{config.baseUrl}/auth/me";
        
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string responseText = request.downloadHandler.text;
                
                UserProfile profile = JsonUtility.FromJson<UserProfile>(responseText);
                
                // Update PlayerWallet with fresh data
                if (playerWallet != null)
                {
                    playerWallet.SetUserFarms(profile.userFarms);
                    playerWallet.SetLevel(profile.level);
                    playerWallet.SetEggs(profile.eggs);
                    playerWallet.SetName(profile.username);
                    playerWallet.SetLocation(profile.location);
                    playerWallet.SetRanking(profile.ranking);
                    
                    if (!string.IsNullOrEmpty(profile.referralCode))
                    {
                        playerWallet.SetReferralCode(profile.referralCode);
                    }
                    
                }
                else
                {
                    Debug.LogWarning("⚠️ PlayerWallet not found, profile data not synced");
                }
                
                onSuccess?.Invoke(profile);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Parse error: {e.Message}");
            }
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }
}

// =====================
// DATA CLASSES
// =====================

[Serializable]
public class UserProfile
{
    public string username;
    public string email;
    public int level;
    public int eggs;
    public string location;
    public int ranking;
    public int userFarms;
    public string referralCode;
    // Add other fields as needed based on your backend response
}