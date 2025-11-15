using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class AuthManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config; // assign in Inspector

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

        // ✅ Add device ID header
        string deviceId = DeviceHelper.GetDeviceId();
        request.SetRequestHeader("x-device-id", deviceId);

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

        // ✅ Send persistent device ID
        string deviceId = DeviceHelper.GetDeviceId();
        request.SetRequestHeader("x-device-id", deviceId);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error);
    }

    // =====================
    // GET ME (Fetch Current User)
    // =====================
    public void GetMe(Action<string> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(GetMeCoroutine(onSuccess, onError));
    }

    private IEnumerator GetMeCoroutine(Action<string> onSuccess, Action<string> onError)
    {
        // Get the access token
        string accessToken = AuthStorage.GetAccessToken();
        
        if (string.IsNullOrEmpty(accessToken))
        {
            onError?.Invoke("No access token found");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(config.baseUrl + "/auth/me");
        
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