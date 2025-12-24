using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    [Header("Translation Key")]
    [Tooltip("The key to look up in the LanguageManager translations")]
    public string translationKey;

    private Text uiText;
    private TextMeshProUGUI tmpText;

    private void Awake()
    {
        // Check which text component this object has
        uiText = GetComponent<Text>();
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (LanguageManager.Instance == null)
        {
            return; // Silently skip if LanguageManager not ready yet
        }

        string translatedText = LanguageManager.Instance.GetTranslation(translationKey);

        // Update the appropriate text component
        if (uiText != null)
        {
            uiText.text = translatedText;
        }
        else if (tmpText != null)
        {
            tmpText.text = translatedText;
        }
    }
}