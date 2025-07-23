using UnityEditor;
using UnityEngine;

public class MocapBodyGenerator : EditorWindow
{
    private string[] jointNames = new string[]
    {
        "K_pelvis",
        "K_left_hip_yaw_link", "K_left_hip_roll_link", "K_left_hip_pitch_link", "K_left_knee_link", "K_left_ankle_link",
        "K_right_hip_yaw_link", "K_right_hip_roll_link", "K_right_hip_pitch_link", "K_right_knee_link", "K_right_ankle_link",
        "K_torso_link",
        "K_left_shoulder_pitch_link", "K_left_shoulder_roll_link", "K_left_shoulder_yaw_link", "K_left_elbow_link",
        "K_right_shoulder_pitch_link", "K_right_shoulder_roll_link", "K_right_shoulder_yaw_link", "K_right_elbow_link"        
    };

    [MenuItem("Tools/Create Mocap Bodies")]
    public static void ShowWindow()
    {
        GetWindow<MocapBodyGenerator>("Mocap Body Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create Mocap_Bodies GameObject", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Mocap_Bodies with Children"))
        {
            GenerateMocapBodies();
        }
    }

    private void GenerateMocapBodies()
    {
        // 检查是否已存在名为 Mocap_Bodies 的物体
        GameObject root = GameObject.Find("Mocap_Bodies");
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
        root = new GameObject("Mocap_Bodies");
        Undo.RegisterCreatedObjectUndo(root, "Create Mocap_Bodies");

        // 创建子物体
        foreach (var joint in jointNames)
        {
            string childName = "Mocap_" + joint;
            GameObject child = new GameObject(childName);
            child.transform.parent = root.transform;
            Undo.RegisterCreatedObjectUndo(child, "Create " + childName);
        }

        Debug.Log("Mocap_Bodies and children created.");
    }
}
