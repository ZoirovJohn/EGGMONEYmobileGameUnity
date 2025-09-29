using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PurchasePopupUI : MonoBehaviour
{
    [Header("Config (per item)")]
    [SerializeField] int priceFP = 100;
    [SerializeField] int minQty = 1;
    [SerializeField] int maxQty = 99;

    [Header("UI refs")]
    [SerializeField] TMP_InputField qtyInput;   // <-- Input field
    [SerializeField] TMP_Text totalText;
    [SerializeField] TMP_Text messageText;
    [SerializeField] Button btnMinus;
    [SerializeField] Button btnPlus;
    [SerializeField] Button btnBuy;
    [SerializeField] Button btnCancel;

    [Header("Wallet")]
    [SerializeField] PlayerWallet wallet;

    int qty = 1;

    void Awake()
    {
        if (btnMinus) btnMinus.onClick.AddListener(() => SetQty(qty - 1));
        if (btnPlus)  btnPlus .onClick.AddListener(() => SetQty(qty + 1));
        if (btnBuy)   btnBuy  .onClick.AddListener(ConfirmBuy);
        if (btnCancel) btnCancel.onClick.AddListener(Close);

        if (qtyInput) qtyInput.onEndEdit.AddListener(OnInputChanged);
    }

    void OnEnable()
    {
        if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
        int maxAffordable = (priceFP <= 0) ? maxQty : Mathf.Max(1, wallet.FP / Mathf.Max(1, priceFP));
        SetQty(Mathf.Clamp(1, minQty, Mathf.Min(maxQty, maxAffordable)));
    }

    void SetQty(int newQty)
    {
        int maxAffordable = (priceFP <= 0) ? maxQty : wallet.FP / Mathf.Max(1, priceFP);
        qty = Mathf.Clamp(newQty, minQty, Mathf.Min(maxQty, Mathf.Max(minQty, maxAffordable)));

        if (qtyInput) qtyInput.text = qty.ToString();

        int total = qty * priceFP;
        if (totalText) totalText.text = $"Total : {total:N0} FP";

        bool canBuy = wallet && wallet.Has(total);

        if (messageText)
        {
            messageText.text = canBuy
                ? $"You have {wallet.FP:N0} FP. You can buy this."
                : $"You have {wallet.FP:N0} FP. Not enough funds.";
            messageText.color = canBuy ? new Color(0.16f, 0.6f, 0.2f) : new Color(0.85f, 0.2f, 0.2f);
        }

        if (btnMinus) btnMinus.interactable = qty > minQty;
        if (btnPlus)  btnPlus .interactable = qty < Mathf.Min(maxQty, Mathf.Max(minQty, maxAffordable));
        if (btnBuy)   btnBuy  .interactable = canBuy;
    }

    void OnInputChanged(string input)
    {
        if (int.TryParse(input, out int val))
            SetQty(val);
        else
            SetQty(qty); // revert to last valid
    }

    void ConfirmBuy()
    {
        int total = qty * priceFP;
        if (wallet != null && wallet.TrySpend(total))
        {
            // TODO: add to inventory
            Close();
        }
        else
        {
            SetQty(qty);
        }
    }

    public void Close() => gameObject.SetActive(false);
}
