using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ManagementTabSwitcher : MonoBehaviour
{
    [Serializable]
    public class Tab
    {
        public string key;             // e.g. "General", "Status", ...
        public Button button;          // the button to click
        public Image background;       // background image to tint; if null, uses button's Image
        public GameObject panel;       // panel to show when active
    }

    [Header("Tabs (order matters)")]
    public List<Tab> tabs = new List<Tab>();

    [Header("Colors")]
    public Color activeColor = new Color32(0xFC, 0xC5, 0x36, 0xFF);  // #FCC536
    public Color fallbackInactive = Color.white;                     // if we can't read original

    [Header("Optional: also hide these when any tab is opened")]
    public List<GameObject> extraPanelsToHide = new List<GameObject>();

    [Header("Startup")]
    public int defaultIndex = 0;

    // internal
    readonly List<Color> _originalColors = new();
    int _current = -1;

    void Awake()
    {
        // cache original colors & hook clicks
        for (int i = 0; i < tabs.Count; i++)
        {
            var t = tabs[i];
            if (!t.button) { Debug.LogWarning($"[TabSwitcher] Missing Button for tab {i}"); continue; }

            var img = t.background ? t.background : t.button.GetComponent<Image>();
            _originalColors.Add(img ? img.color : fallbackInactive);

            int idx = i; // capture
            t.button.onClick.AddListener(() => Select(idx));
        }
    }

    void Start()
    {
        // select default at start
        if (tabs.Count == 0) return;
        defaultIndex = Mathf.Clamp(defaultIndex, 0, tabs.Count - 1);
        Select(defaultIndex);
    }

    public void SelectByKey(string key)
    {
        int idx = tabs.FindIndex(t => string.Equals(t.key, key, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) Select(idx);
    }

    public void Select(int index)
    {
        if (index < 0 || index >= tabs.Count) return;

        _current = index;

        // toggle tabs
        for (int i = 0; i < tabs.Count; i++)
        {
            bool on = (i == index);
            var t = tabs[i];

            if (t.panel) t.panel.SetActive(on);

            var img = t.background ? t.background : t.button ? t.button.GetComponent<Image>() : null;
            if (img) img.color = on ? activeColor : _originalColors[Mathf.Min(i, _originalColors.Count - 1)];
        }

        // hide any extra panels you listed
        foreach (var go in extraPanelsToHide)
            if (go) go.SetActive(false);
    }
}
