using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this script to a Manager GameObject (separate from the panel)
/// Then assign the InfoHatch panel in the Inspector
/// </summary>
public class InfoHatchManager : MonoBehaviour
{
    [Header("⭐ REQUIRED: Assign InfoHatch Panel")]
    [SerializeField] GameObject infoHatchPanel;
    
    [Header("UI Elements - Assign in Inspector")]
    [SerializeField] Button btnMinus;
    [SerializeField] Button btnPlus;
    [SerializeField] TMP_InputField inputCount;
    [SerializeField] TMP_Text infoText;
    [SerializeField] TMP_Text messageText;
    [SerializeField] Button btnOk;
    [SerializeField] Button btnCancel;
    
    [Header("References")]
    [SerializeField] PlayerWallet wallet;
    [SerializeField] EggHatchAPI hatchAPI;
    
    [Header("Limits")]
    [SerializeField] int minQty = 1;
    [SerializeField] int maxQty = 999;
    
    [Header("Auto Find")]
    [SerializeField] bool autoFind = true;

    // Runtime state
    private int currentQty = 1;
    private int availableEggs = 0;
    private string eggType = "";

    void Awake()
    {
        if (autoFind)
        {
            if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>();
            if (!hatchAPI) hatchAPI = FindAnyObjectByType<EggHatchAPI>();
        }
        
        // Validation
        if (infoHatchPanel == null)
        {
            Debug.LogError("❌❌❌ InfoHatch Panel is NOT assigned! Please assign it in the Inspector!");
        }
        else
        {
            // Make sure it starts disabled
            if (infoHatchPanel.activeSelf)
            {
                Debug.LogWarning($"⚠️ InfoHatch panel is active on start - disabling it now");
                infoHatchPanel.SetActive(false);
            }
        }
    }

    public void InitializeHatchPanel(string eggType, int availableEggs)
    {
        if (infoHatchPanel == null)
        {
            Debug.LogError("❌ Cannot open InfoHatch - panel not assigned!");
            return;
        }
        
        this.eggType = eggType;
        this.availableEggs = availableEggs;
        this.currentQty = 1;
        
        SetupButtons();
        RefreshUI();
        
        // Show the panel
        infoHatchPanel.SetActive(true);
        
        // Bring to front in UI hierarchy
        infoHatchPanel.transform.SetAsLastSibling();
        
    }

    void SetupButtons()
    {
        if (btnMinus) btnMinus.onClick.RemoveAllListeners();
        if (btnPlus) btnPlus.onClick.RemoveAllListeners();
        if (btnCancel) btnCancel.onClick.RemoveAllListeners();
        if (inputCount) inputCount.onEndEdit.RemoveAllListeners();
        
        if (btnMinus) btnMinus.onClick.AddListener(() => SetQuantity(currentQty - 1));
        if (btnPlus) btnPlus.onClick.AddListener(() => SetQuantity(currentQty + 1));
        if (btnCancel) btnCancel.onClick.AddListener(OnCancelClicked);
        
        if (inputCount)
        {
            inputCount.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputCount.onEndEdit.AddListener((value) => 
            {
                if (int.TryParse(value, out int qty))
                {
                    SetQuantity(qty);
                }
                else
                {
                    RefreshUI();
                }
            });
        }
    }

    void SetQuantity(int newQty)
    {
        currentQty = Mathf.Clamp(newQty, minQty, Mathf.Min(maxQty, availableEggs));
        RefreshUI();
    }

    void RefreshUI()
    {
        if (inputCount)
        {
            inputCount.text = currentQty.ToString();
        }
        
        if (infoText)
        {
            string eggName = GetEggDisplayName(eggType);
            infoText.text = $"You have: {availableEggs} {eggName}";
        }
        
        bool canHatch = currentQty > 0 && currentQty <= availableEggs && availableEggs > 0;
        
        if (messageText)
        {
            if (canHatch)
            {
                messageText.text = "Available for hatch";
                messageText.color = new Color(0.16f, 0.6f, 0.2f);
            }
            else
            {
                messageText.text = "Don't have enough eggs";
                messageText.color = new Color(0.85f, 0.2f, 0.2f);
            }
        }
        
        if (btnOk)
        {
            btnOk.gameObject.SetActive(canHatch);
            btnOk.interactable = canHatch;
        }
        
        if (btnCancel)
        {
            btnCancel.gameObject.SetActive(!canHatch);
        }
        
        if (btnMinus) btnMinus.interactable = currentQty > minQty;
        if (btnPlus) btnPlus.interactable = currentQty < Mathf.Min(maxQty, availableEggs);
    }

    void OnCancelClicked()
    {
        if (infoHatchPanel != null)
        {
            infoHatchPanel.SetActive(false);
        }
    }

    /// <summary>
    /// PUBLIC METHOD - Connect this to HatchBtn's OnClick in Inspector
    /// </summary>
    public void OnHatchBtnClick()
    {
        if (currentQty <= 0 || currentQty > availableEggs)
        {
            Debug.LogWarning("⚠️ Cannot hatch: invalid quantity");
            return;
        }
        
        if (!hatchAPI)
        {
            Debug.LogError("❌ EggHatchAPI reference is missing!");
            hatchAPI = FindAnyObjectByType<EggHatchAPI>();
            if (!hatchAPI)
            {
                Debug.LogError("❌ Still couldn't find EggHatchAPI!");
                return;
            }
        }
        
        if (!wallet)
        {
            Debug.LogError("❌ PlayerWallet reference is missing!");
            return;
        }
        
        if (btnOk) btnOk.interactable = false;
        
        hatchAPI.HatchEggs(
            eggType: eggType,
            quantity: currentQty,
            onSuccess: (response) =>
            {
                int newChickCount = wallet.GetItemCount("chick");
                int newEggCount = wallet.GetItemCount(eggType);
                
                // Close panel
                if (infoHatchPanel != null)
                {
                    infoHatchPanel.SetActive(false);
                }
            },
            onError: (errorMsg) =>
            {
                Debug.LogError($"❌ Hatching failed: {errorMsg}");
                if (btnOk) btnOk.interactable = true;
            }
        );
    }

    string GetEggDisplayName(string eggId)
    {
        switch (eggId)
        {
            case "silver_egg": return "Silver Eggs";
            case "gold_egg": return "Gold Eggs";
            default: return "eggs";
        }
    }
}