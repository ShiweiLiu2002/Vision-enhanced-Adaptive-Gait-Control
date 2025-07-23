using System.Collections;
using UnityEngine;
using Unity.Sentis;

public class TerrainClassification_inference : MonoBehaviour
{
    public RenderTexture renderTexture;           // 绑定你的RenderTexture
    public ModelAsset modelAsset;                 // 绑定你的best.onnx模型
    public string[] classNames = { "GrassLand", "Ice", "Mud", "StoneFloor", "WoodFloor" };

    private Texture2D inputTexture;
    private Worker worker;
    private Model model;

    void Start()
    {
        // 1. 加载原始模型
        Model sourceModel = ModelLoader.Load(modelAsset);

        // 2. 构建功能图，添加 Softmax 层
        FunctionalGraph graph = new FunctionalGraph();
        FunctionalTensor[] inputs = graph.AddInputs(sourceModel);             // 添加输入节点
        FunctionalTensor[] outputs = Functional.Forward(sourceModel, inputs); // 原始模型输出
        FunctionalTensor softmax = Functional.Softmax(outputs[0], dim: 1);    // 在类别维度添加 softmax

        // 3. 编译新模型
        Model modelWithSoftmax = graph.Compile(softmax);

        // 4. 创建推理引擎
        worker = new Worker(modelWithSoftmax, BackendType.GPUCompute);

        // 5. 初始化输入纹理
        inputTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        // 6. 启动推理循环
        StartCoroutine(RunInferenceLoop());
    }


    IEnumerator RunInferenceLoop()
    {
        while (true)
        {
            // 1. 读取RenderTexture为Texture2D
            RenderTexture.active = renderTexture;
            inputTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.width), 0, 0);
            inputTexture.Apply();
            RenderTexture.active = null;

            // 2. 转换为Tensor（1,3,640,640）
            Tensor<float> inputTensor = TextureConverter.ToTensor(inputTexture, channels: 3);
            // 3. 执行推理
            worker.Schedule(inputTensor);
            Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

            // 4. 获取最大概率的类别
            Tensor<float> outputCpu = outputTensor.ReadbackAndClone();  // 读取到CPU
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

            Debug.Log($"地形识别结果: {classNames[maxIdx]} (置信度: {maxProb:F2})");

            inputTensor.Dispose();
            outputTensor.Dispose();
            outputCpu.Dispose();

            yield return new WaitForSeconds(0.1f);  // 每0.1秒执行一次推理
        }
    }

    void OnDisable()
    {
        worker?.Dispose();
    }

    Tensor<float> NormalizeTensor(Tensor<float> input)
    {
        // 确保输入为 1x3xHxW
        if (input.shape[0] != 1 || input.shape[1] != 3)
        {
            Debug.LogError("输入 Tensor 的形状应为 1x3xHxW");
            return input;
        }

        float[] mean = { 0.5f, 0.5f, 0.5f };
        float[] std = { 0.5f, 0.5f, 0.5f };

        Tensor<float> output = new Tensor<float>(input.shape);

        int height = input.shape[2];
        int width = input.shape[3];

        for (int c = 0; c < 3; c++) // 通道
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