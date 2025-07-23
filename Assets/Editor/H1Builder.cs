using UnityEngine;
using UnityEditor;

public class CreateH1HierarchyEditor : EditorWindow
{
    [MenuItem("Tools/Create H1 Hierarchy")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CreateH1HierarchyEditor), false, "Create H1 Hierarchy");
    }

    void OnGUI()
    {
        GUILayout.Label("Generate H1 Joint Hierarchy", EditorStyles.boldLabel);

        if (GUILayout.Button("Create Hierarchy in Scene"))
        {
            CreateH1Hierarchy();
        }
    }

    void CreateH1Hierarchy()
    {
        GameObject pelvis = new GameObject("pelvis");

        GameObject left_hip_yaw_link = new GameObject("left_hip_yaw_link");
        left_hip_yaw_link.transform.parent = pelvis.transform;

        GameObject left_hip_roll_link = new GameObject("left_hip_roll_link");
        left_hip_roll_link.transform.parent = left_hip_yaw_link.transform;

        GameObject left_hip_pitch_link = new GameObject("left_hip_pitch_link");
        left_hip_pitch_link.transform.parent = left_hip_roll_link.transform;

        GameObject left_knee_link = new GameObject("left_knee_link");
        left_knee_link.transform.parent = left_hip_pitch_link.transform;

        GameObject left_ankle_link = new GameObject("left_ankle_link");
        left_ankle_link.transform.parent = left_knee_link.transform;

        GameObject right_hip_yaw_link = new GameObject("right_hip_yaw_link");
        right_hip_yaw_link.transform.parent = pelvis.transform;

        GameObject right_hip_roll_link = new GameObject("right_hip_roll_link");
        right_hip_roll_link.transform.parent = right_hip_yaw_link.transform;

        GameObject right_hip_pitch_link = new GameObject("right_hip_pitch_link");
        right_hip_pitch_link.transform.parent = right_hip_roll_link.transform;

        GameObject right_knee_link = new GameObject("right_knee_link");
        right_knee_link.transform.parent = right_hip_pitch_link.transform;

        GameObject right_ankle_link = new GameObject("right_ankle_link");
        right_ankle_link.transform.parent = right_knee_link.transform;

        GameObject torso_link = new GameObject("torso_link");
        torso_link.transform.parent = pelvis.transform;

        GameObject left_shoulder_pitch_link = new GameObject("left_shoulder_pitch_link");
        left_shoulder_pitch_link.transform.parent = torso_link.transform;

        GameObject left_shoulder_roll_link = new GameObject("left_shoulder_roll_link");
        left_shoulder_roll_link.transform.parent = left_shoulder_pitch_link.transform;

        GameObject left_shoulder_yaw_link = new GameObject("left_shoulder_yaw_link");
        left_shoulder_yaw_link.transform.parent = left_shoulder_roll_link.transform;

        GameObject left_elbow_link = new GameObject("left_elbow_link");
        left_elbow_link.transform.parent = left_shoulder_yaw_link.transform;

        GameObject right_shoulder_pitch_link = new GameObject("right_shoulder_pitch_link");
        right_shoulder_pitch_link.transform.parent = torso_link.transform;

        GameObject right_shoulder_roll_link = new GameObject("right_shoulder_roll_link");
        right_shoulder_roll_link.transform.parent = right_shoulder_pitch_link.transform;

        GameObject right_shoulder_yaw_link = new GameObject("right_shoulder_yaw_link");
        right_shoulder_yaw_link.transform.parent = right_shoulder_roll_link.transform;

        GameObject right_elbow_link = new GameObject("right_elbow_link");
        right_elbow_link.transform.parent = right_shoulder_yaw_link.transform;

        Debug.Log("H1 hierarchy successfully created.");
    }
}
