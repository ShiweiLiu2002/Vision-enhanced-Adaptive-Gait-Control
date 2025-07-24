using UnityEngine;
using UnityEngine.UI;

public class Depth_Visualizer : MonoBehaviour
{
    [Header("RenderTexture to display (Depth)")]
    public RenderTexture renderTexture;

    [Header("RawImage on UI")]
    public RawImage rawImage;

    [Header("Refresh frequency (in frames)")]
    public int refreshEveryNFrames = 1;

    private Texture2D tempTexture;
    private int frameCounter;

    void Start()
    {
        if (renderTexture == null || rawImage == null)
        {
            Debug.LogError("RenderTexture or RawImage is not assigned!");
            enabled = false;
            return;
        }

        tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        rawImage.texture = tempTexture;
    }

    void Update()
    {
        frameCounter++;
        if (frameCounter % refreshEveryNFrames != 0) return;

        RenderTexture.active = renderTexture;
        tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tempTexture.Apply();
        RenderTexture.active = null;
    }
}
