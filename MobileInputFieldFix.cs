using UnityEngine;
using TMPro;

public class MobileInputFieldFix : MonoBehaviour
{
    private void Start()
    {
        #if UNITY_ANDROID || UNITY_IOS
        TMP_InputField[] inputFields = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None);
        foreach (var field in inputFields)
        {
            field.caretWidth = 2;
            field.customCaretColor = true;
            field.caretColor = Color.black;
        }
        #endif
    }
}