using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutHandler : MonoBehaviour
{
    public void OnLogoutPressed()
    {
        // Remove access token
        AuthStorage.DeleteAccessToken();
        
        // Optional: Clear PlayerPrefs data
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.DeleteKey("email");
        PlayerPrefs.DeleteKey("nickname");
        PlayerPrefs.Save();
        
        Debug.Log("Logged out successfully");
        
        // Go to Bootstrap scene
        SceneManager.LoadScene("Bootstrap");
    }
}