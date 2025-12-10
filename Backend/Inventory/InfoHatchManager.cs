using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoHatchManager : MonoBehaviour
{
    [Header("⭐ REQUIRED: Assign InfoHatch Panel")]
    [SerializeField] GameObject infoHatchPanel;

    [Header("UI Elements")]
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

        if (infoHatchPanel == null)
        {
            Debug.LogError("❌ InfoHatch Panel is NOT assigned!");
        }
        else if (infoHatchPanel.activeSelf)
        {
            infoHatchPanel.SetActive(false);
        }
    }

    // ⭐ Called when egg icon clicked
    public void InitializeHatchPanel(string eggType, int availableEggs)
    {
        if (infoHatchPanel == null)
        {
            Debug.LogError("❌ Cannot open InfoHatch - no panel assigned!");
            return;
        }

        this.eggType = eggType;
        this.availableEggs = availableEggs;
        this.currentQty = 1;

        SetupButtons();
        RefreshUI();

        infoHatchPanel.SetActive(true);
        infoHatchPanel.transform.SetAsLastSibling();
    }

    void SetupButtons()
    {
        btnMinus?.onClick.RemoveAllListeners();
        btnPlus?.onClick.RemoveAllListeners();
        btnCancel?.onClick.RemoveAllListeners();
        inputCount?.onEndEdit.RemoveAllListeners();

        btnMinus?.onClick.AddListener(() => SetQuantity(currentQty - 1));
        btnPlus?.onClick.AddListener(() => SetQuantity(currentQty + 1));
        btnCancel?.onClick.AddListener(OnCancelClicked);

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

    void SetQuantity(int newQty)
    {
        currentQty = Mathf.Clamp(newQty, minQty, Mathf.Min(maxQty, availableEggs));
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (inputCount)
            inputCount.text = currentQty.ToString();

        if (infoText)
        {
            string eggName = GetEggDisplayName(eggType);
            string format = LanguageManager.Instance.GetTranslation("Hatch_YouHave");
            infoText.text = string.Format(format, availableEggs, eggName);
        }

        bool canHatch = currentQty > 0 && currentQty <= availableEggs && availableEggs > 0;

        if (messageText)
        {
            if (canHatch)
            {
                messageText.text = LanguageManager.Instance.GetTranslation("Hatch_Available");
                messageText.color = new Color(0.16f, 0.6f, 0.2f);
            }
            else
            {
                messageText.text = LanguageManager.Instance.GetTranslation("Hatch_NotEnough");
                messageText.color = new Color(0.85f, 0.2f, 0.2f);
            }
        }

        if (btnOk)
        {
            btnOk.gameObject.SetActive(canHatch);
            btnOk.interactable = canHatch;
        }

        if (btnCancel)
            btnCancel.gameObject.SetActive(!canHatch);

        btnMinus.interactable = currentQty > minQty;
        btnPlus.interactable = currentQty < Mathf.Min(maxQty, availableEggs);
    }

    void OnCancelClicked()
    {
        infoHatchPanel?.SetActive(false);
    }

    public void OnHatchBtnClick()
    {
        if (currentQty <= 0 || currentQty > availableEggs)
        {
            Debug.LogWarning("⚠️ Invalid hatch quantity");
            return;
        }

        if (!hatchAPI)
        {
            hatchAPI = FindAnyObjectByType<EggHatchAPI>();
            if (!hatchAPI)
            {
                Debug.LogError("❌ No EggHatchAPI found!");
                return;
            }
        }

        if (!wallet)
        {
            Debug.LogError("❌ Missing PlayerWallet reference");
            return;
        }

        btnOk.interactable = false;

        hatchAPI.HatchEggs(
            eggType,
            currentQty,
            onSuccess: (response) =>
            {
                infoHatchPanel?.SetActive(false);
            },
            onError: (err) =>
            {
                Debug.LogError($"❌ Hatch failed: {err}");
                btnOk.interactable = true;
            }
        );
    }

    string GetEggDisplayName(string eggId)
    {
        string key = eggId switch
        {
            "silver_egg" => "Egg_Silver",
            "gold_egg" => "Egg_Gold",
            _ => "Egg_Generic"
        };

        return LanguageManager.Instance.GetTranslation(key);
    }

    // ⭐ CALLED BY LANGUAGE MANAGER
    public void RefreshLanguage()
    {
        if (infoHatchPanel != null && infoHatchPanel.activeSelf)
            RefreshUI();
    }
}
