using System.Collections;
using UnityEngine;
using Unity.Sentis;

public class TerrainClassification_inference : MonoBehaviour
{
    public RenderTexture renderTexture;           // RenderTexture source for inference
    public ModelAsset modelAsset;                 // ONNX model to be loaded
    public string[] classNames = { "GrassLand", "Ice", "Mud", "Rock", "Woodfloor" };

    public bool enableLogging = true;             // If true, will print result to console

    private Texture2D inputTexture;
    private Worker worker;
    private Model model;
    public float[] LatestLogits { get; private set; } = new float[5];  // original output（no softmax）


    void Start()
    {
        model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, BackendType.GPUCompute);
        inputTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        StartCoroutine(RunInferenceLoop());
    }

    IEnumerator RunInferenceLoop()
    {
        while (true)
        {
            // 1. Read RenderTexture into Texture2D
            RenderTexture.active = renderTexture;
            inputTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            inputTexture.Apply();
            RenderTexture.active = null;

            // 2. Convert texture to Tensor (1,3,H,W)
            Tensor<float> inputTensor = TextureConverter.ToTensor(inputTexture, channels: 3);

            // 3. Run inference
            worker.Schedule(inputTensor);
            Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

            // 4. Read output and get max class
            Tensor<float> outputCpu = outputTensor.ReadbackAndClone();
            for (int i = 0; i < classNames.Length; i++)
            {
                LatestLogits[i] = outputCpu[0, i];
            }
            float maxProb = float.MinValue;
            int maxIdx = -1;
            for (int i = 0; i < classNames.Length; i++)
            {
                float prob = outputCpu[0, i];
                if (prob > maxProb)
                {
                    maxProb = prob;
                    maxIdx = i;
                }
            }

            if (enableLogging)
            {
                Debug.Log($"Terrain classification result: {classNames[maxIdx]} (Network output: {maxProb:F2})");
            }

            inputTensor.Dispose();
            outputTensor.Dispose();
            outputCpu.Dispose();

            yield return new WaitForSeconds(0.2f);  // Run inference every 0.2 seconds
        }
    }

    void OnDisable()
    {
        worker?.Dispose();
    }
}
