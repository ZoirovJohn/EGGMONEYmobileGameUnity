using UnityEngine;

public class UIBootstrap : MonoBehaviour
{
    public GameObject bgLogin;
    public GameObject panelLogin;

    public GameObject panelSignUp;
    public GameObject panelTerms;
    public GameObject panelCharacter;

    public GameObject bgCharacterHouse; // in case you have it active

    void Start()
    {
        // Close all panels and backgrounds first
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelSignUp != null) panelSignUp.SetActive(false);
        if (panelTerms != null) panelTerms.SetActive(false);
        if (panelCharacter != null) panelCharacter.SetActive(false);

        if (bgLogin != null) bgLogin.SetActive(false);
        if (bgCharacterHouse != null) bgCharacterHouse.SetActive(false);

        // Open only login panel and its background
        if (bgLogin != null) bgLogin.SetActive(true);
        if (panelLogin != null) panelLogin.SetActive(true);
    }
}
