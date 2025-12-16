#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

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
            viewer.ForceUpdate();

        if (GUILayout.Button("📦 Toggle Cage Details", GUILayout.Height(30)))
            viewer.ToggleCageDetails();

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("📋 Print to Console", GUILayout.Height(30)))
            viewer.PrintToConsole();

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "Shows live FarmDatabase data during Play Mode.\n" +
            "Includes hen lifetime distribution (1–15 days).",
            MessageType.Info
        );
    }
}
#endif
