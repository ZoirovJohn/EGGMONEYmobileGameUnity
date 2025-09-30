using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class PlayerInfoLabelBolder : MonoBehaviour
{
    [Header("Assign the parent that holds all ItemCells (Content)")]
    [SerializeField] Transform root;                  // drag StorePanel/ItemsScroll/Viewport/Content here
    [SerializeField] string targetChildName = "PlayerInfo_Left";
    [SerializeField] bool runOnEnable = true;         // re-apply when this object enables (optional)

    // Bold everything up to (and including) the first ':' on each line, keep indentation unbolded
    static readonly Regex FirstColon = new Regex(@"^([ \t]*)([^:\r\n]+?:)", RegexOptions.Multiline);

    void Start()  { Apply(); }
    void OnEnable() { if (runOnEnable) Apply(); }

    [ContextMenu("Apply Now")]
    public void Apply()
    {
        var baseRoot = root ? root : transform;
        if (baseRoot == null) return;

        // find every PlayerInfo_Left under the root
        var all = baseRoot.GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            if (t.name != targetChildName) continue;

            // Legacy UI.Text
            foreach (var uiText in t.GetComponentsInChildren<Text>(true))
            {
                uiText.supportRichText = true;
                uiText.text = BoldLineLabels(uiText.text);
            }

            // TextMeshPro
            foreach (var tmp in t.GetComponentsInChildren<TMP_Text>(true))
            {
                tmp.text = BoldLineLabels(tmp.text);
            }
        }
    }

    static string BoldLineLabels(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return FirstColon.Replace(s, m => $"{m.Groups[1].Value}<b>{m.Groups[2].Value}</b>");
    }
}
