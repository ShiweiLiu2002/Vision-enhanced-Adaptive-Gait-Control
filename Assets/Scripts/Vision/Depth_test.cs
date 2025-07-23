using UnityEngine;
using System.Collections;

public class DepthReader : MonoBehaviour
{
    public Camera depthCamera;
    public RenderTexture depthRT;
    private Texture2D depthTexture;

    void Start()
    {
        depthTexture = new Texture2D(depthRT.width, depthRT.height, TextureFormat.RGB24, false);
        StartCoroutine(ReadDepthLoop());
    }

    IEnumerator ReadDepthLoop()
    {
        while (true)
        {
            RenderTexture.active = depthRT;
            depthTexture.ReadPixels(new Rect(0, 0, depthRT.width, depthRT.height), 0, 0);
            depthTexture.Apply();
            RenderTexture.active = null;

            Color centerPixel = depthTexture.GetPixel(depthRT.width / 2, depthRT.height / 2);
            float grayscale = centerPixel.r;
            float depth = (1.0f - grayscale) * depthCamera.farClipPlane;

            Debug.Log($"Center depth: {depth:F3} meters");

            yield return new WaitForSeconds(0.1f);
        }
    }
}
