using UnityEngine;
using TMPro;

public class InfoErrorChanger : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject errorGoStore;
    [SerializeField] private GameObject infoSetItemToFarm;
    [SerializeField] private GameObject errorDefault;
    [SerializeField] private GameObject infoOpenFarm;
    [SerializeField] private GameObject infoSetItemToCage;
    [SerializeField] private GameObject infoExchangeFPCoin;
    [SerializeField] private GameObject infoSetVitaminToFarm; // ✅ NEW

    [Header("Texts inside Panels")]
    [SerializeField] private TMP_Text errorGoStoreText;
    [SerializeField] private TMP_Text infoSetItemToFarmText;
    [SerializeField] private TMP_Text errorDefaultText;
    [SerializeField] private TMP_Text infoOpenFarmText;
    [SerializeField] private TMP_Text infoSetItemToCageText;
    [SerializeField] private TMP_Text infoExchangeFPCoinText;
    [SerializeField] private TMP_Text infoSetVitaminToFarmText; // ✅ NEW

    private void Start()
    {
        CloseAllInfoErrorMethod();
    }

    // ✅ Unified show function
    private void ShowPanel(GameObject panel, TMP_Text textUI, string message)
    {
        CloseAllInfoErrorMethod();

        if (panel != null)
            panel.SetActive(true);

        if (textUI != null && !string.IsNullOrEmpty(message))
            textUI.text = message;
    }

    // ✅ Public Open Methods
    public void OpenErrorGoStore(string message = null)
        => ShowPanel(errorGoStore, errorGoStoreText, message);

    public void OpenInfoSetItemToFarm(string message = null)
        => ShowPanel(infoSetItemToFarm, infoSetItemToFarmText, message);

    public void OpenErrorDefault(string message = null)
        => ShowPanel(errorDefault, errorDefaultText, message);

    public void OpenInfoOpenFarm(string message = null)
        => ShowPanel(infoOpenFarm, infoOpenFarmText, message);

    public void OpenInfoSetItemToCage(string message = null)
        => ShowPanel(infoSetItemToCage, infoSetItemToCageText, message);

    public void OpenInfoExchangeFPCoin(string message = null)
        => ShowPanel(infoExchangeFPCoin, infoExchangeFPCoinText, message);

    // ✅ NEW: Open Vitamin to Farm Info
    public void OpenInfoSetVitaminToFarm(string message = null)
        => ShowPanel(infoSetVitaminToFarm, infoSetVitaminToFarmText, message);

    // ✅ Close All
    public void CloseAllInfoErrorMethod()
    {
        if (errorGoStore != null) errorGoStore.SetActive(false);
        if (infoSetItemToFarm != null) infoSetItemToFarm.SetActive(false);
        if (errorDefault != null) errorDefault.SetActive(false);
        if (infoOpenFarm != null) infoOpenFarm.SetActive(false);
        if (infoSetItemToCage != null) infoSetItemToCage.SetActive(false);
        if (infoExchangeFPCoin != null) infoExchangeFPCoin.SetActive(false);
        if (infoSetVitaminToFarm != null) infoSetVitaminToFarm.SetActive(false); // ✅ NEW
    }
}