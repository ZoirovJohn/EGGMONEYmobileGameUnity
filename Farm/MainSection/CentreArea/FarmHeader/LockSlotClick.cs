using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LockSlotClick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryItemsBarChanger inventoryBarChanger;
    [SerializeField] private InfoErrorChanger infoErrorChanger;
    [SerializeField] private InventoryLockItemApplier lockItemApplier;
    [SerializeField] private FarmHeaderManager farmHeaderManager;

    [Header("UI References")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Auto Find")]
    [SerializeField] private bool autoFind = true;

    private Button button;

    void Awake()
    {
        if (autoFind)
        {
            if (!inventoryBarChanger) inventoryBarChanger = FindAnyObjectByType<InventoryItemsBarChanger>();
            if (!infoErrorChanger) infoErrorChanger = FindAnyObjectByType<InfoErrorChanger>();
            if (!lockItemApplier) lockItemApplier = FindAnyObjectByType<InventoryLockItemApplier>();
            if (!farmHeaderManager) farmHeaderManager = FindAnyObjectByType<FarmHeaderManager>();
        }

        button = GetComponent<Button>() ?? GetComponentInChildren<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnLockSlotClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ LockSlotClick: No Button component found!");
        }
    }

    private void OnLockSlotClicked()
    {
        if (farmHeaderManager != null)
        {
            farmHeaderManager.CloseAllCentreAreaPanels();
        }

        FindUIElements();

        if (lockItemApplier != null)
            lockItemApplier.SetPendingItem("");

        if (inventoryBarChanger != null)
            inventoryBarChanger.InventoryLockBarMethod();
        else
        {
            return;
        }

        if (infoErrorChanger != null)
            infoErrorChanger.OpenInfoOpenFarm();
        else
        {
            return;
        }

        StartCoroutine(SetInitialMessageAfterDelay());
    }

    private System.Collections.IEnumerator SetInitialMessageAfterDelay()
    {
        yield return null;
        FindUIElements();

        if (messageText != null)
        {
            string text = LanguageManager.Instance.GetTranslation("SelectFarmKeyFirst");
            messageText.text = text;
        }

        if (yesButton != null) yesButton.gameObject.SetActive(false);
        if (noButton != null) noButton.gameObject.SetActive(false);
    }

    private void FindUIElements()
    {
        if (messageText == null)
        {
            GameObject panel =
                GameObject.Find("InfoOpenFarm") ??
                GameObject.Find("InfoPanel") ??
                GameObject.Find("Info");

            if (panel != null)
            {
                TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
                foreach (var txt in texts)
                {
                    if (!txt.gameObject.name.ToLower().Contains("title") &&
                        !txt.gameObject.name.ToLower().Contains("button"))
                    {
                        messageText = txt;
                        break;
                    }
                }
                if (messageText == null && texts.Length > 0)
                    messageText = texts[0];
            }
        }

        if (!yesButton)
        {
            GameObject yesObj =
                GameObject.Find("BtnYes") ??
                GameObject.Find("YesButton") ??
                GameObject.Find("ButtonYes");
            
            if (yesObj) yesButton = yesObj.GetComponent<Button>();
        }

        if (!noButton)
        {
            GameObject noObj =
                GameObject.Find("BtnNo") ??
                GameObject.Find("NoButton") ??
                GameObject.Find("ButtonNo");
            
            if (noObj) noButton = noObj.GetComponent<Button>();
        }
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnLockSlotClicked);
    }
}
