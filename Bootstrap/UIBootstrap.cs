using UnityEngine;

public class UIBootstrap : MonoBehaviour
{
  public GameObject bgLogin;
  public GameObject panelLogin;

  void Start()
  {
    // When the scene starts, enable only the login background and panel
    if (bgLogin != null) bgLogin.SetActive(true);
    if (panelLogin != null) panelLogin.SetActive(true);
  }
}
