using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

/// <summary>
/// [最终版] 记录两个机器人之间的 Pose Error。
/// 误差计算方式为：对应关节相对于各自根节点的相对位置之间的“平均总欧几里得距离”。
/// </summary>
public class PoseErrorRecorder : MonoBehaviour
{
    [Header("机器人根节点 (Robot Roots)")]
    [Tooltip("运动学参考机器人（模板）的根节点 Transform。")]
    public Transform kinRoot;

    [Tooltip("物理模拟机器人（代理）的根节点 Transform。")]
    public Transform simRoot;

    [Header("记录设置 (Recording Settings)")]
    [Tooltip("记录数据的时间间隔（秒）。")]
    public float recordingInterval = 0.1f;

    [Tooltip("输出的CSV文件名。")]
    public string outputFileName = "pose_error_log.csv";

    // 内部变量
    private List<Transform> kinBodyParts;
    private List<Transform> simBodyParts;
    private List<float> poseErrorHistory;
    private float timer;
    private bool isRecording = false;

    void Start()
    {
        if (kinRoot == null || simRoot == null)
        {
            Debug.LogError("错误：Kinematic Root 或 Simulated Root 未在 Inspector 中设置！将禁用此脚本。");
            this.enabled = false;
            return;
        }

        kinBodyParts = kinRoot.GetComponentsInChildren<Transform>().Skip(1).ToList();
        simBodyParts = simRoot.GetComponentsInChildren<Transform>().Skip(1).ToList();

        if (kinBodyParts.Count != simBodyParts.Count)
        {
            Debug.LogWarning($"警告：骨骼数量不匹配！ Kinematic: {kinBodyParts.Count}, Simulated: {simBodyParts.Count}. 计算结果可能不准确。");
        }
        
        if (kinBodyParts.Count == 0)
        {
            Debug.LogError("错误：在 Kinematic Root 下未找到任何子骨骼！将禁用此脚本。");
            this.enabled = false;
            return;
        }

        poseErrorHistory = new List<float>();
        isRecording = true;
        timer = 0f;
        
        Debug.Log($"Pose Error (Euclidean Distance) 记录已开始。数据将保存在: {Path.Combine(Application.persistentDataPath, outputFileName)}");
    }

    void Update()
    {
        if (!isRecording) return;

        timer += Time.deltaTime;

        if (timer >= recordingInterval)
        {
            timer -= recordingInterval; 
            
            float currentPoseError = CalculatePoseError();
            poseErrorHistory.Add(currentPoseError);
        }
    }
    
    /// <summary>
    /// 计算当前帧的 Pose Error。
    /// 公式: Average(EuclideanDistance(relative_sim_pos, relative_kin_pos))
    /// </summary>
    /// <returns>Pose Error 值。</returns>
    private float CalculatePoseError()
    {
        // 1. 遍历所有对应的骨骼，计算它们之间相对位置的距离，然后求和。
        float totalDistance = simBodyParts.Zip(kinBodyParts, (sim, kin) =>
        {
            // 计算关节相对于其各自根节点的位置
            Vector3 relativeSimPos = sim.position - simRoot.position;
            Vector3 relativeKinPos = kin.position - kinRoot.position;
            
            // 计算这两个相对位置之间的欧几里得距离
            return Vector3.Distance(relativeKinPos, relativeSimPos);

        }).Sum();

        // 2. 将总距离除以骨骼数量，得到平均距离
        if (simBodyParts.Count == 0)
        {
            return 0f; // 避免除以零
        }
        
        float averageDistance = totalDistance / simBodyParts.Count;
        averageDistance *= 0.7f;
        return averageDistance;
    }

    private void OnApplicationQuit()
    {
        SaveToFile();
    }

    private void OnDisable()
    {
        SaveToFile();
    }

    private void SaveToFile()
    {
        if (!isRecording || poseErrorHistory == null || poseErrorHistory.Count == 0) return;

        isRecording = false; 

        string filePath = Path.Combine(Application.persistentDataPath, outputFileName);
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Time(s),PoseError(AvgEuclideanDist)");

        for (int i = 0; i < poseErrorHistory.Count; i++)
        {
            float timeStamp = i * recordingInterval;
            float errorValue = poseErrorHistory[i];
            sb.AppendLine($"{timeStamp.ToString("F3")},{errorValue}");
        }

        try
        {
            File.WriteAllText(filePath, sb.ToString());
            Debug.Log($"成功将 {poseErrorHistory.Count} 条 Pose Error 数据保存到: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存文件失败: {e.Message}");
        }
    }
}