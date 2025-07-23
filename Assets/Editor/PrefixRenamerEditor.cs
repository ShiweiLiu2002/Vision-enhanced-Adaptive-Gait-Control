using UnityEngine;
using UnityEditor;

public class PrefixRenamerEditor : EditorWindow
{
    private GameObject targetObject;
    private string prefix = "Mocap_";

    [MenuItem("Tools/Prefix Renamer")]
    public static void ShowWindow()
    {
        GetWindow<PrefixRenamerEditor>("Prefix Renamer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Add Prefix to Child Object Names", EditorStyles.boldLabel);

        targetObject = (GameObject)EditorGUILayout.ObjectField("Target GameObject", targetObject, typeof(GameObject), true);
        prefix = EditorGUILayout.TextField("Prefix", prefix);

        if (GUILayout.Button("Apply Prefix"))
        {
            if (targetObject == null)
            {
                Debug.LogError("Please assign a target GameObject.");
                return;
            }

            AddPrefixToChildren(targetObject.transform, prefix);
            Debug.Log("Prefix added to child objects.");
        }
    }

    private void AddPrefixToChildren(Transform parent, string prefix)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(includeInactive: true))
        {
            if (!child.name.StartsWith(prefix))
            {
                Undo.RecordObject(child.gameObject, "Rename with Prefix");
                child.name = prefix + child.name;
            }
        }
    }
}
