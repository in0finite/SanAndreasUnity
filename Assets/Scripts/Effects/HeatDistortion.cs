using UnityEngine;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(Camera))]
public class HeatDistortion : PostEffectsBase
{
    private Material heatMaterial;

    public Texture2D heatTexture;
    public Shader heatShader;

    [Range(-1, 1)]
    public float m_strength = 1;

    public override bool CheckResources()
    {
        CheckSupport(true);

        heatMaterial = CheckShaderAndCreateMaterial(heatShader, heatMaterial);

        heatMaterial.SetTexture("_DistortionTex", heatTexture);

        if (!isSupported)
            ReportAutoDisable();

        return isSupported;
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!CheckResources())
        {
            Graphics.Blit(source, destination);
            return;
        }

        heatMaterial.SetFloat("_Strength", m_strength);

        Graphics.Blit(source, destination, heatMaterial);
    }
}