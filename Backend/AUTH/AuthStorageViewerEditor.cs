using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AuthStorageViewer))]
public class AuthStorageViewerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(15);

        if (GUILayout.Button("Show Stored Access Token"))
        {
            string token = AuthStorage.GetAccessToken();
            if (string.IsNullOrEmpty(token))
                Debug.Log("No token saved.");
            else
                Debug.Log("Saved Token:\n" + token);
        }

        if (GUILayout.Button("Delete Stored Token"))
        {
            AuthStorage.DeleteAccessToken();
            Debug.Log("Token deleted.");
        }
    }
}
