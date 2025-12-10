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

    // Dictionary to store all translations
    private Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Remove DontDestroyOnLoad if only using in Scene 2
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
        // Only setup UI if this is the active instance
        if (Instance != this) return;

        if (langPanel != null) langPanel.SetActive(false);

        if (langButton != null) langButton.onClick.AddListener(TogglePanel);
        if (engBtn != null) engBtn.onClick.AddListener(() => SetLanguage("English"));
        if (korBtn != null) korBtn.onClick.AddListener(() => SetLanguage("Korean"));

        // Apply the current language to all text elements
        UpdateAllTexts();
    }

    private void InitializeTranslations()
    {
        // Load all translations from the TranslationsData file
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
        langPanel.SetActive(false);

        Debug.Log("Language changed to: " + language);
    }

    private void UpdateAllTexts()
    {
        // Update the current language display text
        if (currentLanguageText != null)
        {
            currentLanguageText.text = currentLanguage == "English" ? "ENG" : "한국어";
        }

        // Find all LocalizedText components in the scene, including inactive ones
        LocalizedText[] localizedTexts = Resources.FindObjectsOfTypeAll<LocalizedText>();
        foreach (LocalizedText text in localizedTexts)
        {
            // Skip objects that are in the editor (prefabs), only update scene objects
            if (text.gameObject.scene.name != null)
            {
                text.UpdateText();
            }
        }
    }

    public string GetTranslation(string key)
    {
        if (translations.ContainsKey(key) && translations[key].ContainsKey(currentLanguage))
        {
            return translations[key][currentLanguage];
        }
        
        Debug.LogWarning($"Translation not found for key: {key} in language: {currentLanguage}");
        return key; // Return the key itself if translation not found
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