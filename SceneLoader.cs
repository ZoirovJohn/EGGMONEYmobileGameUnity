using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
  void Start()
  {
      if (AuthStorage.IsTokenValid())
      {
          // User is already logged in → open Farm
          LoadFarm();
      }
  }

  public void LoadFarm()
  {
    SceneManager.LoadScene("Farm");
  }
}
