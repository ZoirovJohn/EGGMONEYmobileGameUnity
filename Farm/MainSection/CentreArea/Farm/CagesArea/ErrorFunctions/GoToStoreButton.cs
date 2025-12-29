using UnityEngine;
using UnityEngine.UI;

public class GoToStoreButton : MonoBehaviour
{
    [Header("Footer Store Button")]
    [SerializeField] Button footerStoreButton;
    
    [Header("Error Panel to Close")]
    [SerializeField] GameObject errorGoToStorePanel;
    
    [Header("Optional - Auto Find")]
    [SerializeField] bool autoFindPanels = true;

    void Awake()
    {
        if (autoFindPanels)
        {
            if (!footerStoreButton)
            {
                GameObject storeButtonObj = GameObject.Find("StoreButton");
                if (!storeButtonObj) storeButtonObj = GameObject.Find("Store Button");
                if (!storeButtonObj) storeButtonObj = GameObject.Find("FooterStoreButton");
                
                if (storeButtonObj)
                    footerStoreButton = storeButtonObj.GetComponent<Button>();
            }
            
            if (!errorGoToStorePanel)
            {
                errorGoToStorePanel = GameObject.Find("ErrorGoToStore");
                if (!errorGoToStorePanel) errorGoToStorePanel = GameObject.Find("errorGoStore");
                if (!errorGoToStorePanel) errorGoToStorePanel = GameObject.Find("ErrorGoStore");
            }
        }
    }

    public void OnGoToStoreClicked()
    {
        if (errorGoToStorePanel != null)
        {
            errorGoToStorePanel.SetActive(false);
        }
        
        if (footerStoreButton != null)
        {
            footerStoreButton.onClick.Invoke();
        }
        else
        {
            Debug.LogWarning("Footer Store Button not assigned!");
        }
    }
}