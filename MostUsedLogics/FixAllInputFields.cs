using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FixAllInputFields : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FixInputFields();
    }

    void Start()
    {
        FixInputFields();
    }

    void FixInputFields()
    {
        TMP_InputField[] allInputFields = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None);
        
        foreach (TMP_InputField inputField in allInputFields)
        {
            // Visual settings
            inputField.caretBlinkRate = 0.85f;
            inputField.caretWidth = 3;
            inputField.caretColor = Color.black;
            
            // CRITICAL MOBILE KEYBOARD SETTINGS
            inputField.shouldHideSoftKeyboard = false;  // Make sure keyboard shows!
            inputField.shouldHideMobileInput = false;   // Show mobile input overlay
            
            // Keyboard settings
            inputField.keyboardType = TouchScreenKeyboardType.Default;
            inputField.interactable = true;
            
            Debug.Log($"Fixed '{inputField.name}' - HideSoftKeyboard: {inputField.shouldHideSoftKeyboard}, HideMobileInput: {inputField.shouldHideMobileInput}");
        }
        
        Debug.Log($"✓ Fixed {allInputFields.Length} InputFields in {SceneManager.GetActiveScene().name}");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}