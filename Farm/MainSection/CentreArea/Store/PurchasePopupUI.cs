using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PurchasePopupUI : MonoBehaviour
{
    [Header("DB source")]
    [SerializeField] StoreDB store;        // drag StorePanel (with StoreDB)
    [SerializeField] string productId;     // nest, silver_egg, food, gold_egg, vitamin, battery, robot, super_blue_egg, super_red_egg, farmKey

    [Header("UI refs")]
    [SerializeField] TMP_Text unitPriceText;   // Window/PriceText
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

    // temp message coroutine handle
    Coroutine tempMsgCo;

    void Awake()
    {
        if (btnMinus) btnMinus.onClick.AddListener(() => SetQty(qty - 1));
        if (btnPlus)  btnPlus .onClick.AddListener(() => SetQty(qty + 1));

        // Ensure no prefab-wired actions linger
        if (btnBuy)    btnBuy.onClick.RemoveAllListeners();
        if (btnCancel) btnCancel.onClick.RemoveAllListeners();

        if (qtyInput)
        {
            qtyInput.onValueChanged.AddListener(OnQtyTyped); // live typing
            qtyInput.onEndEdit.AddListener(OnQtyTyped);      // commit on enter/focus loss
        }
    }

    void OnEnable()
    {
        // Resolve refs
        if (!store)  store  = StoreDB.Instance;
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        if (!marketManager) marketManager = FindAnyObjectByType<MarketManager>(FindObjectsInactive.Include);

        // Reset button listeners every time the popup opens
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

        // Compute purchasable state
        unitPrice   = Mathf.Max(0, item.priceFP);
        purchasable = item.canBuy && unitPrice > 0;

        // Price label
        if (unitPriceText)
            unitPriceText.text = $"Price : {(unitPrice > 0 ? $"{unitPrice:N0} FP" : "None")}";

        // Default buttons visible state before affordability check
        if (btnBuy)    btnBuy.gameObject.SetActive(true);
        if (btnCancel) btnCancel.gameObject.SetActive(false);

        // Set initial qty (affordability handled in RefreshUI)
        SetQty(Mathf.Clamp(1, minQty, maxQty));

        // Wire actions AFTER initial RefreshUI so state is correct
        if (btnBuy)
        {
            btnBuy.onClick.AddListener(() =>
            {
                if (!btnBuy.interactable) return; // simple debounce
                btnBuy.interactable = false;
                TryBuy();
            });
        }

        if (btnCancel)
            btnCancel.onClick.AddListener(Close);
    }

    void OnQtyTyped(string s)
    {
        if (int.TryParse(s, out var v)) SetQty(v);
        else RefreshUI();
    }

    void SetQty(int newQty)
    {
        qty = Mathf.Clamp(newQty, minQty, maxQty); // no affordability clamp
        if (qtyInput && qtyInput.text != qty.ToString())
            qtyInput.text = qty.ToString();

        RefreshUI();
    }

    void RefreshUI()
    {
        // overflow-safe total calc
        long totalL = (long)qty * unitPrice;
        int total = totalL > int.MaxValue ? int.MaxValue : (int)totalL;

        if (totalText) totalText.text = $"Total : {total:N0} FP";

        bool canBuyNow;

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
            // No wallet -> can't buy
            canBuyNow = false;
            if (messageText)
            {
                messageText.text  = "No wallet found.";
                messageText.color = new Color(0.85f, 0.2f, 0.2f);
            }
        }
        else
        {
            // ✅ Always allow purchase attempt - let server validate
            canBuyNow = true;
            
            if (messageText)
            {
                // Show local balance as reference, but don't block
                bool hasLocalBalance = wallet.Has(total);
                if (hasLocalBalance)
                {
                    messageText.text  = "Ready to purchase.";
                    messageText.color = new Color(0.16f, 0.6f, 0.2f);
                }
                else
                {
                    messageText.text  = "Low local balance - server will verify.";
                    messageText.color = new Color(0.8f, 0.6f, 0.2f); // Orange/warning color
                }
            }
        }

        // +/- only respect min/max
        if (btnMinus) btnMinus.interactable = qty > minQty;
        if (btnPlus)  btnPlus .interactable = qty < maxQty;

        // Toggle which button is visible
        if (btnBuy)    btnBuy.gameObject.SetActive(canBuyNow);
        if (btnCancel) btnCancel.gameObject.SetActive(!canBuyNow);
    }

    void TryBuy()
    {
        if (!wallet || item == null) return;

        // overflow-safe total calc
        long totalL = (long)qty * unitPrice;
        int total = totalL > int.MaxValue ? int.MaxValue : (int)totalL;

        // ✅ Don't check local balance - let server decide
        // The server has the authoritative balance

        // ✅ Call API first before local update
        if (marketManager != null)
        {
            PurchaseData purchaseData = MapProductToPurchaseData(productId, qty);
            
            if (purchaseData != null)
            {
                // Show "Processing..." message
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
                        
                        // ✅ Just deduct the price from current balance
                        if (wallet.TrySpend(total))
                        {
                            // Add items to local inventory
                            wallet.AddItem(productId, qty);

                            // Show success message
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
                            // Check if it's an insufficient balance error
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

    // ✅ Map productId to API format
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

        // Save current state
        string prevText = messageText.text;
        Color  prevCol  = messageText.color;

        // Show temporary message
        messageText.text  = text;
        messageText.color = color;

        yield return new WaitForSecondsRealtime(seconds);

        // Restore previous message
        if (messageText)
        {
            messageText.text  = prevText;
            messageText.color = prevCol;
        }

        // Re-evaluate UI in case wallet/qty changed during the toast
        RefreshUI();
    }

    // Intentionally a NO-OP so even if wired in Inspector, nothing happens.
    public void Close() { /* do nothing */ }
}