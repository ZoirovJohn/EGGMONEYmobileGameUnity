using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryLockInfo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InventoryCellId cellId;
    [SerializeField] PlayerWallet wallet;
    [SerializeField] InfoErrorChanger infoErrorChanger;
    [SerializeField] InventoryLockItemApplier lockItemApplier;
    [SerializeField] InventoryManager inventoryManager;

    [Header("UI Elements")]
    [SerializeField] TMP_Text errorMessageText;
    [SerializeField] TMP_Text infoMessageText;
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;

    [Header("Optional - Auto Find")]
    [SerializeField] bool autoFind = true;

    [Header("Loading State")]
    [SerializeField] GameObject loadingIndicator;

    void Awake()
    {
        if (!cellId)
            cellId = GetComponent<InventoryCellId>();

        if (autoFind)
        {
            if (!wallet) wallet = FindAnyObjectByType<PlayerWallet>();
            if (!infoErrorChanger) infoErrorChanger = FindAnyObjectByType<InfoErrorChanger>();
            if (!lockItemApplier) lockItemApplier = FindAnyObjectByType<InventoryLockItemApplier>();
            if (!inventoryManager) inventoryManager = FindAnyObjectByType<InventoryManager>();
        }

        Button btn = GetComponent<Button>();
        if (btn)
            btn.onClick.AddListener(OnLockCellClicked);
    }

    public void OnLockCellClicked()
    {
        if (!cellId || string.IsNullOrEmpty(cellId.productId))
        {
            Debug.LogWarning("Cell ID or product ID is missing!");
            return;
        }

        if (!wallet)
        {
            Debug.LogWarning("Wallet reference is missing!");
            return;
        }

        CheckKeyAvailability();
    }

    void CheckKeyAvailability()
    {
        string keyId = cellId.productId;
        int keyCount = wallet.GetItemCount(keyId);

        if (keyCount == 0)
        {
            if (inventoryManager != null)
            {
                ShowLoading(true);

                inventoryManager.GetInventory(
                    onSuccess: (response) =>
                    {
                        ShowLoading(false);
                        int updated = wallet.GetItemCount(keyId);

                        if (updated == 0)
                            ShowNoKeyError();
                        else
                            ShowKeyConfirmation(keyId);
                    },
                    onError: (err) =>
                    {
                        ShowLoading(false);
                        Debug.LogError(err);
                        ShowNoKeyError();
                    });
            }
            else
            {
                ShowNoKeyError();
            }
        }
        else
        {
            ShowKeyConfirmation(keyId);
        }
    }

    void ShowNoKeyError()
    {
        FindUIElements();

        if (errorMessageText)
        {
            string msg = LanguageManager.Instance.GetTranslation("ErrorNoKey");
            errorMessageText.text = msg;
        }

        if (infoErrorChanger != null)
        {
            infoErrorChanger.CloseAllInfoErrorMethod();
            infoErrorChanger.OpenErrorGoStore();
        }
    }

    void ShowKeyConfirmation(string keyId)
    {
        FindUIElements();

        if (infoErrorChanger != null)
            infoErrorChanger.CloseAllInfoErrorMethod();

        if (lockItemApplier != null)
            lockItemApplier.SetPendingItem(keyId);

        if (infoErrorChanger != null)
            infoErrorChanger.OpenInfoOpenFarm();

        StartCoroutine(SetMessageAfterPanelOpens());
    }

    System.Collections.IEnumerator SetMessageAfterPanelOpens()
    {
        yield return null;

        FindUIElements();

        if (infoMessageText)
        {
            string msg = LanguageManager.Instance.GetTranslation("ConfirmOpenNewFarm");
            infoMessageText.text = msg;
        }

        if (yesButton) yesButton.gameObject.SetActive(true);
        if (noButton) noButton.gameObject.SetActive(true);
    }

    void ShowLoading(bool show)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(show);
    }

    void FindUIElements()
    {
        if (errorMessageText == null)
        {
            GameObject panel = GameObject.Find("ErrorGoToStore") ??
                               GameObject.Find("errorGoStore") ??
                               GameObject.Find("Error");

            if (panel)
                errorMessageText = panel.GetComponentInChildren<TMP_Text>(true);
        }

        if (infoMessageText == null)
        {
            GameObject panel = GameObject.Find("InfoOpenFarm") ??
                               GameObject.Find("InfoPanel") ??
                               GameObject.Find("Info");

            if (panel)
            {
                TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 0)
                    infoMessageText = texts[0];
            }
        }

        if (yesButton == null)
        {
            GameObject obj = GameObject.Find("BtnYes") ??
                             GameObject.Find("YesButton") ??
                             GameObject.Find("Yes");
            if (obj) yesButton = obj.GetComponent<Button>();
        }

        if (noButton == null)
        {
            GameObject obj = GameObject.Find("BtnNo") ??
                             GameObject.Find("NoButton") ??
                             GameObject.Find("No");
            if (obj) noButton = obj.GetComponent<Button>();
        }
    }

    // LOCALIZED key display names
    string GetKeyDisplayName(string keyId)
    {
        string key = keyId switch
        {
            "key_farm" => "Key_Farm",
            "premiumfarmkey" => "Key_PremiumFarm",
            "premium_farm_key" => "Key_PremiumFarm",
            "premiumFarmKey" => "Key_PremiumFarm",

            "key_farm_4" => "Key_Farm_4",
            "farmKey4" => "Key_Farm_4",
            "farm_4_key" => "Key_Farm_4",

            "key_farm_5" => "Key_Farm_5",
            "farmKey5" => "Key_Farm_5",
            "farm_5_key" => "Key_Farm_5",

            "key_farm_6" => "Key_Farm_6",
            "farmKey6" => "Key_Farm_6",
            "farm_6_key" => "Key_Farm_6",

            "key_farm_7" => "Key_Farm_7",
            "farmKey7" => "Key_Farm_7",
            "farm_7_key" => "Key_Farm_7",

            "key_farm_8" => "Key_Farm_8",
            "farmKey8" => "Key_Farm_8",
            "farm_8_key" => "Key_Farm_8",

            _ => "Key_Farm"
        };

        return LanguageManager.Instance.GetTranslation(key);
    }
}
