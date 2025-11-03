using UnityEngine;
using UnityEngine.UI;

public class Panel1ProfileStatusButtonsConf : MonoBehaviour
{
    [Header("Panels (roots)")]
    public GameObject profilePanel;
    public GameObject statusPanel;

    CanvasGroup profileCG, statusCG;

    void Awake()
    {
        profileCG = EnsureCanvasGroup(profilePanel);
        statusCG  = EnsureCanvasGroup(statusPanel);
    }

    void Start()
    {
        OpenProfile(); // default: profile visible, status hidden
    }

    public void OpenProfile()
    {
        Show(profileCG);
        Hide(statusCG);
    }

    public void OpenStatus()
    {
        Hide(profileCG);
        Show(statusCG);
    }

    CanvasGroup EnsureCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    void Show(CanvasGroup cg)
    {
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
        // keep GameObject active; no SetActive here
    }

    void Hide(CanvasGroup cg)
    {
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        // keep GameObject active; no SetActive here
    }
}
