using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;

public class Detection_inference : MonoBehaviour
{
    public Camera captureCamera;
    public Camera depthCamera;
    public RenderTexture renderTexture;
    public RenderTexture depthRenderTexture;
    public ModelAsset modelAsset;
    public List<Detection> detections = new List<Detection>();

    private Texture2D inputTexture;
    private Texture2D depthReadbackTexture;
    private Worker worker;
    private Model model;
    void Start()
    {
        model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, BackendType.GPUCompute);
        inputTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        depthReadbackTexture = new Texture2D(depthRenderTexture.width, depthRenderTexture.height, TextureFormat.RFloat, false);
        StartCoroutine(RunInferenceLoop());
    }

    IEnumerator RunInferenceLoop()
    {
        while (true)
        {
            // 1. 将当前摄像头图像读取成 Texture2D
            RenderTexture.active = renderTexture;
            inputTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            inputTexture.Apply();
            RenderTexture.active = null;

            // 深度图同理
            RenderTexture.active = depthRenderTexture;
            depthReadbackTexture.ReadPixels(new Rect(0, 0, depthRenderTexture.width, depthRenderTexture.height), 0, 0);
            depthReadbackTexture.Apply();
            RenderTexture.active = null;


            // 2. 转为张量
            Tensor<float> inputTensor = TextureConverter.ToTensor(inputTexture, channels: 3);

            // 3. 推理
            worker.Schedule(inputTensor);
            Tensor<float> output = worker.PeekOutput() as Tensor<float>;
            Tensor<float> outputCpu = output.ReadbackAndClone();
            // 4. 解析
            ParseYoloOutput(outputCpu);
            // Debug.Log("结束一轮推理！！！！！！");

            // 5. 清理
            inputTensor.Dispose();
            output.Dispose();
            outputCpu.Dispose();
            yield return new WaitForSeconds(0.03f);  // 每 0.03 秒推理一次(33FPS)
        }
    }

    void OnDisable()
    {
        worker?.Dispose();
    }

    void ParseYoloOutput(Tensor<float> outputCpu, float scoreThreshold = 0.7f)
    {
        detections.Clear();
        int numBoxes = outputCpu.shape[1];  // 第 2 维是 N（框的数量）

        for (int i = 0; i < numBoxes; i++)
        {
            float x1 = outputCpu[0, i, 0];
            float y1 = outputCpu[0, i, 1];
            float x2 = outputCpu[0, i, 2];
            float y2 = outputCpu[0, i, 3];
            float score = outputCpu[0, i, 4];
            int cls = (int)outputCpu[0, i, 5];

            if (score > scoreThreshold)
            {
                float w = x2 - x1;
                float h = y2 - y1;
                float cx = x1 + w / 2;
                float cy = y1 + h / 2;

                // 计算像素坐标 (注意 Unity 图像 y 坐标是从下到上的)
                int px = Mathf.Clamp(Mathf.RoundToInt(cx), 0, depthReadbackTexture.width - 1);
                int py = Mathf.Clamp(Mathf.RoundToInt(cy), 0, depthReadbackTexture.height - 1);
                float grayscale = depthReadbackTexture.GetPixel(px, py).r;
                float depth = (1.0f - grayscale) * depthCamera.farClipPlane;
                // Debug.Log($"depth: {depth}");

                // 将图像坐标 + 深度 转为相机坐标
                Vector3 screenPoint = new Vector3(cx, renderTexture.height - cy, depth);  // 注意 Y 翻转
                Vector3 worldPos = captureCamera.ScreenToWorldPoint(screenPoint);
                detections.Add(new Detection
                {
                    classID = cls,
                    score = score,
                    bbox = new Rect(x1, y1, w, h),
                    centerPosition = worldPos
                });
                // Debug.Log($"Detected: Class {cls}, Position: {worldPos}");
                // Debug.Log($"OBJ_DETECTION: Class= {cls}, Score= {score:F2}, BBox: x={cx:F1}, y={cy:F1}, w={w:F1}, h={h:F1}");
            }
        }
    }
}
