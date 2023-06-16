
namespace UnityEngine.Rendering.Universal
{
    public class SunShaftsPass : ScriptableRenderPass
    {
        static readonly string k_RenderTag = "Sun Shafts";

        SunShafts m_SunShafts = null;
        Material sunShaftsMaterial = null;
        RenderTargetIdentifier currentTarget;

        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int TempTargetId = Shader.PropertyToID("Destination");

        public SunShaftsPass(RenderPassEvent evt, Shader sunShaftsPS)
        {
            renderPassEvent = evt;
            if (sunShaftsPS == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            sunShaftsMaterial = CoreUtils.CreateEngineMaterial(sunShaftsPS);
        }

        public bool UpdateSun(Camera cam, out Vector3 vSun)
        {
            Vector3 dir = RenderSettings.sun.transform.forward;
            Vector3 sunPos = cam.transform.position - dir * cam.farClipPlane;
            vSun = cam.WorldToViewportPoint(sunPos);

            return vSun.z >= 0f;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!renderingData.cameraData.postProcessEnabled) return;

            var stack = VolumeManager.instance.stack;
            m_SunShafts = stack.GetComponent<SunShafts>();
            if (m_SunShafts == null) { return; }
            if (!m_SunShafts.IsActive()) { return; }

            if (sunShaftsMaterial == null) return;

            var cmd = CommandBufferPool.Get(k_RenderTag);
            if (RenderSettings.sun == null)
            {
                return;
            }

            Vector3 vSun;
            bool positiveZ = UpdateSun(renderingData.cameraData.camera, out vSun);

            int divider = 4;
            if (m_SunShafts.resolution == SunShaftsResolution.Normal)
                divider = 2;

            if (!positiveZ)
            {
                return;
            }

            float sradius = m_SunShafts.sunShaftBlurRadius.value;

            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;

            var material = sunShaftsMaterial;

            var destination = currentTarget;

            cmd.SetGlobalTexture(MainTexId, destination);
            cmd.GetTemporaryRT(TempTargetId, desc.width, desc.height, 0, FilterMode.Bilinear, desc.colorFormat);
            
            cmd.Blit(destination, TempTargetId);

            int rtW = desc.width / divider;
            int rtH = desc.height / divider;

            RenderTexture lrColorB;
            RenderTexture lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, desc.colorFormat);

            // mask out everything except the skybox
            // we have 2 methods, one of which requires depth buffer support, the other one is just comparing images

            material.SetVector("_SunPosition", new Vector4(vSun.x, vSun.y, vSun.z, m_SunShafts.maxRadius.value));

            cmd.Blit(TempTargetId, lrDepthBuffer, material, 2);
            // paint a small black small border to get rid of clamping problems
            //DrawBorder(lrDepthBuffer, Color.clear);

            // radial blur:

            m_SunShafts.radialBlurIterations.value = Mathf.Clamp(m_SunShafts.radialBlurIterations.value, 1, 4);

            int iter = m_SunShafts.radialBlurIterations.value;

            float ofs = sradius * (1.0f / 768.0f);

            material.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            material.SetVector("_SunPosition", new Vector4(vSun.x, vSun.y, vSun.z, m_SunShafts.maxRadius.value));

            for (int it2 = 0; it2 < iter; it2++)
            {
                // each iteration takes 2 * 6 samples
                // we update _BlurRadius each time to cheaply get a very smooth look

                lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0, desc.colorFormat);
                cmd.Blit(lrDepthBuffer, lrColorB, material, 1);
                RenderTexture.ReleaseTemporary(lrDepthBuffer);
                ofs = sradius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
                material.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

                lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, desc.colorFormat);
                cmd.Blit(lrColorB, lrDepthBuffer, material, 1);
                RenderTexture.ReleaseTemporary(lrColorB);
                ofs = sradius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
                material.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            }

            if(m_SunShafts.lastBlur.value)
            {
                lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0, desc.colorFormat);
                cmd.Blit(lrDepthBuffer, lrColorB, material, 5);
                RenderTexture.ReleaseTemporary(lrDepthBuffer);

                lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, desc.colorFormat);
                cmd.Blit(lrColorB, lrDepthBuffer, material, 6);
                RenderTexture.ReleaseTemporary(lrColorB);
            }

            // put together:
            material.SetTexture("_ColorBuffer", lrDepthBuffer);
            cmd.Blit(TempTargetId, destination, material, (m_SunShafts.screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);

            RenderTexture.ReleaseTemporary(lrDepthBuffer);

            cmd.ReleaseTemporaryRT(TempTargetId);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Setup(in RenderTargetIdentifier target)
        {
            currentTarget = target;
        }

    }
}
