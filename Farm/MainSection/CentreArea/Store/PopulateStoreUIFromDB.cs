using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PopulateStoreUIFromDB : MonoBehaviour
{
    [Header("Assign these")]
    [SerializeField] StoreDB store;
    [SerializeField] Transform content;
    [SerializeField] string playerInfoNode = "PlayerInfo_Left";

    void Start() => Apply();

    [ContextMenu("Apply Now")]
    public void Apply()
    {
        if (!store) store = StoreDB.Instance;
        if (!store || content == null || store.items.Count == 0)
        {
            Debug.LogWarning("Store or content missing.");
            return;
        }

        List<Transform> cells = new();
        for (int i = 0; i < content.childCount; i++)
        {
            Transform cell = content.GetChild(i);
            if (cell.Find(playerInfoNode) != null)
                cells.Add(cell);
        }

        int count = Mathf.Min(store.items.Count, cells.Count);

        for (int i = 0; i < count; i++)
        {
            var item = store.items[i];
            var infoRoot = cells[i].Find(playerInfoNode);

            if (!infoRoot) continue;

            string name = item.Title;
            string desc = item.Description;
            string price = $"{item.priceFP:N0} FP";

            SetField(infoRoot, "Name", name);
            SetField(infoRoot, "Info", desc);
            SetField(infoRoot, "Price", price);
        }
    }

    void SetField(Transform root, string fieldName, string value)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == fieldName)
            {
                var tmp = t.GetComponent<TMP_Text>();
                if (tmp) { tmp.text = value; return; }

                var ui = t.GetComponent<Text>();
                if (ui) { ui.text = value; return; }
            }
        }

        Debug.LogWarning($"Store field not found: {fieldName}");
    }
}
