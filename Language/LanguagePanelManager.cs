using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    [Header("UI References")]
    public Button langButton;
    public GameObject langPanel;
    public Button engBtn;
    public Button korBtn;
    public TMP_Text currentLanguageText;

    [Header("Current Language")]
    public string currentLanguage = "English";

    private Dictionary<string, Dictionary<string, string>> translations;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeTranslations();
            LoadLanguagePreference();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (langPanel != null) langPanel.SetActive(false);

        if (langButton != null) langButton.onClick.AddListener(TogglePanel);
        if (engBtn != null) engBtn.onClick.AddListener(() => SetLanguage("English"));
        if (korBtn != null) korBtn.onClick.AddListener(() => SetLanguage("Korean"));

        UpdateAllTexts();
        UpdateAllStoreUI();
        UpdateAllHatchPanels();
    }

    private void InitializeTranslations()
    {
        translations = TranslationsData.GetTranslations();
    }

    private void TogglePanel()
    {
        langPanel.SetActive(!langPanel.activeSelf);
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;
        SaveLanguagePreference();

        UpdateAllTexts();
        UpdateAllStoreUI();
        UpdateAllHatchPanels();
        UpdateAllHatchUI();
        UpdateAllPurchasePopups();
        UpdateAllFarmItemAppliers(); 
        UpdateAllEggExchangeManagers();

        var farmHeader = FindAnyObjectByType<FarmHeaderManager>();
        if (farmHeader != null)
        {
            farmHeader.RefreshFarmInfoLanguage();
        }

        langPanel.SetActive(false);
    }

    private void UpdateAllTexts()
    {
        if (currentLanguageText != null)
            currentLanguageText.text = currentLanguage == "English" ? "ENG" : "한국어";

        LocalizedText[] localizedTexts = Resources.FindObjectsOfTypeAll<LocalizedText>();

        foreach (LocalizedText text in localizedTexts)
        {
            if (text.gameObject.scene == SceneManager.GetActiveScene())
                text.UpdateText();
        }
    }

    private void UpdateAllEggExchangeManagers()
    {
        var exchangeManagers = FindObjectsByType<EggExchangeManager>(FindObjectsSortMode.None);
        foreach (var em in exchangeManagers)
            em.UpdateLocalizedTexts();
    }

    private void UpdateAllStoreUI()
    {
        var storeUIs = FindObjectsByType<PopulateStoreUIFromDB>(FindObjectsSortMode.None);
        foreach (var ui in storeUIs)
            ui.Apply();
    }

    private void UpdateAllFarmItemAppliers()
    {
        var appliers = FindObjectsByType<InventoryFarmItemApplier>(FindObjectsSortMode.None);
        foreach (var a in appliers)
            a.UpdateStatusImages();
    }

    private void UpdateAllHatchPanels()
    {
        var hatchManagers = FindObjectsByType<InfoHatchManager>(FindObjectsSortMode.None);
        foreach (var hm in hatchManagers)
            hm.RefreshLanguage();
    }

    private void UpdateAllHatchUI()
    {
        var hatchUIs = FindObjectsByType<InfoHatchManager>(FindObjectsSortMode.None);

        foreach (var ui in hatchUIs)
            ui.RefreshUI(); 
    }

    private void UpdateAllPurchasePopups()
    {
        var popups = FindObjectsByType<PurchasePopupUI>(FindObjectsSortMode.None);

        foreach (var popup in popups)
            popup.RefreshUI();
    }



    public string GetTranslation(string key)
    {
        if (translations.ContainsKey(key) &&
            translations[key].ContainsKey(currentLanguage))
        {
            return translations[key][currentLanguage];
        }
        return key;
    }

    private void SaveLanguagePreference()
    {
        PlayerPrefs.SetString("Language", currentLanguage);
        PlayerPrefs.Save();
    }

    private void LoadLanguagePreference()
    {
        currentLanguage = PlayerPrefs.GetString("Language", "English");
    }
}
