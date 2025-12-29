using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class inventoryCageInfo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InventoryCellId cellId;
    [SerializeField] PlayerWallet wallet;
    [SerializeField] InfoErrorChanger infoErrorChanger;
    [SerializeField] InventoryManager inventoryManager;

    [Header("Error Message")]
    [SerializeField] TMP_Text errorMessageText;

    [Header("Optional - Auto Find")]
    [SerializeField] bool autoFind = true;

    [Header("Loading State")]
    [SerializeField] GameObject loadingIndicator;

    [Header("Cage Panels")]
    [SerializeField] GameObject bigCageInside1;
    [SerializeField] GameObject bigCageInside2;

    void Awake()
    {
        if (!cellId)
            cellId = GetComponent<InventoryCellId>();

        if (autoFind)
        {
            if (!wallet)
                wallet = FindAnyObjectByType<PlayerWallet>();

            if (!infoErrorChanger)
                infoErrorChanger = FindAnyObjectByType<InfoErrorChanger>();

            if (!inventoryManager)
                inventoryManager = FindAnyObjectByType<InventoryManager>();

            if (!errorMessageText)
            {
                GameObject errorPanel = GameObject.Find("ErrorGoToStore")
                                  ?? GameObject.Find("errorGoStore")
                                  ?? GameObject.Find("ErrorGoStore");

                if (errorPanel)
                    errorMessageText = errorPanel.GetComponentInChildren<TMP_Text>(true);
            }
        }

        Button btn = GetComponent<Button>();
        if (btn)
            btn.onClick.AddListener(inventoryCageInfoMethod);
    }

    public void inventoryCageInfoMethod()
    {
        if (!cellId || string.IsNullOrEmpty(cellId.productId))
        {
            return;
        }

        if (!wallet)
        {
            return;
        }

        CheckItemAvailability();
    }

    void CheckItemAvailability()
    {
        int itemCount = wallet.GetItemCount(cellId.productId);

        if (itemCount == 0)
        {
            if (inventoryManager != null)
            {
                ShowLoading(true);

                inventoryManager.GetInventory(
                    onSuccess: (response) =>
                    {
                        ShowLoading(false);

                        int updatedCount = wallet.GetItemCount(cellId.productId);

                        if (updatedCount == 0)
                            ShowNotEnoughItemsError();
                        else
                            ShowSetItemPanel();
                    },
                    onError: (err) =>
                    {
                        ShowLoading(false);
                        ShowNotEnoughItemsError();
                    }
                );
            }
            else
            {
                ShowNotEnoughItemsError();
            }
        }
        else
        {
            ShowSetItemPanel();
        }
    }

    void ShowNotEnoughItemsError()
    {
        string itemName = GetItemDisplayName(cellId.productId);

        if (errorMessageText)
        {
            string template = LanguageManager.Instance.GetTranslation("ItemNotEnough");
            errorMessageText.text = string.Format(template, itemName);
        }

        if (infoErrorChanger != null)
            infoErrorChanger.OpenErrorGoStore();
        else
            Debug.LogWarning("⚠️ InfoErrorChanger is not assigned!");
    }

    void ShowSetItemPanel()
    {
        bool bigCageInside2WasActive = bigCageInside2 != null && bigCageInside2.activeSelf;

        if (!bigCageInside2WasActive)
            CopyChildrenActiveStates(bigCageInside1, bigCageInside2);

        if (bigCageInside1 != null)
            bigCageInside1.SetActive(false);

        if (bigCageInside2 != null)
            bigCageInside2.SetActive(true);

        if (infoErrorChanger != null)
        {
            string itemName = GetItemDisplayName(cellId.productId);

            string template = LanguageManager.Instance.GetTranslation("HowManyToPutCage");
            string message = string.Format(template, itemName);

            infoErrorChanger.OpenInfoSetItemToCage(message);

            ManyToFarm manyToFarm = FindAnyObjectByType<ManyToFarm>();
            if (manyToFarm != null)
                manyToFarm.SetItem(cellId);
        }
    }

    void CopyChildrenActiveStates(GameObject source, GameObject target)
    {
        if (source == null || target == null) return;

        Transform[] sourceChildren = source.GetComponentsInChildren<Transform>(true);
        Transform[] targetChildren = target.GetComponentsInChildren<Transform>(true);

        foreach (Transform sourceChild in sourceChildren)
        {
            if (sourceChild == source.transform) continue;

            foreach (Transform targetChild in targetChildren)
            {
                if (targetChild == target.transform) continue;

                if (sourceChild.name == targetChild.name)
                {
                    targetChild.gameObject.SetActive(sourceChild.gameObject.activeSelf);
                    break;
                }
            }
        }
    }

    void ShowLoading(bool show)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(show);
    }

    string GetItemDisplayName(string productId)
    {
        string key = productId switch
        {
            "nest" => "Item_Nest",
            "silver_egg" => "Item_SilverEgg",
            "food" => "Item_Food",
            "gold_egg" => "Item_GoldEgg",
            "booster" => "Item_Vitamin",
            "vitamin" => "Item_Vitamin",
            "battery" => "Item_Battery",
            "robot" => "Item_Robot",
            "super_blue_egg" => "Item_SuperBlueEgg",
            "super_red_egg" => "Item_SuperRedEgg",
            "farmKey" => "Item_FarmKey",

            "white_chick" => "Item_WhiteChick",
            "champ_chick" => "Item_ChampChick",

            _ => null
        };

        if (key == null)
            return LanguageManager.Instance.currentLanguage == "Korean" 
                ? "이 아이템" 
                : "this item";

        return LanguageManager.Instance.GetTranslation(key);
    }


    public void RefreshInventoryAfterUse()
    {
        if (inventoryManager != null)
        {
            inventoryManager.GetInventory(
                onSuccess: (response) =>
                {
                    Debug.Log("✅ Inventory refreshed after item use");
                },
                onError: (err) =>
                {
                    Debug.LogError($"Failed to refresh inventory: {err}");
                }
            );
        }
    }
}
