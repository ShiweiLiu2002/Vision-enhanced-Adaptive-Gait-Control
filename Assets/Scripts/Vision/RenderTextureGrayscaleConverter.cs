using UnityEngine;

[ExecuteAlways]
public class RenderTextureGrayscaleConverter : MonoBehaviour
{
    [Header("Source RGBA32 RenderTexture")]
    public RenderTexture sourceTexture;

    [Header("Converted Grayscale RenderTexture")]
    public RenderTexture grayscaleTexture;
    public Shader grayscaleShader;
    private Material grayscaleMaterial;
    
    void Start()
    {
        if (sourceTexture == null || grayscaleTexture == null)
        {
            Debug.LogError("RenderTextures not assigned.");
            enabled = false;
            return;
        }

        grayscaleMaterial = new Material(grayscaleShader);
    }

    void Update()
    {
        if (sourceTexture == null || grayscaleTexture == null || grayscaleMaterial == null)
            return;

        Graphics.Blit(sourceTexture, grayscaleTexture, grayscaleMaterial);
    }
}
