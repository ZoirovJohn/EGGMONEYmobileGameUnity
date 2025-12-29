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
            inputField.caretBlinkRate = 0.85f;
            inputField.caretWidth = 3;
            inputField.caretColor = Color.black;
            inputField.shouldHideSoftKeyboard = false; 
            inputField.shouldHideMobileInput = false;  
            
            // Keyboard settings
            inputField.keyboardType = TouchScreenKeyboardType.Default;
            inputField.interactable = true;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}