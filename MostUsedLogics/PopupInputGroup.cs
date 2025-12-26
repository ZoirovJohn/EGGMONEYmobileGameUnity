using TMPro;
using UnityEngine;

public class PopupInputGroup : MonoBehaviour
{
    [SerializeField] TMP_InputField[] inputs;

    // Call this from Button / EventTrigger / OnSelect
    public void FocusInput(int index)
    {
        if (inputs == null) return;
        if (index < 0 || index >= inputs.Length) return;

        inputs[index].Select();
        inputs[index].ActivateInputField();
    }
}
