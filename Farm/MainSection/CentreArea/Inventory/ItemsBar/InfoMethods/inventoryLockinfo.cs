using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryLockInfo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InventoryCellId cellId;
    [SerializeField] PlayerWallet wallet;
    [SerializeField] InfoErrorChanger infoErrorChanger;
    [SerializeField] InventoryLockItemApplier lockItemApplier;
    
    [Header("UI Elements")]
    [SerializeField] TMP_Text errorMessageText;
    [SerializeField] TMP_Text infoMessageText;
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;
    
    [Header("Optional - Auto Find")]
    [SerializeField] bool autoFind = true;

    void Awake()
    {
        if (!cellId)
            cellId = GetComponent<InventoryCellId>();
            
        if (autoFind)
        {
            if (!wallet)
                wallet = FindAnyObjectByType<PlayerWallet>();
            
            if (!infoErrorChanger)
                infoErrorChanger = FindAnyObjectByType<InfoErrorChanger>();
            
            // Auto find lockItemApplier
            if (!lockItemApplier)
                lockItemApplier = FindAnyObjectByType<InventoryLockItemApplier>();
        }
        
        // Hook up button click
        Button btn = GetComponent<Button>();
        if (btn)
        {
            btn.onClick.AddListener(OnLockCellClicked);
        }
    }

    public void OnLockCellClicked()
    {
        if (!cellId || string.IsNullOrEmpty(cellId.productId))
        {
            Debug.LogWarning("Cell ID or product ID is missing!");
            return;
        }
        
        if (!wallet)
        {
            Debug.LogWarning("Wallet reference is missing!");
            return;
        }
        
        // Find UI elements at click time
        FindUIElements();
        
        // Get the key ID from the inventory cell
        string keyId = cellId.productId;
        
        // Get key count from wallet
        int keyCount = wallet.GetItemCount(keyId);
        
        Debug.Log($"🔑 Key selected: {keyId} (Count: {keyCount})");
        
        if (keyCount == 0)
        {
            // Show error: not enough keys
            if (errorMessageText)
            {
                errorMessageText.text = "You don't have this key.\nPurchase it from the store.";
            }
            
            // Close current panel and open error panel
            if (infoErrorChanger != null)
            {
                infoErrorChanger.CloseAllInfoErrorMethod();
                infoErrorChanger.OpenErrorGoStore();
                Debug.Log("🚨 Opened ErrorGoStore - No key available");
            }
            else
            {
                Debug.LogWarning("⚠️ InfoErrorChanger is not assigned!");
            }
        }
        else
        {
            // Key count > 0, show confirmation with Yes/No buttons
            
            // Set the pending key
            if (lockItemApplier != null)
            {
                lockItemApplier.SetPendingItem(keyId);
                Debug.Log($"🔑 Set pending key: {keyId}");
            }
            else
            {
                Debug.LogWarning("⚠️ InventoryLockItemApplier is not assigned!");
                return;
            }
            
            // Update message to confirmation text (after key is selected)
            if (infoMessageText != null)
            {
                infoMessageText.text = "Would you want to open a new farm?";
                Debug.Log("📝 Updated message to confirmation (key selected)");
            }
            else
            {
                Debug.LogWarning("⚠️ Info message text not found! Searching...");
                FindUIElements();
                if (infoMessageText != null)
                {
                    infoMessageText.text = "Would you want to open a new farm?";
                    Debug.Log("📝 Updated message after search");
                }
            }
            
            // Show Yes/No buttons (after key selection)
            if (yesButton != null)
            {
                yesButton.gameObject.SetActive(true);
                Debug.Log("👁️ Showed Yes button (key selected)");
            }
            else
            {
                Debug.LogWarning("⚠️ Yes button reference not found!");
            }
            
            if (noButton != null)
            {
                noButton.gameObject.SetActive(true);
                Debug.Log("👁️ Showed No button (key selected)");
            }
            else
            {
                Debug.LogWarning("⚠️ No button reference not found!");
            }
            
            Debug.Log("✅ Ready for user confirmation");
        }
    }
    
    private void FindUIElements()
    {
        // Find error message text
        if (errorMessageText == null)
        {
            GameObject errorPanel = GameObject.Find("ErrorGoToStore");
            if (errorPanel == null) errorPanel = GameObject.Find("errorGoStore");
            if (errorPanel == null) errorPanel = GameObject.Find("ErrorGoStore");
            if (errorPanel == null) errorPanel = GameObject.Find("Error");
            
            if (errorPanel != null)
            {
                TMP_Text[] texts = errorPanel.GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 0)
                {
                    errorMessageText = texts[0];
                    Debug.Log($"✅ Found error text: {texts[0].gameObject.name}");
                }
            }
        }
        
        // Find info message text
        if (infoMessageText == null)
        {
            GameObject infoPanel = GameObject.Find("InfoOpenFarm");
            if (infoPanel == null) infoPanel = GameObject.Find("InfoPanel");
            if (infoPanel == null) infoPanel = GameObject.Find("Info");
            
            if (infoPanel != null)
            {
                TMP_Text[] texts = infoPanel.GetComponentsInChildren<TMP_Text>(true);
                foreach (var txt in texts)
                {
                    // Skip title texts and button texts
                    if (!txt.gameObject.name.ToLower().Contains("title") && 
                        !txt.gameObject.name.ToLower().Contains("button"))
                    {
                        infoMessageText = txt;
                        Debug.Log($"✅ Found info text: {txt.gameObject.name}");
                        break;
                    }
                }
                
                // Fallback to first text if nothing found
                if (infoMessageText == null && texts.Length > 0)
                {
                    infoMessageText = texts[0];
                    Debug.Log($"✅ Using first text: {texts[0].gameObject.name}");
                }
            }
        }
        
        // Find Yes/No buttons
        if (yesButton == null)
        {
            GameObject yesObj = GameObject.Find("BtnYes");
            if (yesObj == null) yesObj = GameObject.Find("YesButton");
            if (yesObj == null) yesObj = GameObject.Find("Yes");
            if (yesObj == null) yesObj = GameObject.Find("ButtonYes");
            if (yesObj == null) yesObj = GameObject.Find("Btn_Yes");
            
            if (yesObj != null)
            {
                yesButton = yesObj.GetComponent<Button>();
                Debug.Log($"✅ Found Yes button: {yesObj.name}");
            }
        }
        
        if (noButton == null)
        {
            GameObject noObj = GameObject.Find("BtnNo");
            if (noObj == null) noObj = GameObject.Find("NoButton");
            if (noObj == null) noObj = GameObject.Find("No");
            if (noObj == null) noObj = GameObject.Find("ButtonNo");
            if (noObj == null) noObj = GameObject.Find("Btn_No");
            
            if (noObj != null)
            {
                noButton = noObj.GetComponent<Button>();
                Debug.Log($"✅ Found No button: {noObj.name}");
            }
        }
    }
    
    // Helper method to get a friendly display name for the key
    string GetKeyDisplayName(string keyId)
    {
        // Handle generic farm keys
        if (keyId == "key_farm")
        {
            return "Farm Key";
        }
        
        // Handle premium farm key
        if (keyId == "premiumfarmkey" || keyId == "premium_farm_key" || keyId == "premiumFarmKey")
        {
            return "Premium Farm Key";
        }
        
        switch (keyId)
        {
            case "key_farm_4":
            case "farmKey4":
            case "farm_4_key":
                return "Farm 4 Key";
                
            case "key_farm_5":
            case "farmKey5":
            case "farm_5_key":
                return "Farm 5 Key";
                
            case "key_farm_6":
            case "farmKey6":
            case "farm_6_key":
                return "Farm 6 Key";
                
            case "key_farm_7":
            case "farmKey7":
            case "farm_7_key":
                return "Farm 7 Key";
                
            case "key_farm_8":
            case "farmKey8":
            case "farm_8_key":
                return "Farm 8 Key";
                
            default:
                return "Farm Key";
        }
    }
}