using UnityEngine;
using UnityEngine.UI;

public class TermsSignupCheckBox : MonoBehaviour
{
    [Header("Assign both toggles")]
    public Toggle signUpToggle;   // the checkbox on Panel_SignUp
    public Toggle termsToggle;    // the checkbox on Panel_Terms

    bool _updating;

    void Awake()
    {
        if (signUpToggle == null || termsToggle == null) return;

        // Start identical (prefer sign-up's value if you want)
        termsToggle.isOn = signUpToggle.isOn;

        // Subscribe both ways
        signUpToggle.onValueChanged.AddListener(OnSignUpChanged);
        termsToggle.onValueChanged.AddListener(OnTermsChanged);
    }

    void OnDestroy()
    {
        if (signUpToggle != null) signUpToggle.onValueChanged.RemoveListener(OnSignUpChanged);
        if (termsToggle != null) termsToggle.onValueChanged.RemoveListener(OnTermsChanged);
    }

    void OnSignUpChanged(bool v)
    {
        if (_updating) return;
        _updating = true;
        termsToggle.isOn = v;
        _updating = false;
    }

    void OnTermsChanged(bool v)
    {
        if (_updating) return;
        _updating = true;
        signUpToggle.isOn = v;
        _updating = false;
    }
}
