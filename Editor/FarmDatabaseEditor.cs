using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FarmDatabase))]
public class FarmDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        FarmDatabase database = (FarmDatabase)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Load Data from JSON", GUILayout.Height(40)))
        {
            database.LoadFromJSON();
            EditorUtility.SetDirty(database);
            Debug.Log("✅ FarmDatabase reloaded from JSON!");
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Show Farm IDs (Debug)", GUILayout.Height(30)))
        {
            if (database.farms != null && database.farms.Count > 0)
            {
                Debug.Log("=== FARM IDs ===");
                foreach (var farm in database.farms)
                {
                    Debug.Log($"Farm {farm.farmIndex + 1}: farmId='{farm.farmId}', name='{farm.farmName}'");
                }
                Debug.Log("================");
            }
            else
            {
                Debug.LogWarning("No farms loaded!");
            }
        }
    }
}