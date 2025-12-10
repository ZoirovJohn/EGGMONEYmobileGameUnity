using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    // All translations
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
        UpdateAllStoreUI(); // ⭐ Run once at start
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
        UpdateAllStoreUI(); // ⭐ force-update store UI

        langPanel.SetActive(false);
        Debug.Log("Language changed to: " + language);
    }

    private void UpdateAllTexts()
    {
        if (currentLanguageText != null)
            currentLanguageText.text = currentLanguage == "English" ? "ENG" : "한국어";

        // Update LocalizedText components
        LocalizedText[] localizedTexts = Resources.FindObjectsOfTypeAll<LocalizedText>();

        foreach (LocalizedText text in localizedTexts)
        {
            // Only update objects IN THE ACTIVE SCENE (skip prefabs)
            if (text.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
            {
                text.UpdateText();
            }
        }
    }

    // ⭐ UPDATE STORE UI
    private void UpdateAllStoreUI()
    {
        var storeUIs = FindObjectsByType<PopulateStoreUIFromDB>(FindObjectsSortMode.None);

        foreach (var ui in storeUIs)
            ui.Apply();
    }


    public string GetTranslation(string key)
    {
        if (translations.ContainsKey(key) &&
            translations[key].ContainsKey(currentLanguage))
        {
            return translations[key][currentLanguage];
        }

        Debug.LogWarning($"Translation not found for key: {key} in language: {currentLanguage}");
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

    private void OnDestroy()
    {
        if (langButton != null) langButton.onClick.RemoveAllListeners();
        if (engBtn != null) engBtn.onClick.RemoveAllListeners();
        if (korBtn != null) korBtn.onClick.RemoveAllListeners();
    }
}
