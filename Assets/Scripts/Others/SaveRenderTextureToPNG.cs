using UnityEngine;
using System.IO;

public class SaveRenderTextureToPNG : MonoBehaviour
{
    [Header("RenderTexture to capture")]
    public RenderTexture targetRenderTexture;

    [Header("Save Settings")]
    public string fileName = "CapturedImage";
    public string folderName = "SavedFrames";

    void Start()
    {
        if (targetRenderTexture == null)
        {
            Debug.LogError("No RenderTexture assigned!");
            return;
        }

        // 创建文件夹（如果不存在）
        string folderPath = Path.Combine(Application.dataPath, "..", folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 读取 RenderTexture 内容
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = targetRenderTexture;

        Texture2D image = new Texture2D(targetRenderTexture.width, targetRenderTexture.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, targetRenderTexture.width, targetRenderTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = currentRT;

        // 保存为 PNG
        byte[] bytes = image.EncodeToPNG();
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filePath = Path.Combine(folderPath, $"{fileName}_{timestamp}.png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Saved RenderTexture to: {filePath}");
    }
}
