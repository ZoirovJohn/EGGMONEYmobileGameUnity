using UnityEngine;

public class OpenUrlLink : MonoBehaviour
{
    [SerializeField] string url = "https://google.com";
    public void Open()
    {
        Debug.Log("[OpenUrlLink] Opening: " + url);
        Application.OpenURL(url);
    }
}
