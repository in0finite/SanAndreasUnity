using System;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal
{
    public class ReCopyFeature : ScriptableRendererFeature
    {
        XCopyDepthPass m_CopyDepthPass;
        XCopyColorPass m_CopyColorPass;

        Material copyDepthPassMaterial = null;
        Material samplingMaterial;

        RenderTargetHandle m_CameraDepthAttachment;
        RenderTargetHandle m_DepthTexture;

        RenderTargetHandle m_CameraColorAttachment;
        RenderTargetHandle m_ColorTexture;

        public override void Create()
        {
            m_CameraDepthAttachment.Init("_CameraDepthAttachment");
            m_DepthTexture.Init("_CameraDepthTexture");

            m_CameraColorAttachment.Init("_CameraColorTexture");
            m_ColorTexture.Init("_ColorTexture");

            copyDepthPassMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Universal Render Pipeline/CopyDepth"));
            samplingMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Universal Render Pipeline/Sampling"));

            m_CopyDepthPass = new XCopyDepthPass(RenderPassEvent.AfterRenderingTransparents, copyDepthPassMaterial);
            m_CopyColorPass = new XCopyColorPass(RenderPassEvent.AfterRenderingTransparents, samplingMaterial);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!Application.isPlaying)
                return;

            m_CopyDepthPass.Setup(m_CameraDepthAttachment, m_DepthTexture);
            renderer.EnqueuePass(m_CopyDepthPass);

            Downsampling downsamplingMethod = UniversalRenderPipeline.asset.opaqueDownsampling;
            m_CopyColorPass.Setup(m_CameraColorAttachment.Identifier(), m_ColorTexture, downsamplingMethod);
            renderer.EnqueuePass(m_CopyColorPass);
        }
    }
}
