using Unity.MLAgents.Sensors;
using UnityEngine;
using System;

/// <summary>
/// 读取 TerrainEncoder_Inference 中模型输出（未 softmax）作为观测向量输入 Agent
/// </summary>
public class TerrainFeatureObservation : ObservationSource
{
    [SerializeField]
    private bool EnableLogging;
    private TerrainClassification_inference terrainEncoder;

    private float[] cachedLogits = new float[5];  // fallback vector

    public override int Size => 5;

    private void Awake()
    {
        terrainEncoder = GetComponent<TerrainClassification_inference>();
        if (terrainEncoder == null)
        {
            Debug.LogError("[TerrainFeatureObservation] 找不到 TerrainClassification_inference 组件！");
        }
    }

    public override void OnAgentStart()
    {
        // 可选：重置 cachedLogits
        for (int i = 0; i < 5; i++) cachedLogits[i] = 0f;
    }

    public override void FeedObservationsToSensor(VectorSensor sensor)
    {
        if (terrainEncoder != null && terrainEncoder.LatestLogits != null)
        {
            Array.Copy(terrainEncoder.LatestLogits, cachedLogits, 5);
        }
        if (EnableLogging)
        {
            Debug.Log("[TerrainFeatureObservation] Feeding logits: " + string.Join(", ", cachedLogits));
        }

        foreach (float val in cachedLogits)
        {
            sensor.AddObservation(val);
        }
    }
}
