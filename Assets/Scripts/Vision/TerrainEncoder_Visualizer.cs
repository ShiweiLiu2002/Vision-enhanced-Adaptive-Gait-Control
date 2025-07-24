using System.Collections;
using UnityEngine;
using Unity.Sentis;

public class TerrainClassification_inference : MonoBehaviour
{
    public RenderTexture renderTexture;           // RenderTexture source for inference
    public ModelAsset modelAsset;                 // ONNX model to be loaded
    public string[] classNames = { "GrassLand", "Ice", "Mud", "StoneFloor", "WoodFloor" };

    public bool enableLogging = true;             // If true, will print result to console

    private Texture2D inputTexture;
    private Worker worker;
    private Model model;

    void Start()
    {
        // 1. Load the original model
        Model sourceModel = ModelLoader.Load(modelAsset);

        // 2. Create a FunctionalGraph and add Softmax to output
        FunctionalGraph graph = new FunctionalGraph();
        FunctionalTensor[] inputs = graph.AddInputs(sourceModel);
        FunctionalTensor[] outputs = Functional.Forward(sourceModel, inputs);
        FunctionalTensor softmax = Functional.Softmax(outputs[0], dim: 1); // Apply Softmax on class dimension

        // 3. Compile new model
        Model modelWithSoftmax = graph.Compile(softmax);

        // 4. Create inference worker
        worker = new Worker(modelWithSoftmax, BackendType.GPUCompute);

        // 5. Prepare input texture
        inputTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        // 6. Start inference loop
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
                Debug.Log($"Terrain classification result: {classNames[maxIdx]} (confidence: {maxProb:F2})");
            }

            inputTensor.Dispose();
            outputTensor.Dispose();
            outputCpu.Dispose();

            yield return new WaitForSeconds(0.1f);  // Run inference every 0.1 seconds
        }
    }

    void OnDisable()
    {
        worker?.Dispose();
    }

    Tensor<float> NormalizeTensor(Tensor<float> input)
    {
        // Ensure input shape is 1x3xHxW
        if (input.shape[0] != 1 || input.shape[1] != 3)
        {
            Debug.LogError("Input tensor shape must be 1x3xHxW");
            return input;
        }

        float[] mean = { 0.5f, 0.5f, 0.5f };
        float[] std = { 0.5f, 0.5f, 0.5f };

        Tensor<float> output = new Tensor<float>(input.shape);

        int height = input.shape[2];
        int width = input.shape[3];

        for (int c = 0; c < 3; c++)
        {
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    float val = input[0, c, h, w];
                    float norm = (val - mean[c]) / std[c];
                    output[0, c, h, w] = norm;
                }
            }
        }

        return output;
    }
}
