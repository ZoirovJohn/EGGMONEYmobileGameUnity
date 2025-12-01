using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PurchasePopupUI : MonoBehaviour
{
    [Header("DB source")]
    [SerializeField] StoreDB store;
    [SerializeField] string productId;

    [Header("UI refs")]
    [SerializeField] TMP_Text unitPriceText;
    [SerializeField] TMP_InputField qtyInput;
    [SerializeField] TMP_Text totalText;
    [SerializeField] TMP_Text messageText;
    [SerializeField] Button btnMinus;
    [SerializeField] Button btnPlus;
    [SerializeField] Button btnBuy;
    [SerializeField] Button btnCancel;

    [Header("Limits")]
    [SerializeField] int minQty = 1;
    [SerializeField] int maxQty = 999;

    [Header("Wallet (required)")]
    [SerializeField] PlayerWallet wallet;

    [Header("Market Manager")]
    [SerializeField] MarketManager marketManager;

    // runtime
    StoreDB.Item item;
    int unitPrice = 0;
    int qty = 1;
    bool purchasable = false;
    bool updatingQtyFromCode = false; // ✅ Flag to prevent recursion

    Coroutine tempMsgCo;

    void Awake()
    {
        if (qtyInput)
        {
            qtyInput.onValueChanged.AddListener(OnQtyTyped);
            qtyInput.onEndEdit.AddListener(OnQtyTyped);
        }
    }

    void OnEnable()
    {
        if (!store)  store  = StoreDB.Instance;
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        if (!marketManager) marketManager = FindAnyObjectByType<MarketManager>(FindObjectsInactive.Include);

        // Clear ALL button listeners first
        if (btnMinus)
        {
            btnMinus.onClick.RemoveAllListeners();
            Debug.Log("➖ Minus button listener cleared and being set up");
        }
        
        if (btnPlus)
        {
            btnPlus.onClick.RemoveAllListeners();
            Debug.Log("➕ Plus button listener cleared and being set up");
        }

        if (btnBuy)    btnBuy.onClick.RemoveAllListeners();
        if (btnCancel) btnCancel.onClick.RemoveAllListeners();

        // Load item
        item = store ? store.Get(productId) : null;
        if (item == null)
        {
            Debug.LogWarning($"[PurchasePopupUI] productId '{productId}' not found in StoreDB.");

            if (btnBuy)    btnBuy.gameObject.SetActive(false);
            if (btnCancel) btnCancel.gameObject.SetActive(true);
            if (btnCancel) btnCancel.onClick.AddListener(Close);

            if (messageText)
            {
                messageText.text  = "Item not found.";
                messageText.color = new Color(0.85f, 0.2f, 0.2f);
            }
            return;
        }

        unitPrice   = Mathf.Max(0, item.priceFP);
        purchasable = item.canBuy && unitPrice > 0;

        if (unitPriceText)
            unitPriceText.text = $"Price : {(unitPrice > 0 ? $"{unitPrice:N0} FP" : "None")}";

        if (btnBuy)    btnBuy.gameObject.SetActive(true);
        if (btnCancel) btnCancel.gameObject.SetActive(false);

        SetQty(Mathf.Clamp(1, minQty, maxQty));

        // Set up button listeners AFTER initial quantity is set
        if (btnMinus)
        {
            btnMinus.onClick.AddListener(OnMinusClicked);
            Debug.Log("➖ Minus button listener added");
        }
        
        if (btnPlus)
        {
            btnPlus.onClick.AddListener(OnPlusClicked);
            Debug.Log("➕ Plus button listener added");
        }

        if (btnBuy)
        {
            btnBuy.onClick.AddListener(() =>
            {
                if (!btnBuy.interactable) return;
                btnBuy.interactable = false;
                TryBuy();
            });
        }

        if (btnCancel)
            btnCancel.onClick.AddListener(Close);
    }

    void OnQtyTyped(string s)
    {
        // ✅ Ignore if we're updating from code (button clicks)
        if (updatingQtyFromCode) return;

        if (int.TryParse(s, out var v)) SetQty(v);
        else RefreshUI();
    }

    void OnMinusClicked()
    {
        Debug.Log($"➖ MINUS BUTTON CLICKED! Current qty: {qty}, new will be: {qty - 1}");
        SetQty(qty - 1);
    }

    void OnPlusClicked()
    {
        Debug.Log($"➕ PLUS BUTTON CLICKED! Current qty: {qty}, new will be: {qty + 1}");
        SetQty(qty + 1);
    }

    void SetQty(int newQty)
    {
        qty = Mathf.Clamp(newQty, minQty, maxQty);
        
        // ✅ Set flag to prevent OnQtyTyped from triggering
        updatingQtyFromCode = true;
        
        if (qtyInput && qtyInput.text != qty.ToString())
            qtyInput.text = qty.ToString();
        
        updatingQtyFromCode = false;

        RefreshUI();
    }

    void RefreshUI()
    {
        long totalL = (long)qty * unitPrice;
        int total = totalL > int.MaxValue ? int.MaxValue : (int)totalL;

        if (totalText) totalText.text = $"Total : {total:N0} FP";

        bool canBuyNow;

        if (btnMinus)
        {
            bool minusEnabled = qty > minQty;
            btnMinus.interactable = minusEnabled;
            Debug.Log($"➖ Minus button interactable: {minusEnabled}, qty: {qty}, minQty: {minQty}");
        }
        
        if (btnPlus)
        {
            bool plusEnabled = qty < maxQty;
            btnPlus.interactable = plusEnabled;
            Debug.Log($"➕ Plus button interactable: {plusEnabled}, qty: {qty}, maxQty: {maxQty}");
        }

        if (!purchasable)
        {
            canBuyNow = false;
            if (messageText)
            {
                messageText.text  = "This item is not purchasable.";
                messageText.color = new Color(0.85f, 0.2f, 0.2f);
            }
        }
        else if (!wallet)
        {
            canBuyNow = false;
            if (messageText)
            {
                messageText.text  = "No wallet found.";
                messageText.color = new Color(0.85f, 0.2f, 0.2f);
            }
        }
        else
        {
            bool hasEnough = wallet.Has(total);
            canBuyNow = hasEnough;
            
            if (messageText)
            {
                if (hasEnough)
                {
                    messageText.text  = "Ready to purchase.";
                    messageText.color = new Color(0.16f, 0.6f, 0.2f);
                }
                else
                {
                    messageText.text  = "Insufficient FP balance.";
                    messageText.color = new Color(0.85f, 0.2f, 0.2f);
                }
            }
        }

        if (btnBuy)    btnBuy.gameObject.SetActive(canBuyNow);
        if (btnCancel) btnCancel.gameObject.SetActive(!canBuyNow);
    }

    void TryBuy()
    {
        if (!wallet || item == null)
        {
            if (btnBuy) btnBuy.interactable = true;
            return;
        }

        long totalL = (long)qty * unitPrice;
        int total = totalL > int.MaxValue ? int.MaxValue : (int)totalL;

        if (!wallet.Has(total))
        {
            Debug.LogWarning($"Insufficient balance: need {total} FP, have {wallet.FP} FP");
            
            if (messageText)
            {
                messageText.text  = "Insufficient FP balance.";
                messageText.color = new Color(0.85f, 0.2f, 0.2f);
            }
            
            if (btnBuy) btnBuy.interactable = true;
            RefreshUI();
            return;
        }

        if (marketManager != null)
        {
            PurchaseData purchaseData = MapProductToPurchaseData(productId, qty);
            
            if (purchaseData != null)
            {
                if (messageText)
                {
                    messageText.text  = "Processing purchase...";
                    messageText.color = new Color(0.6f, 0.6f, 0.6f);
                }

                marketManager.Purchase(
                    purchaseData,
                    onSuccess: (response) =>
                    {
                        Debug.Log("Purchase API successful: " + response);
                        
                        if (wallet.TrySpend(total))
                        {
                            wallet.AddItem(productId, qty);

                            if (tempMsgCo != null) { StopCoroutine(tempMsgCo); tempMsgCo = null; }
                            tempMsgCo = StartCoroutine(FlashMessage(
                                $"Purchased {qty}x {productId}!",
                                new Color(0.2f, 0.6f, 1f),
                                3f
                            ));
                        }
                        
                        if (btnBuy) btnBuy.interactable = true;
                    },
                    onError: (err) =>
                    {
                        Debug.LogError("Purchase API failed: " + err);
                        
                        if (messageText)
                        {
                            if (err.Contains("insufficient") || err.Contains("balance") || err.Contains("enough"))
                            {
                                messageText.text  = "Insufficient balance on server.";
                            }
                            else
                            {
                                messageText.text  = "Purchase failed. Please try again.";
                            }
                            messageText.color = new Color(0.85f, 0.2f, 0.2f);
                        }
                        
                        if (btnBuy) btnBuy.interactable = true;
                        RefreshUI();
                    }
                );
            }
            else
            {
                Debug.LogError($"Failed to map productId '{productId}' to PurchaseData");
                if (btnBuy) btnBuy.interactable = true;
            }
        }
        else
        {
            Debug.LogError("MarketManager not found!");
            if (btnBuy) btnBuy.interactable = true;
        }
    }

    PurchaseData MapProductToPurchaseData(string productId, int quantity)
    {
        switch (productId)
        {
            case "silver_egg":
                return new PurchaseData(ItemType.egg, EggTier.normal, quantity);
            case "gold_egg":
                return new PurchaseData(ItemType.egg, EggTier.gold, quantity);
            case "super_blue_egg":
                return new PurchaseData(ItemType.egg, EggTier.blue, quantity);
            case "super_red_egg":
                return new PurchaseData(ItemType.egg, EggTier.red, quantity);
            case "nest":
                return new PurchaseData(ItemType.nest, quantity);
            case "food":
                return new PurchaseData(ItemType.food, quantity);
            case "vitamin":
                return new PurchaseData(ItemType.vitamin, quantity);
            case "battery":
                return new PurchaseData(ItemType.battery, quantity);
            case "farmKey":
                return new PurchaseData(ItemType.farmKey, quantity);
            case "robot":
                return new PurchaseData(ItemType.robot, quantity);
            default:
                Debug.LogWarning($"Unknown productId: {productId}");
                return null;
        }
    }

    IEnumerator FlashMessage(string text, Color color, float seconds)
    {
        if (!messageText) yield break;

        string prevText = messageText.text;
        Color  prevCol  = messageText.color;

        messageText.text  = text;
        messageText.color = color;

        yield return new WaitForSecondsRealtime(seconds);

        if (messageText)
        {
            messageText.text  = prevText;
            messageText.color = prevCol;
        }

        RefreshUI();
    }

    public void Close() { /* do nothing */ }
}