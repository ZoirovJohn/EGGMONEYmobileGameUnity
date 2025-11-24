using UnityEngine;

public class OpenUrlLink : MonoBehaviour
{
    [SerializeField] string url = "https://google.com";
    public void Open()
    {
        Application.OpenURL(url);
    }
}
