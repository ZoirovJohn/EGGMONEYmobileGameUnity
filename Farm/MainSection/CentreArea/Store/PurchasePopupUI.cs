using System.Collections;
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

    [Header("Wallet (required)")]
    [SerializeField] PlayerWallet wallet;

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
                btnBuy.interactable = true;
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
            canBuyNow = wallet.Has(total);
            if (messageText)
            {
                if (canBuyNow)
                {
                    messageText.text  = "You can buy this.";
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

        if (wallet.TrySpend(total))
        {
            // Add to inventory
            wallet.AddItem(productId, qty);

            // Show success for ~3s, then restore previous message
            if (tempMsgCo != null) { StopCoroutine(tempMsgCo); tempMsgCo = null; }
            tempMsgCo = StartCoroutine(FlashMessage(
                $"Purchased {qty}x {productId}!",
                new Color(0.2f, 0.6f, 1f),
                3f
            ));

            // keep popup open; previous message will be restored automatically
        }
        else
        {
            if (messageText)
            {
                messageText.text  = "Purchase failed. Not enough FP.";
                messageText.color = new Color(0.85f, 0.2f, 0.2f);
            }
            RefreshUI();
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
