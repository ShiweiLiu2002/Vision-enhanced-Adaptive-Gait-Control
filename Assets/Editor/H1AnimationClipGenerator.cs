// H1AnimationClipGenerator.cs
// MuJoCo → Unity (left–hand) 轴转换：x → x, y → z, z → y
// 顶层 Unitree_h1 读前两列 (x, y→z)；pelvis 的 xz 固定 0，只取 z→y 及四元数。

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class H1AnimationClipGenerator : EditorWindow
{
    public TextAsset csvFile;
    public GameObject rigRoot;          // 这里就是 Unitree_h1
    public float frameRate = 30f;

    private readonly string[] jointNames = new string[]
    {
        "right_hip_yaw_link", "right_hip_roll_link", "right_hip_pitch_link", "right_knee_link", "right_ankle_link",
        "left_hip_yaw_link",  "left_hip_roll_link",  "left_hip_pitch_link",  "left_knee_link",  "left_ankle_link",
        "torso_link",
        "right_shoulder_pitch_link", "right_shoulder_roll_link", "right_shoulder_yaw_link", "right_elbow_link",
        "left_shoulder_pitch_link",  "left_shoulder_roll_link",  "left_shoulder_yaw_link",  "left_elbow_link"
        // "K_right_hip_yaw_link", "K_right_hip_roll_link", "K_right_hip_pitch_link", "K_right_knee_link", "K_right_ankle_link",
        // "K_left_hip_yaw_link", "K_left_hip_roll_link", "K_left_hip_pitch_link", "K_left_knee_link", "K_left_ankle_link",
        // "K_torso_link",
        // "K_right_shoulder_pitch_link", "K_right_shoulder_roll_link", "K_right_shoulder_yaw_link", "K_right_elbow_link",
        // "K_left_shoulder_pitch_link", "K_left_shoulder_roll_link", "K_left_shoulder_yaw_link", "K_left_elbow_link" 
    };

    // MuJoCo 关节角→Unity 关节欧拉轴映射
    private readonly Dictionary<string, (string axis, int sign)> jointAxisMap = new()
    {
        {"left_hip_yaw_link",         ("y",  1)},
        {"left_hip_roll_link",        ("x",  1)},
        {"left_hip_pitch_link",       ("z", -1)},
        {"left_knee_link",            ("z", -1)},
        {"left_ankle_link",           ("z", -1)},

        {"right_hip_yaw_link",        ("y",  1)},
        {"right_hip_roll_link",       ("x",  1)},
        {"right_hip_pitch_link",      ("z", -1)},
        {"right_knee_link",           ("z", -1)},
        {"right_ankle_link",          ("z", -1)},

        {"torso_link",                ("y",  1)},

        {"left_shoulder_pitch_link",  ("z", -1)},
        {"left_shoulder_roll_link",   ("x",  1)},
        {"left_shoulder_yaw_link",    ("y",  1)},
        {"left_elbow_link",           ("z", -1)},

        {"right_shoulder_pitch_link", ("z", -1)},
        {"right_shoulder_roll_link",  ("x",  1)},
        {"right_shoulder_yaw_link",   ("y",  1)},
        {"right_elbow_link",          ("z", -1)}
        // {"K_left_hip_yaw_link",         ("y",  1)},
        // {"K_left_hip_roll_link",        ("x",  1)},
        // {"K_left_hip_pitch_link",       ("z", -1)},
        // {"K_left_knee_link",            ("z", -1)},
        // {"K_left_ankle_link",           ("z", -1)},

        // {"K_right_hip_yaw_link",        ("y",  1)},
        // {"K_right_hip_roll_link",       ("x",  1)},
        // {"K_right_hip_pitch_link",      ("z", -1)},
        // {"K_right_knee_link",           ("z", -1)},
        // {"K_right_ankle_link",          ("z", -1)},

        // {"K_torso_link",                ("y",  1)},

        // {"K_left_shoulder_pitch_link",  ("z", -1)},
        // {"K_left_shoulder_roll_link",   ("x",  1)},
        // {"K_left_shoulder_yaw_link",    ("y",  1)},
        // {"K_left_elbow_link",           ("z", -1)},

        // {"K_right_shoulder_pitch_link", ("z", -1)},
        // {"K_right_shoulder_roll_link",  ("x",  1)},
        // {"K_right_shoulder_yaw_link",   ("y",  1)},
        // {"K_right_elbow_link",          ("z", -1)}
    };

    [MenuItem("Tools/Generate h1 animation clip")]
    private static void Init()
    {
        var window = GetWindow<H1AnimationClipGenerator>();
        window.titleContent = new GUIContent("H1 Animator");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate AnimationClip from H1 CSV", EditorStyles.boldLabel);
        csvFile  = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        rigRoot  = (GameObject)EditorGUILayout.ObjectField("Rig Root (Unitree_h1)", rigRoot, typeof(GameObject), true);
        frameRate = EditorGUILayout.FloatField("Frame Rate", frameRate);

        if (GUILayout.Button("Generate Full‑Body Clip"))
            CreateAnimationClip();
    }

    private void CreateAnimationClip()
    {
        if (csvFile == null || rigRoot == null)
        {
            Debug.LogError("CSV 或 RigRoot 未指定！");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        var clip = new AnimationClip { frameRate = frameRate };

        /* ---------- Unitree_h1（根节点）平移曲线 ---------- */
        var rootPosX = new AnimationCurve();
        var rootPosZ = new AnimationCurve();      // 只需要 x、z 平面移动

        /* ---------- pelvis 位移 & 旋转曲线 ---------- */
        var pelvisPosY = new AnimationCurve();
        var pelvisRotX = new AnimationCurve();
        var pelvisRotY = new AnimationCurve();
        var pelvisRotZ = new AnimationCurve();
        var pelvisRotW = new AnimationCurve();

        /* ---------- 各关节欧拉角曲线 ---------- */
        Dictionary<string, AnimationCurve> jointCurveX = new();
        Dictionary<string, AnimationCurve> jointCurveY = new();
        Dictionary<string, AnimationCurve> jointCurveZ = new();

        foreach (var j in jointNames)
        {
            jointCurveX[j] = new AnimationCurve();
            jointCurveY[j] = new AnimationCurve();
            jointCurveZ[j] = new AnimationCurve();
        }

        /* ---------- 逐帧读取 CSV ---------- */
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] tok = line.Split(',');
            float t = i / frameRate;

            /* MuJoCo → Unity 轴重排：x y z → x z y  */
            float csv_x = float.Parse(tok[0]);    // → Unity.x
            float csv_y = float.Parse(tok[1]);    // → Unity.z
            float csv_z = float.Parse(tok[2]);    // → Unity.y

            // ---------- 顶层 Unitree_h1 只做平面移动 ----------
            rootPosX.AddKey(t, csv_x);
            rootPosZ.AddKey(t, -csv_y);            // 注意：MuJoCo.y → Unity.z

            // ---------- pelvis 只做竖直位移 ----------
            pelvisPosY.AddKey(t, csv_z);          // MuJoCo.z → Unity.y

            // ---------- pelvis 旋转 ----------
            Quaternion qCsv = new(
                float.Parse(tok[3]),  // qx
                float.Parse(tok[4]),  // qy
                float.Parse(tok[5]),  // qz
                float.Parse(tok[6])); // qw

            // 沿用先前的四元数修正（x→-x, y→-z, z→y, w→-w）
            Quaternion qUnity = new(
                -qCsv.x,
                -qCsv.z,
                 qCsv.y,
                -qCsv.w);

            pelvisRotX.AddKey(t, qUnity.x);
            pelvisRotY.AddKey(t, qUnity.y);
            pelvisRotZ.AddKey(t, qUnity.z);
            pelvisRotW.AddKey(t, qUnity.w);

            // ---------- 关节欧拉角 ----------
            for (int j = 0; j < jointNames.Length; j++)
            {
                string joint = jointNames[j];
                float angRad = float.Parse(tok[7 + j]);
                float angDeg = angRad * Mathf.Rad2Deg * jointAxisMap[joint].sign;

                switch (jointAxisMap[joint].axis)
                {
                    case "x": jointCurveX[joint].AddKey(t, angDeg); break;
                    case "y": jointCurveY[joint].AddKey(t, angDeg); break;
                    case "z": jointCurveZ[joint].AddKey(t, angDeg); break;
                }
            }
        }

        /* ---------- 写入 AnimationClip ---------- */

        // 1. Unitree_h1 根节点（空路径即表示自身）
        clip.SetCurve("", typeof(Transform), "localPosition.x", rootPosX);
        clip.SetCurve("", typeof(Transform), "localPosition.z", rootPosZ);

        // 2. pelvis 位移 + 旋转
        string pelvisPath = GetRelativePath("pelvis");
        clip.SetCurve(pelvisPath, typeof(Transform), "localPosition.y", pelvisPosY);
        clip.SetCurve(pelvisPath, typeof(Transform), "localRotation.x", pelvisRotX);
        clip.SetCurve(pelvisPath, typeof(Transform), "localRotation.y", pelvisRotY);
        clip.SetCurve(pelvisPath, typeof(Transform), "localRotation.z", pelvisRotZ);
        clip.SetCurve(pelvisPath, typeof(Transform), "localRotation.w", pelvisRotW);

        // 3. 其余关节
        foreach (var j in jointNames)
        {
            string p = GetRelativePath(j);
            if (jointCurveX[j].length > 0) clip.SetCurve(p, typeof(Transform), "localEulerAngles.x", jointCurveX[j]);
            if (jointCurveY[j].length > 0) clip.SetCurve(p, typeof(Transform), "localEulerAngles.y", jointCurveY[j]);
            if (jointCurveZ[j].length > 0) clip.SetCurve(p, typeof(Transform), "localEulerAngles.z", jointCurveZ[j]);
        }

        /* ---------- 保存 ---------- */
        AssetDatabase.CreateAsset(clip, "Assets/Animation/H1_Walk.anim");
        AssetDatabase.SaveAssets();
        Debug.Log("✅ Full‑body animation created: Assets/Animation/H1_Walk.anim");
    }

    /* -------------------------------------------------------------------- */
    /* ---------------------   工具函数保持不变   --------------------------- */
    /* -------------------------------------------------------------------- */

    private string GetRelativePath(string jointName)
    {
        Transform joint = FindTransformInChildren(rigRoot.transform, jointName);
        if (joint == null)
        {
            Debug.LogError("找不到骨骼节点: " + jointName);
            return jointName;
        }

        string path = joint.name;
        while (joint.parent != rigRoot.transform && joint.parent != null)
        {
            joint = joint.parent;
            path = joint.name + "/" + path;
        }
        return path;
    }

    private Transform FindTransformInChildren(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }
}

// // H1AnimationClipGenerator.cs
// // Root position + rotation + joint angles with axis correction (MuJoCo to Unity: x→x, y→z (flip), z→y)

// // using UnityEngine;
// // using UnityEditor;
// // using System.Collections.Generic;
// // using System.IO;

// // public class H1AnimationClipGenerator : EditorWindow
// // {
// //     public TextAsset csvFile;
// //     public GameObject rigRoot;
// //     public float frameRate = 30f;

// //     private string[] jointNames = new string[]
// //     {
// //         "K_right_hip_yaw_link", "K_right_hip_roll_link", "K_right_hip_pitch_link", "K_right_knee_link", "K_right_ankle_link",
// //         "K_left_hip_yaw_link", "K_left_hip_roll_link", "K_left_hip_pitch_link", "K_left_knee_link", "K_left_ankle_link",
// //         "K_torso_link",
// //         "K_right_shoulder_pitch_link", "K_right_shoulder_roll_link", "K_right_shoulder_yaw_link", "K_right_elbow_link",
// //         "K_left_shoulder_pitch_link", "K_left_shoulder_roll_link", "K_left_shoulder_yaw_link", "K_left_elbow_link" 
// //     };

// //     // From MuJoCo axis (x,y,z) to Unity (x,z,-y) coordinate correction
// //     private Dictionary<string, (string axis, int sign)> jointAxisMap = new Dictionary<string, (string, int)>
// //     {
// //         {"K_left_hip_yaw_link",         ("y",  1)},
// //         {"K_left_hip_roll_link",        ("x",  1)},
// //         {"K_left_hip_pitch_link",       ("z", -1)},
// //         {"K_left_knee_link",            ("z", -1)},
// //         {"K_left_ankle_link",           ("z", -1)},

// //         {"K_right_hip_yaw_link",        ("y",  1)},
// //         {"K_right_hip_roll_link",       ("x",  1)},
// //         {"K_right_hip_pitch_link",      ("z", -1)},
// //         {"K_right_knee_link",           ("z", -1)},
// //         {"K_right_ankle_link",          ("z", -1)},

// //         {"K_torso_link",                ("y",  1)},

// //         {"K_left_shoulder_pitch_link",  ("z", -1)},
// //         {"K_left_shoulder_roll_link",   ("x",  1)},
// //         {"K_left_shoulder_yaw_link",    ("y",  1)},
// //         {"K_left_elbow_link",           ("z", -1)},

// //         {"K_right_shoulder_pitch_link", ("z", -1)},
// //         {"K_right_shoulder_roll_link",  ("x",  1)},
// //         {"K_right_shoulder_yaw_link",   ("y",  1)},
// //         {"K_right_elbow_link",          ("z", -1)}
// //     };

// //     [MenuItem("Tools/Generate h1 animation clip")]
// //     static void Init()
// //     {
// //         H1AnimationClipGenerator window = GetWindow<H1AnimationClipGenerator>();
// //         window.titleContent = new GUIContent("H1 Animator");
// //         window.Show();
// //     }

// //     void OnGUI()
// //     {
// //         GUILayout.Label("Generate AnimationClip from H1 CSV", EditorStyles.boldLabel);
// //         csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
// //         rigRoot = (GameObject)EditorGUILayout.ObjectField("Rig Root", rigRoot, typeof(GameObject), true);
// //         frameRate = EditorGUILayout.FloatField("Frame Rate", frameRate);

// //         if (GUILayout.Button("Generate Full Body Clip"))
// //         {
// //             CreateAnimationClip();
// //         }
// //     }

// //     void CreateAnimationClip()
// //     {
// //         if (csvFile == null || rigRoot == null)
// //         {
// //             Debug.LogError("CSV or rig root is not assigned.");
// //             return;
// //         }

// //         string[] lines = csvFile.text.Split('\n');
// //         AnimationClip clip = new AnimationClip { frameRate = frameRate };

// //         var rootPosX = new AnimationCurve();
// //         var rootPosY = new AnimationCurve();
// //         var rootPosZ = new AnimationCurve();

// //         var rootRotX = new AnimationCurve();
// //         var rootRotY = new AnimationCurve();
// //         var rootRotZ = new AnimationCurve();
// //         var rootRotW = new AnimationCurve();

// //         Dictionary<string, AnimationCurve> jointCurveX = new();
// //         Dictionary<string, AnimationCurve> jointCurveY = new();
// //         Dictionary<string, AnimationCurve> jointCurveZ = new();

// //         foreach (var joint in jointNames)
// //         {
// //             jointCurveX[joint] = new AnimationCurve();
// //             jointCurveY[joint] = new AnimationCurve();
// //             jointCurveZ[joint] = new AnimationCurve();
// //         }

// //         for (int i = 0; i < lines.Length; i++)
// //         {
// //             string line = lines[i].Trim();
// //             if (string.IsNullOrWhiteSpace(line)) continue;

// //             string[] tokens = line.Split(',');
// //             float t = i / frameRate;

// //             float csv_x = float.Parse(tokens[0]);
// //             float csv_y = float.Parse(tokens[1]);
// //             float csv_z = float.Parse(tokens[2]);

// //             Vector3 posUnity = new Vector3(csv_x, csv_z, -csv_y); // Mirror z
// //             rootPosX.AddKey(t, posUnity.x);
// //             rootPosY.AddKey(t, posUnity.y);
// //             rootPosZ.AddKey(t, posUnity.z);

// //             Quaternion qCsv = new Quaternion(
// //                 float.Parse(tokens[3]), float.Parse(tokens[4]), float.Parse(tokens[5]), float.Parse(tokens[6])
// //             );
// //             Quaternion qUnity = new Quaternion(
// //                 -qCsv.x, -qCsv.z, qCsv.y, -qCsv.w // reverted to previous form: no z flip here
// //             );

// //             rootRotX.AddKey(t, qUnity.x);
// //             rootRotY.AddKey(t, qUnity.y);
// //             rootRotZ.AddKey(t, qUnity.z);
// //             rootRotW.AddKey(t, qUnity.w);

// //             for (int j = 0; j < jointNames.Length; j++)
// //             {
// //                 string joint = jointNames[j];
// //                 float angleRad = float.Parse(tokens[7 + j]);
// //                 float angleDeg = angleRad * Mathf.Rad2Deg * jointAxisMap[joint].sign;

// //                 switch (jointAxisMap[joint].axis)
// //                 {
// //                     case "x": jointCurveX[joint].AddKey(t, angleDeg); break;
// //                     case "y": jointCurveY[joint].AddKey(t, angleDeg); break;
// //                     case "z": jointCurveZ[joint].AddKey(t, angleDeg); break;
// //                 }
// //             }
// //         }

// //         string rootPath = GetRelativePath("K_pelvis");
// //         clip.SetCurve(rootPath, typeof(Transform), "localPosition.x", rootPosX);
// //         clip.SetCurve(rootPath, typeof(Transform), "localPosition.y", rootPosY);
// //         clip.SetCurve(rootPath, typeof(Transform), "localPosition.z", rootPosZ);
// //         clip.SetCurve(rootPath, typeof(Transform), "localRotation.x", rootRotX);
// //         clip.SetCurve(rootPath, typeof(Transform), "localRotation.y", rootRotY);
// //         clip.SetCurve(rootPath, typeof(Transform), "localRotation.z", rootRotZ);
// //         clip.SetCurve(rootPath, typeof(Transform), "localRotation.w", rootRotW);

// //         foreach (var joint in jointNames)
// //         {
// //             string path = GetRelativePath(joint);
// //             if (jointCurveX[joint].length > 0) clip.SetCurve(path, typeof(Transform), "localEulerAngles.x", jointCurveX[joint]);
// //             if (jointCurveY[joint].length > 0) clip.SetCurve(path, typeof(Transform), "localEulerAngles.y", jointCurveY[joint]);
// //             if (jointCurveZ[joint].length > 0) clip.SetCurve(path, typeof(Transform), "localEulerAngles.z", jointCurveZ[joint]);
// //         }

// //         AssetDatabase.CreateAsset(clip, "Assets/Animation/H1_Walk.anim");
// //         AssetDatabase.SaveAssets();
// //         Debug.Log("✅ Full-body animation created: Assets/Animation/H1_Walk.anim");
// //     }

// //     string GetRelativePath(string jointName)
// //     {
// //         Transform joint = FindTransformInChildren(rigRoot.transform, jointName);
// //         if (joint == null)
// //         {
// //             Debug.LogError("Transform not found for: " + jointName);
// //             return jointName;
// //         }

// //         string path = joint.name;
// //         while (joint.parent != rigRoot.transform && joint.parent != null)
// //         {
// //             joint = joint.parent;
// //             path = joint.name + "/" + path;
// //         }
// //         return path;
// //     }

// //     Transform FindTransformInChildren(Transform root, string name)
// //     {
// //         foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
// //             if (t.name == name) return t;
// //         return null;
// //     }
// // }
