using UnityEditor;
using UnityEngine;

public class WeldOffsetGenerator : EditorWindow
{
    private string[] jointNames = new string[]
    {
        "pelvis",
        "left_hip_yaw_link", "left_hip_roll_link", "left_hip_pitch_link", "left_knee_link", "left_ankle_link",
        "right_hip_yaw_link", "right_hip_roll_link", "right_hip_pitch_link", "right_knee_link", "right_ankle_link",
        "torso_link",
        "left_shoulder_pitch_link", "left_shoulder_roll_link", "left_shoulder_yaw_link", "left_elbow_link",
        "right_shoulder_pitch_link", "right_shoulder_roll_link", "right_shoulder_yaw_link", "right_elbow_link"        
    };

    [MenuItem("Tools/Create WeldOffset Objects")]
    public static void ShowWindow()
    {
        GetWindow<WeldOffsetGenerator>("WeldOffset Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create WeldOffset GameObject", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate"))
        {
            GenerateMocapBodies();
        }
    }

    private void GenerateMocapBodies()
    {
        GameObject root = GameObject.Find("equality constraints");
        if (root != null)
        {
            if (!EditorUtility.DisplayDialog("Mocap_Bodies already exists",
                "A GameObject named 'Mocap_Bodies' already exists. Do you want to overwrite it?", "Yes", "No"))
            {
                return;
            }
            DestroyImmediate(root);
        }

        // 创建根节点
        root = new GameObject("equality constraints");
        Undo.RegisterCreatedObjectUndo(root, "Create WeldOffset");

        // 创建子物体
        foreach (var joint in jointNames)
        {
            string childName = "Weld_" + joint;
            GameObject child = new GameObject(childName);
            child.transform.parent = root.transform;
            Undo.RegisterCreatedObjectUndo(child, "Create " + childName);
        }

        Debug.Log("WeldOffset objects created.");
    }
}
