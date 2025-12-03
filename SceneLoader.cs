using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        if (AuthStorage.IsTokenValid())
        {
            LoadFarm();
        }
    }

    public void LoadFarm()
    {
        SceneManager.LoadScene("Farm");
    }
}
