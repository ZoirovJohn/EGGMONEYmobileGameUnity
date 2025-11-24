using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this script to each lock slot prefab to handle click events
/// Opens the InventoryLockBar when a lock slot is clicked
/// </summary>
public class LockSlotClick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryItemsBarChanger inventoryBarChanger;
    [SerializeField] private InfoErrorChanger infoErrorChanger;
    [SerializeField] private InventoryLockItemApplier lockItemApplier;
    [SerializeField] private FarmHeaderManager farmHeaderManager; // ✅ NEW
    
    [Header("UI References")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    
    [Header("Auto Find")]
    [SerializeField] private bool autoFind = true;
    
    private Button button;
    
    void Awake()
    {
        // Auto-find references if not assigned
        if (autoFind)
        {
            if (inventoryBarChanger == null)
            {
                inventoryBarChanger = FindAnyObjectByType<InventoryItemsBarChanger>();
            }
            
            if (infoErrorChanger == null)
            {
                infoErrorChanger = FindAnyObjectByType<InfoErrorChanger>();
            }
            
            if (lockItemApplier == null)
            {
                lockItemApplier = FindAnyObjectByType<InventoryLockItemApplier>();
            }
            
            // ✅ NEW: Auto-find FarmHeaderManager
            if (farmHeaderManager == null)
            {
                farmHeaderManager = FindAnyObjectByType<FarmHeaderManager>();
            }
        }
        
        // Get button component (check both root and children)
        button = GetComponent<Button>();
        if (button == null)
        {
            button = GetComponentInChildren<Button>();
        }
        
        // Hook up click listener
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnLockSlotClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ LockSlotClick: No Button component found on lock slot!");
        }
    }
    
    private void OnLockSlotClicked()
    {
        // ✅ NEW: Close all centre area panels except farm panel (same as farm slot behavior)
        if (farmHeaderManager != null)
        {
            farmHeaderManager.CloseAllCentreAreaPanels();
        }
        else
        {
            Debug.LogWarning("⚠️ FarmHeaderManager reference is missing! Panels may not close properly.");
        }
        
        // Find UI elements at click time (more reliable)
        FindUIElements();
        
        // Clear any pending key first
        if (lockItemApplier != null)
        {
            lockItemApplier.SetPendingItem("");
        }
        
        // Open the inventory lock bar
        if (inventoryBarChanger != null)
        {
            inventoryBarChanger.InventoryLockBarMethod();
        }
        else
        {
            Debug.LogError("❌ InventoryItemsBarChanger reference is missing!");
            return;
        }
        
        // Open the info panel
        if (infoErrorChanger != null)
        {
            infoErrorChanger.OpenInfoOpenFarm();
        }
        else
        {
            Debug.LogError("❌ InfoErrorChanger reference is missing!");
            return;
        }
        
        // Wait a frame to ensure panel is active, then set message
        StartCoroutine(SetInitialMessageAfterDelay());
    }
    
    private System.Collections.IEnumerator SetInitialMessageAfterDelay()
    {
        yield return null; // Wait one frame
        
        // Find UI elements again after panel opens
        FindUIElements();
        
        // Set the initial message text (before key selection)
        if (messageText != null)
        {
            messageText.text = "Would you want to open a new farm?\nPlease select farm key first!";
        }
        else
        {
            Debug.LogWarning("⚠️ Message text reference not found! Searching in scene...");
            
            // Try more aggressive search
            TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
            foreach (var txt in allTexts)
            {
                if (txt.gameObject.name.Contains("Info") || txt.gameObject.name.Contains("Message") || 
                    txt.transform.parent?.name.Contains("InfoOpenFarm") == true)
                {
                    messageText = txt;
                    messageText.text = "Would you want to open a new farm?\nPlease select farm key first!";
                    break;
                }
            }
        }
        
        // Hide Yes/No buttons initially (they should appear after key selection)
        if (yesButton != null)
        {
            yesButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ Yes button not found!");
        }
        
        if (noButton != null)
        {
            noButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ No button not found!");
        }
    }
    
    private void FindUIElements()
    {
        // Try to find message text
        if (messageText == null)
        {
            GameObject infoPanel = GameObject.Find("InfoOpenFarm");
            if (infoPanel == null) infoPanel = GameObject.Find("InfoPanel");
            if (infoPanel == null) infoPanel = GameObject.Find("Info");
            
            if (infoPanel != null)
            {
                // Look for text in children
                TMP_Text[] texts = infoPanel.GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 0)
                {
                    // Try to find the main message text (usually the largest or first one)
                    foreach (var txt in texts)
                    {
                        if (!txt.gameObject.name.ToLower().Contains("title") && 
                            !txt.gameObject.name.ToLower().Contains("button"))
                        {
                            messageText = txt;
                            break;
                        }
                    }
                    
                    // If still not found, just use the first one
                    if (messageText == null)
                    {
                        messageText = texts[0];
                    }
                }
            }
        }
        
        // Try to find Yes button
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
            }
        }
        
        // Try to find No button
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
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up listener when destroyed
        if (button != null)
        {
            button.onClick.RemoveListener(OnLockSlotClicked);
        }
    }
}