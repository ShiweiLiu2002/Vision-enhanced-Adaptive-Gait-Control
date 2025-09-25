using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;

/// <summary>
/// 记录一个Transform对象的XZ平面轨迹，并将其保存为CSV文件。
/// </summary>
public class TrajectoryRecorder : MonoBehaviour
{
    [Header("设置 (Settings)")]
    [Tooltip("需要追踪其轨迹的目标物体")]
    public Transform targetObject; // 需要追踪的目标物体

    [Tooltip("每隔多少秒记录一次位置")]
    public float recordingInterval = 0.2f; // 记录间隔

    [Tooltip("输出的CSV文件名")]
    public string outputFileName = "trajectory_data.csv"; // 输出文件名

    // 私有变量
    private Vector3 originPosition; // 参考原点位置
    private List<Vector2> recordedPoints; // 存储记录点的列表
    private float timer; // 计时器
    private bool isRecording; // 是否正在记录的标志

    void Awake()
    {
        // 初始化列表
        recordedPoints = new List<Vector2>();
        
        // 检查目标对象是否已设置
        if (targetObject == null)
        {
            Debug.LogError("错误：请在Inspector面板中指定一个'Target Object'！");
            this.enabled = false; // 禁用此脚本
            return;
        }
    }

    // 当游戏开始时，自动开始记录
    void Start()
    {
        StartRecording();
    }

    void Update()
    {
        // 如果没有在记录，则不做任何事
        if (!isRecording)
        {
            return;
        }

        // 更新计时器
        timer += Time.deltaTime;

        // 如果计时器超过了设定的间隔
        if (timer >= recordingInterval)
        {
            RecordCurrentPosition();
            // 重置计时器，同时减去超出的部分以保持时间精度
            timer -= recordingInterval;
        }
    }

    /// <summary>
    /// 开始记录轨迹。
    /// </summary>
    public void StartRecording()
    {
        if (isRecording)
        {
            Debug.LogWarning("记录已经开始。");
            return;
        }

        // 设置参考原点为当前目标的位置
        originPosition = targetObject.position;
        
        // 清空之前的数据并重置计时器
        recordedPoints.Clear();
        timer = 0f;
        isRecording = true;

        // 记录第一个点，即原点 (0, 0)
        RecordCurrentPosition();

        Debug.Log("轨迹记录开始！参考原点设置为：" + originPosition);
    }

    /// <summary>
    /// 停止记录并保存文件。
    /// </summary>
    public void StopRecordingAndSave()
    {
        if (!isRecording)
        {
            Debug.LogWarning("记录尚未开始，无法停止。");
            return;
        }

        isRecording = false;
        SaveToFile();
    }
    
    // 记录当前位置的函数
    private void RecordCurrentPosition()
    {
        // 计算相对于原点的位移
        Vector3 relativePosition = targetObject.position - originPosition;

        // 只取X和Z的值
        Vector2 xzPoint = new Vector2(relativePosition.x, relativePosition.z);

        // 将数据点添加到列表中
        recordedPoints.Add(xzPoint);
    }
    
    // 将记录的数据保存到CSV文件的函数
    private void SaveToFile()
    {
        // 使用StringBuilder来高效地构建CSV字符串
        StringBuilder sb = new StringBuilder();

        // 添加CSV文件的表头
        sb.AppendLine("relative_x,relative_z");

        // 遍历所有记录的点，并将它们添加到StringBuilder中
        foreach (Vector2 point in recordedPoints)
        {
            // 使用F4格式化字符串，保留4位小数，使数据更整洁
            sb.AppendLine(point.x.ToString("F4") + "," + point.y.ToString("F4"));
        }

        // 获取一个安全的、跨平台的文件保存路径
        // 通常在 C:\Users\[YourUsername]\AppData\LocalLow\[CompanyName]\[ProductName]
        string filePath = Path.Combine(Application.persistentDataPath, outputFileName);

        try
        {
            // 将StringBuilder的内容写入文件
            File.WriteAllText(filePath, sb.ToString());
            Debug.Log($"成功！轨迹数据已保存到: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"错误：无法写入文件 {filePath}。 异常信息: {e.Message}");
        }
    }

    // 当应用程序退出时，自动停止记录并保存文件
    private void OnApplicationQuit()
    {
        if (isRecording)
        {
            StopRecordingAndSave();
        }
    }
}