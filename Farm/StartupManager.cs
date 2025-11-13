using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupManager : MonoBehaviour
{
    [Header("Scene names")]
    public string farmScene = "Farm";
    public string bootstrapScene = "Bootstrap";

    void Start()
    {
        if (AuthStorage.IsTokenValid())
        {
            // User is already logged in → open Farm
            SceneManager.LoadScene(farmScene);
        }
        else
        {
            // No valid token → go to Bootstrap/login
            SceneManager.LoadScene(bootstrapScene);
        }
    }
}
