using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutHandler : MonoBehaviour
{
    public void OnLogoutPressed()
    {
        AuthStorage.DeleteAccessToken();
        
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.DeleteKey("email");
        PlayerPrefs.DeleteKey("nickname");
        PlayerPrefs.Save();
        
        Debug.Log("Logged out successfully");
        
        SceneManager.LoadScene("Bootstrap");
    }
}