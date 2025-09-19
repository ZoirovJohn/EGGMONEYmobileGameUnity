using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadFarm()
    {
        SceneManager.LoadScene("Farm");
    }
}
