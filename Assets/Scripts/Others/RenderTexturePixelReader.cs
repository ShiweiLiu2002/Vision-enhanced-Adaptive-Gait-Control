using UnityEngine;

public class RenderTexturePixelReader : MonoBehaviour
{
    [Tooltip("待读取的 RenderTexture")]
    public RenderTexture renderTexture;

    [Tooltip("读取频率（秒）")]
    public float readInterval = 1.0f;

    private Texture2D tempTexture;
    private float timer = 0f;

    void Start()
    {
        if (renderTexture == null)
        {
            Debug.LogError("[RenderTexturePixelReader] RenderTexture 未设置！");
            enabled = false;
            return;
        }

        // 创建与 RenderTexture 分辨率相同的 Texture2D
        tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= readInterval)
        {
            timer = 0f;
            ReadCenterPixel();
        }
    }

    private void ReadCenterPixel()
    {
        // 将 RenderTexture 设置为激活的渲染目标
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        // 从 RenderTexture 读取到 tempTexture 中
        tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tempTexture.Apply();

        // 恢复原渲染目标
        RenderTexture.active = currentRT;

        // 获取中心像素坐标
        int cx = renderTexture.width / 2;
        int cy = renderTexture.height / 2;

        Color centerColor = tempTexture.GetPixel(cx, cy);
        Debug.Log($"[RenderTexturePixelReader] 中心像素 RGB: ({centerColor.r:F3}, {centerColor.g:F3}, {centerColor.b:F3})");
    }

    private void OnDestroy()
    {
        if (tempTexture != null)
        {
            Destroy(tempTexture);
        }
    }
}
