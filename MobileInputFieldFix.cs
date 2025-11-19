using UnityEngine;
using TMPro;

public class MobileInputFieldFix : MonoBehaviour
{
    private void Start()
    {
        // Force refresh all input fields on mobile
        #if UNITY_ANDROID || UNITY_IOS
        TMP_InputField[] inputFields = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None);
        foreach (var field in inputFields)
        {
            field.caretWidth = 2; // Make caret visible
            field.customCaretColor = true;
            field.caretColor = Color.black; // Or your desired color
        }
        #endif
    }
}