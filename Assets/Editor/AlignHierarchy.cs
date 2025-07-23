// AlignHierarchyWindow.cs
// 在编辑模式下，将 Target 层级（指定骨骼列表）对齐到 Reference 层级
// 打开方式：Tools ▸ Align Hierarchy Window

using UnityEngine;
using UnityEditor;

public class AlignHierarchyWindow : EditorWindow
{
    /* -------------------- 可拖拽的两个对象 -------------------- */
    private GameObject referenceRoot;
    private GameObject targetRoot;

    /* -------------------- 要同步的节点名字 -------------------- */
    private static readonly string[] NodeNames =
    {
        "pelvis",
        "right_hip_yaw_link",  "right_hip_roll_link",  "right_hip_pitch_link",  "right_knee_link",  "right_ankle_link",
        "left_hip_yaw_link",   "left_hip_roll_link",   "left_hip_pitch_link",   "left_knee_link",   "left_ankle_link",
        "torso_link",
        "right_shoulder_pitch_link", "right_shoulder_roll_link", "right_shoulder_yaw_link", "right_elbow_link",
        "left_shoulder_pitch_link",  "left_shoulder_roll_link",  "left_shoulder_yaw_link",  "left_elbow_link"
    };

    /* -------------------- 打开窗口 -------------------- */
    [MenuItem("Tools/Align Hierarchy")]
    private static void ShowWindow()
    {
        var wnd = GetWindow<AlignHierarchyWindow>();
        wnd.titleContent = new GUIContent("Align Hierarchy");
        wnd.minSize = new Vector2(300, 120);
        wnd.Show();
    }

    /* -------------------- GUI 绘制 -------------------- */
    private void OnGUI()
    {
        GUILayout.Label("Step 1  Drag objects here", EditorStyles.boldLabel);
        referenceRoot = (GameObject)EditorGUILayout.ObjectField("Reference Root", referenceRoot, typeof(GameObject), true);
        targetRoot    = (GameObject)EditorGUILayout.ObjectField("Target Root",    targetRoot,    typeof(GameObject), true);

        // 方便：如果双选两个物体，点一下就自动填
        if (GUILayout.Button("Use Current Selection (first = reference, second = target)"))
            TryFillFromSelection();

        GUILayout.Space(8);

        EditorGUI.BeginDisabledGroup(referenceRoot == null || targetRoot == null);
        if (GUILayout.Button("Align Now", GUILayout.Height(32)))
            Align(referenceRoot, targetRoot);
        EditorGUI.EndDisabledGroup();
    }

    /* -------------------- 自动从当前选择填充 -------------------- */
    private void TryFillFromSelection()
    {
        var sel = Selection.gameObjects;
        if (sel.Length == 2)
        {
            referenceRoot = sel[0];
            targetRoot    = sel[1];
            Repaint();
        }
        else
        {
            ShowNotification(new GUIContent("请一次选中 2 个对象"));
        }
    }

    /* ===================================================================
       核心对齐逻辑 —— 与之前示例保持一致
       =================================================================== */
    private static void Align(GameObject reference, GameObject target)
    {
        if (reference == null || target == null)
        {
            Debug.LogError("Reference 或 Target 为空，无法对齐。");
            return;
        }

        Undo.RecordObject(target.transform, "Align Hierarchy"); // 支持 Ctrl‑Z

        // 1) 根节点
        CopyTransform(reference.transform, target.transform);

        // 2) 指定子节点
        foreach (string name in NodeNames)
        {
            Transform refChild = FindChild(reference.transform, name);
            Transform tarChild = FindChild(target.transform, name);

            if (refChild == null || tarChild == null)
            {
                Debug.LogWarning($"⚠️ 缺少节点 \"{name}\"："
                    + (refChild == null ? "Reference" : "Target"));
                continue;
            }

            Undo.RecordObject(tarChild, "Align Hierarchy Child");
            CopyTransform(refChild, tarChild);
        }

        Debug.Log($"✅ 已将 <{target.name}> 对齐到 <{reference.name}>");
    }

    private static void CopyTransform(Transform src, Transform dst)
    {
        dst.localPosition = src.localPosition;
        dst.localRotation = src.localRotation;
        dst.localScale    = src.localScale;
    }

    private static Transform FindChild(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }
}
