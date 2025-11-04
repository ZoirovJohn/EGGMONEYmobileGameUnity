using UnityEngine;

public class InfoErrorChanger : MonoBehaviour
{
    [Header("Info/Error Panel References")]
    [SerializeField] private GameObject errorGoStore;
    [SerializeField] private GameObject infoSetItemToFarm;
    [SerializeField] private GameObject errorDefault;
    [SerializeField] private GameObject infoOpenFarm;

    private void Start()
    {
        // Initialize - close all panels on start
        CloseAllInfoErrorMethod();
    }

    public void OpenErrorGoStore()
    {
        // Close all others
        CloseAllInfoErrorMethod();
        
        // Open ErrorGoStore
        if (errorGoStore != null)
        {
            errorGoStore.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ErrorGoStore is not assigned!");
        }
    }

    public void OpenInfoSetItemToFarm()
    {
        // Close all others
        CloseAllInfoErrorMethod();
        
        // Open InfoSetItemToFarm
        if (infoSetItemToFarm != null)
        {
            infoSetItemToFarm.SetActive(true);
        }
        else
        {
            Debug.LogWarning("InfoSetItemToFarm is not assigned!");
        }
    }

    public void OpenErrorDefault()
    {
        // Close all others
        CloseAllInfoErrorMethod();
        
        // Open ErrorDefault
        if (errorDefault != null)
        {
            errorDefault.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ErrorDefault is not assigned!");
        }
    }

    public void OpenInfoOpenFarm()
    {
        // Close all others
        CloseAllInfoErrorMethod();
        
        // Open InfoOpenFarm
        if (infoOpenFarm != null)
        {
            infoOpenFarm.SetActive(true);
        }
        else
        {
            Debug.LogWarning("InfoOpenFarm is not assigned!");
        }
    }

    public void CloseAllInfoErrorMethod()
    {
        if (errorGoStore != null)
            errorGoStore.SetActive(false);
            
        if (infoSetItemToFarm != null)
            infoSetItemToFarm.SetActive(false);
            
        if (errorDefault != null)
            errorDefault.SetActive(false);
            
        if (infoOpenFarm != null)
            infoOpenFarm.SetActive(false);
    }
}