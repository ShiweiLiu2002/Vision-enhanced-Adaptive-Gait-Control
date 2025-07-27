using System;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

namespace ModularAgents.TrainingEvents
{
    public class TerrainsOffsetHandler : TrainingEventHandler
    {
        [Tooltip("所有 terrain 父物体的根节点（如名为 Terrains）")]
        [SerializeField]
        private Transform terrainsRoot;

        [Tooltip("偏移向量列表（世界坐标系）")]
        [SerializeField]
        private List<Vector3> offsetList = new List<Vector3>();

        [Tooltip("是否每一幕随机选择偏移（否则按顺序循环）")]
        [SerializeField]
        private bool useRandomOffset = true;

        private int currentOffsetIndex = 0;

        private List<Transform> geomObjects = new List<Transform>();
        private List<Vector3> originalPositions = new List<Vector3>();

        public override EventHandler Handler => HandleTerrainsOffset;

        private void Awake()
        {
            if (terrainsRoot == null)
            {
                Debug.LogError("[TerrainsOffsetHandler] terrainsRoot 未设置！");
                enabled = false;
                return;
            }

            // 自动查找所有带有 MjGeom 的子物体
            geomObjects.Clear();
            originalPositions.Clear();

            MjGeom[] geoms = terrainsRoot.GetComponentsInChildren<MjGeom>(includeInactive: true);
            foreach (var geom in geoms)
            {
                Transform t = geom.transform;
                geomObjects.Add(t);
                originalPositions.Add(t.position);
            }

            if (geomObjects.Count == 0)
            {
                Debug.LogWarning("[TerrainsOffsetHandler] 在 terrainsRoot 下未找到任何 MjGeom！");
            }
            else
            {
                Debug.Log($"[TerrainsOffsetHandler] 发现 {geomObjects.Count} 个 MjGeom 地形对象");
            }
        }

        private void HandleTerrainsOffset(object sender, EventArgs args)
        {
            if (offsetList == null || offsetList.Count == 0)
            {
                Debug.LogWarning("[TerrainsOffsetHandler] offsetList 为空！");
                return;
            }

            if (geomObjects.Count == 0)
            {
                Debug.LogWarning("[TerrainsOffsetHandler] 没有找到需要偏移的地形 MjGeom 对象！");
                return;
            }

            // 选择偏移量
            Vector3 offset = useRandomOffset
                ? offsetList[UnityEngine.Random.Range(0, offsetList.Count)]
                : offsetList[currentOffsetIndex++ % offsetList.Count];

            for (int i = 0; i < geomObjects.Count; i++)
            {
                geomObjects[i].position = originalPositions[i] + offset;
            }

            Debug.Log($"[TerrainsOffsetHandler] 所有 MjGeom 地形偏移 {offset}");
        }
    }
}
