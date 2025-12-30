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
    [SerializeField] Button btnPlus1;
    [SerializeField] Button btnPlus5;
    [SerializeField] Button btnPlus10;
    [SerializeField] Button btnBuy;
    [SerializeField] Button btnCancel;
    [SerializeField] GameObject inventoryButton;

    [Header("Limits")]
    [SerializeField] int minQty = 0;
    [SerializeField] int maxQty = 999;

    [Header("Wallet (required)")]
    [SerializeField] PlayerWallet wallet;

    [Header("Market Manager")]
    [SerializeField] MarketManager marketManager;

    [Header("Bounce Settings")]
    [SerializeField] float bounceHeight = 40f;
    [SerializeField] float bounceSpeed = 5f;
    [SerializeField] AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Popup Root")]
    [SerializeField] GameObject popupRoot;

    StoreDB.Item item;
    int unitPrice = 0;
    int qty = 1;
    bool purchasable = false;
    bool updatingFromCode = false;

    Coroutine tempMsgCo;
    Coroutine bounceCoroutine; 
    Vector2 inventoryOriginalPosition;
    bool originalPositionSet = false;

    void Awake()
    {
        // Handle typing
        if (qtyInput)
        {
            qtyInput.onValueChanged.AddListener(OnQtyTyped);
            qtyInput.onEndEdit.AddListener(OnQtyTyped);
        }
    }

    void Start()
    {
        if (inventoryButton != null && !originalPositionSet)
        {
            RectTransform rectTransform = inventoryButton.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                inventoryOriginalPosition = rectTransform.anchoredPosition;
                originalPositionSet = true;
            }
        }
    }

    void OnEnable()
    {
        if (!store) store = StoreDB.Instance;
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        if (!marketManager) marketManager = FindAnyObjectByType<MarketManager>(FindObjectsInactive.Include);
        if (inventoryButton != null && !originalPositionSet)
        {
            RectTransform rectTransform = inventoryButton.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                inventoryOriginalPosition = rectTransform.anchoredPosition;
                originalPositionSet = true;
            }
        }

        btnPlus1?.onClick.RemoveAllListeners();
        btnPlus5?.onClick.RemoveAllListeners();
        btnPlus10?.onClick.RemoveAllListeners();
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

        if (unitPriceText)
        {
            string priceFormat = LanguageManager.Instance.GetTranslation("Store_Price");
            unitPriceText.text = string.Format(priceFormat, unitPrice);
        }

        SetQty(Mathf.Clamp(0, minQty, maxQty));

        btnPlus1?.onClick.AddListener(() => SetQty(qty + 1));
        btnPlus5?.onClick.AddListener(() => SetQty(qty + 5));
        btnPlus10?.onClick.AddListener(() => SetQty(qty + 10));

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
        {
            btnCancel.onClick.AddListener(() =>
            {
                if (qty == 0)
                {
                    ClosePanel();
                }
                else
                {
                    SetQty(0);
                }
            });
        }

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
        if (unitPriceText)
        {
            string priceFormat = LanguageManager.Instance.GetTranslation("Store_Price");
            unitPriceText.text = string.Format(priceFormat, unitPrice);
        }

        long totalLong = (long)qty * unitPrice;
        int total = totalLong > int.MaxValue ? int.MaxValue : (int)totalLong;

        if (totalText)
        {
            string totalFormat = LanguageManager.Instance.GetTranslation("Store_Total");
            totalText.text = string.Format(totalFormat, total);
        }

        btnPlus1.interactable = qty < maxQty;
        btnPlus5.interactable = qty < maxQty;
        btnPlus10.interactable = qty < maxQty;

        bool canBuyNow;

        if (qty == 0)
        {
            canBuyNow = false;
            LocalizeMsg("Store_SelectQuantity", false); 
        }
        else if (!purchasable)
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

        if (btnBuy) btnBuy.interactable = canBuyNow;
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
                FXManager.Instance?.PlayPurchaseFX_Center();

                if (bounceCoroutine != null)
                {
                    StopCoroutine(bounceCoroutine);
                    
                    if (inventoryButton != null)
                    {
                        RectTransform rectTransform = inventoryButton.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.anchoredPosition = inventoryOriginalPosition;
                        }
                    }
                }

                if (inventoryButton != null)
                {
                    bounceCoroutine = StartCoroutine(BounceInventoryButton(3));
                }
                
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
    // Bounce Animation for Inventory Button
    // ------------------------------
    IEnumerator BounceInventoryButton(int bounceCount)
    {
        if (inventoryButton == null) yield break;

        RectTransform rectTransform = inventoryButton.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        rectTransform.anchoredPosition = inventoryOriginalPosition;

        for (int i = 0; i < bounceCount; i++)
        {
            float time = 0f;

            while (time < Mathf.PI)
            {
                if (rectTransform == null || inventoryButton == null)
                {
                    bounceCoroutine = null;
                    yield break;
                }

                time += Time.deltaTime * bounceSpeed;
                float bounce = Mathf.Max(0, Mathf.Sin(time)) * bounceHeight;
                float curveValue = bounceCurve.Evaluate(Mathf.Sin(time));
                bounce = bounce * curveValue;
                rectTransform.anchoredPosition = inventoryOriginalPosition + new Vector2(0, bounce);
                
                yield return null;
            }

            rectTransform.anchoredPosition = inventoryOriginalPosition;
            
            if (i < bounceCount - 1)
            {
                yield return new WaitForSeconds(0.15f);
            }
        }

        rectTransform.anchoredPosition = inventoryOriginalPosition;
        bounceCoroutine = null;
    }

    void OnDisable()
    {
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
            bounceCoroutine = null;
        }

        if (inventoryButton != null && originalPositionSet)
        {
            RectTransform rectTransform = inventoryButton.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = inventoryOriginalPosition;
            }
        }
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

        if (btnBuy) btnBuy.interactable = false;
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

    void ClosePanel()
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
        else
        {
            Debug.LogError("PurchasePopupUI: popupRoot not assigned");
        }
    }

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