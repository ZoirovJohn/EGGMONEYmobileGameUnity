using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoHatchManager : MonoBehaviour
{
    [Header("⭐ REQUIRED: Assign InfoHatch Panel")]
    [SerializeField] GameObject infoHatchPanel;

    [Header("UI Elements")]
    [SerializeField] TMP_InputField inputCount;

    [Header("Add Buttons")]
    [SerializeField] Button btnAdd1;
    [SerializeField] Button btnAdd5;
    [SerializeField] Button btnAdd10;

    [SerializeField] Button btnOk;
    [SerializeField] Button btnCancel;

    [Header("References")]
    [SerializeField] PlayerWallet wallet;
    [SerializeField] EggHatchAPI hatchAPI;

    [Header("Limits")]
    [SerializeField] int maxQty = 999;

    [Header("Auto Find")]
    [SerializeField] bool autoFind = true;

    private int currentQty = 0;
    private int availableEggs = 0;
    private string eggType = "";

    void Awake()
    {
        if (autoFind)
        {
            if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>();
            if (!hatchAPI) hatchAPI = FindAnyObjectByType<EggHatchAPI>();
        }

        if (infoHatchPanel == null)
        {
            Debug.LogError("❌ InfoHatch Panel is NOT assigned!");
        }
        else if (infoHatchPanel.activeSelf)
        {
            infoHatchPanel.SetActive(false);
        }
    }

    public void InitializeHatchPanel(string eggType, int availableEggs)
    {
        if (infoHatchPanel == null)
        {
            return;
        }

        this.eggType = eggType;
        this.availableEggs = availableEggs;
        this.currentQty = 0;
        SetupButtons();
        RefreshUI(); 
        infoHatchPanel.SetActive(true);
        infoHatchPanel.transform.SetAsLastSibling();
    }

    void SetupButtons()
    {
        btnAdd1?.onClick.RemoveAllListeners();
        btnAdd5?.onClick.RemoveAllListeners();
        btnAdd10?.onClick.RemoveAllListeners();
        btnCancel?.onClick.RemoveAllListeners();
        inputCount?.onEndEdit.RemoveAllListeners();

        btnAdd1?.onClick.AddListener(() => AddQuantity(1));
        btnAdd5?.onClick.AddListener(() => AddQuantity(5));
        btnAdd10?.onClick.AddListener(() => AddQuantity(10));

        btnCancel?.onClick.AddListener(ResetQuantity);

        if (inputCount)
        {
            inputCount.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputCount.onEndEdit.AddListener((value) =>
            {
                if (int.TryParse(value, out int qty))
                    SetQuantity(qty);
                else
                    RefreshUI();
            });
        }
    }

    void AddQuantity(int amount)
    {
        SetQuantity(currentQty + amount);
    }

    void SetQuantity(int newQty)
    {
        currentQty = Mathf.Clamp(newQty, 0, Mathf.Min(maxQty, availableEggs));
        RefreshUI();
    }

    void ResetQuantity()
    {
        currentQty = 0;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (inputCount)
            inputCount.text = currentQty.ToString();

        bool canHatch = currentQty > 0 && currentQty <= availableEggs;

        if (btnOk)
            btnOk.interactable = canHatch;
    }

    public void OnHatchBtnClick()
    {
        if (currentQty <= 0 || currentQty > availableEggs)
        {
            return;
        }

        if (!hatchAPI)
        {
            hatchAPI = FindAnyObjectByType<EggHatchAPI>();
            if (!hatchAPI)
            {
                return;
            }
        }

        if (!wallet)
        {
            return;
        }

        btnOk.interactable = false;

        hatchAPI.HatchEggs(
            eggType,
            currentQty,
            onSuccess: (response) =>
            {
                FXManager.Instance?.PlayPurchaseFX_Center();
                
                infoHatchPanel?.SetActive(false);
            },
            onError: (err) =>
            {
                btnOk.interactable = true;
            }
        );
    }

    public void RefreshLanguage()
    {
        if (infoHatchPanel != null && infoHatchPanel.activeSelf)
            RefreshUI();
    }
}
