using UnityEngine;

[CreateAssetMenu(fileName = "APIConfig", menuName = "Config/APIConfig")]
public class APIConfig : ScriptableObject
{
    [Header("Backend URL")]
    public string baseUrl = "https://api.sparkgames.co.kr/";
}
