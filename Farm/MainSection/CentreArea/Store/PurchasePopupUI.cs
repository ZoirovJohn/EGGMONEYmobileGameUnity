using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PurchasePopupUI : MonoBehaviour
{
    [Header("DB source")]
    [SerializeField] StoreDB store;        // drag StorePanel (with StoreDB)
    [SerializeField] string productId;     // nest, silver_egg, food, gold_egg, booster, battery, robot, super_blue_egg, super_red_egg

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

    [Header("Wallet (optional)")]
    [SerializeField] PlayerWallet wallet;

    // runtime
    StoreDB.Item item;
    int unitPrice = 0;
    int qty = 1;
    bool purchasable = false;

    void Awake()
    {
        if (btnMinus) btnMinus.onClick.AddListener(() => SetQty(qty - 1));
        if (btnPlus)  btnPlus .onClick.AddListener(() => SetQty(qty + 1));

        // Make Buy/Cancel do NOTHING on click
        if (btnBuy)    { btnBuy.onClick.RemoveAllListeners(); }
        if (btnCancel) { btnCancel.onClick.RemoveAllListeners(); }

        if (qtyInput)
        {
            qtyInput.onValueChanged.AddListener(OnQtyTyped); // live typing
            qtyInput.onEndEdit.AddListener(OnQtyTyped);      // commit on enter/focus loss
        }
    }

    void OnEnable()
    {
        if (!store)  store  = StoreDB.Instance;
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);

        item = store ? store.Get(productId) : null;
        if (item == null)
        {
            Debug.LogWarning($"[PurchasePopupUI] productId '{productId}' not found in StoreDB.");
            // no Close() call — leave UI as-is
            return;
        }

        unitPrice   = Mathf.Max(0, item.priceFP);
        purchasable = item.canBuy && unitPrice > 0;

        if (unitPriceText)
            unitPriceText.text = $"Price : {(unitPrice > 0 ? $"{unitPrice:N0} FP" : "None")}";

        // Default state when opening
        if (btnBuy)    btnBuy.gameObject.SetActive(true);
        if (btnCancel) btnCancel.gameObject.SetActive(false);

        SetQty(Mathf.Clamp(1, minQty, maxQty)); // affordability handled in RefreshUI()
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
        int total = qty * unitPrice;
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
            // Test mode (no wallet) → allow buy
            canBuyNow = true;
            if (messageText)
            {
                messageText.text  = "Test mode (no wallet).";
                messageText.color = new Color(0.4f, 0.4f, 0.4f);
            }
        }
        else
        {
            canBuyNow = wallet.Has(total);
            if (messageText)
            {
                if (canBuyNow)
                {
                    messageText.text  = $"You have {wallet.FP:N0} FP. You can buy this.";
                    messageText.color = new Color(0.16f, 0.6f, 0.2f);
                }
                else
                {
                    messageText.text  = "Insufficient balance.";
                    messageText.color = new Color(0.85f, 0.2f, 0.2f);
                }
            }
        }

        // +/- only respect min/max
        if (btnMinus) btnMinus.interactable = qty > minQty;
        if (btnPlus)  btnPlus .interactable = qty < maxQty;

        // Toggle which button is visible (buttons themselves do nothing)
        if (btnBuy)    btnBuy.gameObject.SetActive(canBuyNow);
        if (btnCancel) btnCancel.gameObject.SetActive(!canBuyNow);
    }

    // Intentionally a NO-OP so even if wired in Inspector, nothing happens.
    public void Close() { /* do nothing */ }
}
