using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelLogin;
    public GameObject panelSignUp;
    public GameObject panelTerms;
    public GameObject panelCharacter;

    [Header("Backgrounds")]
    public GameObject bgSignupLogin;    // used for login, signup, terms
    public GameObject bgCharacterHouse;  // used for character select

    public void ShowLogin()
    {
        HideAll();
        if (bgSignupLogin != null) bgSignupLogin.SetActive(true);
        if (panelLogin != null) panelLogin.SetActive(true);
    }

    public void ShowSignUp()
    {
        HideAll();
        if (bgSignupLogin != null) bgSignupLogin.SetActive(true);
        if (panelSignUp != null) panelSignUp.SetActive(true);
    }

    public void ShowTerms()
    {
        HideAll();
        if (bgSignupLogin != null) bgSignupLogin.SetActive(true);
        if (panelTerms != null) panelTerms.SetActive(true);
    }

    public void ShowCharacter()
    {
        HideAll();
        if (bgCharacterHouse != null) bgCharacterHouse.SetActive(true);
        if (panelCharacter != null) panelCharacter.SetActive(true);
    }

    private void HideAll()
    {
        // panels
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelSignUp != null) panelSignUp.SetActive(false);
        if (panelTerms != null) panelTerms.SetActive(false);
        if (panelCharacter != null) panelCharacter.SetActive(false);

        // backgrounds
        if (bgSignupLogin != null) bgSignupLogin.SetActive(false);
        if (bgCharacterHouse != null) bgCharacterHouse.SetActive(false);
    }
}
