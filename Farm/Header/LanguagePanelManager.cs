using UnityEngine;
using UnityEngine.UI;

public class LanguagePanelManager : MonoBehaviour
{
    [Header("UI References")]
    public Button langButton;
    public GameObject langPanel;
    public Button engBtn;
    public Button korBtn;

    private void Start()
    {
        // Make sure panel is closed at start
        langPanel.SetActive(false);

        // Add button listeners
        langButton.onClick.AddListener(TogglePanel);
        engBtn.onClick.AddListener(OnEnglishSelected);
        korBtn.onClick.AddListener(OnKoreanSelected);
    }

    private void TogglePanel()
    {
        langPanel.SetActive(!langPanel.activeSelf);
    }

    private void OnEnglishSelected()
    {
        // Add your language change logic here
        Debug.Log("English selected");
        
        // Close the panel
        langPanel.SetActive(false);
    }

    private void OnKoreanSelected()
    {
        // Add your language change logic here
        Debug.Log("Korean selected");
        
        // Close the panel
        langPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // Clean up listeners
        langButton.onClick.RemoveListener(TogglePanel);
        engBtn.onClick.RemoveListener(OnEnglishSelected);
        korBtn.onClick.RemoveListener(OnKoreanSelected);
    }
}