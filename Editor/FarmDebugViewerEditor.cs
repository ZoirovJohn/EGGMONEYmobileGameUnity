#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for FarmDebugViewer - adds useful buttons
/// Place this file in an "Editor" folder
/// </summary>
[CustomEditor(typeof(FarmDebugViewer))]
public class FarmDebugViewerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        FarmDebugViewer viewer = (FarmDebugViewer)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("🔄 Force Update", GUILayout.Height(30)))
        {
            viewer.ForceUpdate();
        }
        
        if (GUILayout.Button("📦 Toggle Cage Details", GUILayout.Height(30)))
        {
            viewer.ToggleCageDetails();
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("📋 Print to Console", GUILayout.Height(30)))
        {
            viewer.PrintToConsole();
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "This viewer updates automatically during play mode.\n" +
            "• Updates every 0.5 seconds by default\n" +
            "• Toggle 'Show Cage Details' to see individual cage data\n" +
            "• Use 'Print to Console' to copy the data",
            MessageType.Info
        );
    }
}
#endif