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

    StoreDB.Item item;
    int unitPrice = 0;
    int qty = 1;
    bool purchasable = false;
    bool updatingFromCode = false;

    Coroutine tempMsgCo;

    void Awake()
    {
        // Handle typing
        if (qtyInput)
        {
            qtyInput.onValueChanged.AddListener(OnQtyTyped);
            qtyInput.onEndEdit.AddListener(OnQtyTyped);
        }
    }

    void OnEnable()
    {
        if (!store) store = StoreDB.Instance;
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        if (!marketManager) marketManager = FindAnyObjectByType<MarketManager>(FindObjectsInactive.Include);

        btnMinus?.onClick.RemoveAllListeners();
        btnPlus?.onClick.RemoveAllListeners();
        btnBuy?.onClick.RemoveAllListeners();
        btnCancel?.onClick.RemoveAllListeners();

        item = store ? store.Get(productId) : null;

        if (item == null)
        {
            ShowLocalizedError("Store_ItemNotFound");
            return;
        }

        unitPrice = Mathf.Max(0, item.priceFP);
        purchasable = item.canBuy && unitPrice > 0;

        // Localized price
        if (unitPriceText)
        {
            string priceFormat = LanguageManager.Instance.GetTranslation("Store_Price");
            unitPriceText.text = string.Format(priceFormat, unitPrice);
        }

        SetQty(Mathf.Clamp(1, minQty, maxQty));

        btnMinus?.onClick.AddListener(() => SetQty(qty - 1));
        btnPlus?.onClick.AddListener(() => SetQty(qty + 1));

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

        RefreshUI();
    }

    // ------------------------------
    // Quantity Handling
    // ------------------------------
    void OnQtyTyped(string s)
    {
        if (updatingFromCode) return;

        if (int.TryParse(s, out int v))
            SetQty(v);
        else
            RefreshUI();
    }

    void SetQty(int newQty)
    {
        qty = Mathf.Clamp(newQty, minQty, maxQty);

        updatingFromCode = true;
        if (qtyInput && qtyInput.text != qty.ToString())
            qtyInput.text = qty.ToString();
        updatingFromCode = false;

        RefreshUI();
    }

    // ------------------------------
    // UI Refresh with Localization
    // ------------------------------
        public void RefreshUI()
    {
        if (item == null) return;

        // ⭐⭐ FIX: Update localized price text
        if (unitPriceText)
        {
            string priceFormat = LanguageManager.Instance.GetTranslation("Store_Price");
            unitPriceText.text = string.Format(priceFormat, unitPrice);
        }

        // Continue existing code...
        long totalLong = (long)qty * unitPrice;
        int total = totalLong > int.MaxValue ? int.MaxValue : (int)totalLong;

        if (totalText)
        {
            string totalFormat = LanguageManager.Instance.GetTranslation("Store_Total");
            totalText.text = string.Format(totalFormat, total);
        }

        btnMinus.interactable = qty > minQty;
        btnPlus.interactable = qty < maxQty;

        bool canBuyNow;

        if (!purchasable)
        {
            canBuyNow = false;
            LocalizeMsg("Store_NotPurchasable", false);
        }
        else if (!wallet)
        {
            canBuyNow = false;
            LocalizeMsg("Store_NoWallet", false);
        }
        else
        {
            bool enough = wallet.Has(total);
            canBuyNow = enough;

            if (enough)
                LocalizeMsg("Store_ClickToBuy", true);
            else
                LocalizeMsg("Store_InsufficientFP", false);
        }

        btnBuy?.gameObject.SetActive(canBuyNow);
        btnCancel?.gameObject.SetActive(!canBuyNow);
    }

    // ------------------------------
    // Purchase Processing
    // ------------------------------
    void TryBuy()
    {
        if (wallet == null || item == null)
        {
            btnBuy.interactable = true;
            return;
        }

        long totalLong = (long)qty * unitPrice;
        int total = totalLong > int.MaxValue ? int.MaxValue : (int)totalLong;

        if (!wallet.Has(total))
        {
            LocalizeMsg("Store_InsufficientFP", false);
            btnBuy.interactable = true;
            RefreshUI();
            return;
        }

        if (marketManager == null)
        {
            Debug.LogError("MarketManager missing");
            btnBuy.interactable = true;
            return;
        }

        PurchaseData data = MapProduct(productId, qty);
        if (data == null)
        {
            Debug.LogError("Unknown product mapping");
            btnBuy.interactable = true;
            return;
        }

        LocalizeMsg("Store_Processing", false);

        marketManager.Purchase(
            data,
            onSuccess: (res) =>
            {
                wallet.TrySpend(total);
                wallet.AddItem(productId, qty);

                FlashMsg("Store_Purchased", new Color(0.2f, 0.6f, 1f), 3f);
                btnBuy.interactable = true;
            },
            onError: (err) =>
            {
                LocalizeMsg("Store_PurchaseFailed", false);
                btnBuy.interactable = true;
                RefreshUI();
            });
    }

    // ------------------------------
    // Localization Helpers
    // ------------------------------
    void LocalizeMsg(string key, bool good)
    {
        if (!messageText) return;

        string msg = LanguageManager.Instance.GetTranslation(key);
        messageText.text = msg;
        messageText.color = good
            ? new Color(0.16f, 0.6f, 0.2f)
            : new Color(0.85f, 0.2f, 0.2f);
    }

    void ShowLocalizedError(string key)
    {
        if (!messageText) return;

        messageText.text = LanguageManager.Instance.GetTranslation(key);
        messageText.color = new Color(0.85f, 0.2f, 0.2f);

        btnBuy?.gameObject.SetActive(false);
        btnCancel?.gameObject.SetActive(true);
    }

    IEnumerator FlashMsg(string key, Color col, float sec)
    {
        if (!messageText) yield break;

        string oldText = messageText.text;
        Color oldCol = messageText.color;

        messageText.text = LanguageManager.Instance.GetTranslation(key);
        messageText.color = col;

        yield return new WaitForSecondsRealtime(sec);

        messageText.text = oldText;
        messageText.color = oldCol;

        RefreshUI();
    }

    public void Close() { }

    // ------------------------------
    // Mapping
    // ------------------------------
    PurchaseData MapProduct(string id, int qty)
    {
        switch (id)
        {
            case "silver_egg": return new PurchaseData(ItemType.egg, EggTier.normal, qty);
            case "gold_egg": return new PurchaseData(ItemType.egg, EggTier.gold, qty);
            case "super_blue_egg": return new PurchaseData(ItemType.egg, EggTier.blue, qty);
            case "super_red_egg": return new PurchaseData(ItemType.egg, EggTier.red, qty);
            case "nest": return new PurchaseData(ItemType.nest, qty);
            case "food": return new PurchaseData(ItemType.food, qty);
            case "vitamin": return new PurchaseData(ItemType.vitamin, qty);
            case "battery": return new PurchaseData(ItemType.battery, qty);
            case "farmKey": return new PurchaseData(ItemType.farmKey, qty);
            case "robot": return new PurchaseData(ItemType.robot, qty);
            default: return null;
        }
    }
}
