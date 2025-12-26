using System.Collections;
using TMPro;
using UnityEngine;

public class PopupInputGroup : MonoBehaviour
{
    [Header("Input order (top → bottom)")]
    [SerializeField] TMP_InputField[] inputs;

    void OnEnable()
    {
        if (inputs != null && inputs.Length > 0)
            StartCoroutine(FocusFirstInput());
    }

    IEnumerator FocusFirstInput()
    {
        // Important for mobile keyboards
        yield return null;
        inputs[0].ActivateInputField();
    }

    // Optional: call manually (Android fallback)
    public void FocusInput(int index)
    {
        if (inputs == null) return;
        if (index < 0 || index >= inputs.Length) return;

        inputs[index].ActivateInputField();
    }
}
