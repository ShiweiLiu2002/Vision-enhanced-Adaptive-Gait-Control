using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Detection_Visualizer : MonoBehaviour
{
    public Detection_inference inferenceScript;  // 拖入推理脚本所在的 GameObject
    public RenderTexture sourceTexture;       // 摄像头渲染图
    public RawImage displayImage;   // 用于显示最终图像
    private Texture2D drawTexture;
    private RenderTexture outputTexture;

    void Start()
    {
        outputTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.RGB565);
        drawTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGB24, false);
        if (displayImage != null)
        {
            displayImage.texture = outputTexture;
        }
    }

    void Update()
    {
        if (inferenceScript == null || inferenceScript.detections == null)
            return;
        Graphics.Blit(sourceTexture, outputTexture);  // 拷贝原图

        RenderTexture.active = outputTexture;
        drawTexture.ReadPixels(new Rect(0, 0, outputTexture.width, outputTexture.height), 0, 0);
        drawTexture.Apply();

        List<Detection> detections = inferenceScript.detections;
        foreach (var det in detections)
        {
            DrawBox(drawTexture, det.bbox, GetColorForClass(det.classID), $"{det.classID} ({det.score:F2})");
        }

        drawTexture.Apply();
        Graphics.Blit(drawTexture, outputTexture);
        RenderTexture.active = null;
    }

    // 可视化接口
    public RenderTexture GetVisualizedOutput()
    {
        Graphics.Blit(drawTexture, outputTexture);
        return outputTexture;
    }

    // 绘制检测框
    void DrawBox(Texture2D tex, Rect box, Color color, string label)
    {
        int x = Mathf.RoundToInt(box.x);
        int y = Mathf.RoundToInt(tex.height - box.y - box.height);
        int w = Mathf.RoundToInt(box.width);
        int h = Mathf.RoundToInt(box.height);

        for (int i = 0; i < w; i++)
        {
            tex.SetPixel(x + i, y, color);
            tex.SetPixel(x + i, y + h, color);
        }
        for (int j = 0; j < h; j++)
        {
            tex.SetPixel(x, y + j, color);
            tex.SetPixel(x + w, y + j, color);
        }

        // 可以扩展用 GUI 绘制 label（需额外挂载到 UI Canvas）
    }

    // 不同类别显示不同颜色
    Color GetColorForClass(int classID)
    {
        Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
        return colors[classID % colors.Length];
    }
}
