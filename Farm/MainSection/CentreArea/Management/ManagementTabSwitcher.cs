using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ManagementTabSwitcher : MonoBehaviour
{
    [Serializable]
    public class Tab
    {
        public string key;
        public Button button;       // Button with Image already set
        public GameObject panel;    // Panel to show
        public Sprite activeSprite; // Assign only this!
        
        [HideInInspector] 
        public Sprite originalSprite; // Auto-stored at start
        [HideInInspector] 
        public Image img;            // Cached Image component
    }

    [Header("Tabs (order matters)")]
    public List<Tab> tabs = new List<Tab>();

    [Header("Hide these when any tab is opened (optional)")]
    public List<GameObject> extraPanelsToHide = new List<GameObject>();

    [Header("Startup")]
    public int defaultIndex = 0;

    int _current = -1;

    void Awake()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            var t = tabs[i];
            if (!t.button) continue;

            // cache image + original bg
            t.img = t.button.GetComponent<Image>();
            if (t.img) t.originalSprite = t.img.sprite;

            int idx = i;
            t.button.onClick.AddListener(() => Select(idx));
        }
    }

    void Start()
    {
        if (tabs.Count == 0) return;
        defaultIndex = Mathf.Clamp(defaultIndex, 0, tabs.Count - 1);
        Select(defaultIndex);
    }

    public void Select(int index)
    {
        if (index < 0 || index >= tabs.Count) return;
        _current = index;

        for (int i = 0; i < tabs.Count; i++)
        {
            bool on = (i == index);
            var t = tabs[i];

            if (t.panel) t.panel.SetActive(on);

            if (t.img)
                t.img.sprite = on ? t.activeSprite : t.originalSprite;
        }

        foreach (var go in extraPanelsToHide)
            if (go) go.SetActive(false);
    }
}
