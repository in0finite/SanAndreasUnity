using System;

namespace UnityEngine.Rendering.Universal
{
    public class SunShaftsRenderFeature : ScriptableRendererFeature
    {
        SunShaftsPass sunShaftsPass;
        
        public Shader sunShaftsPS;

        public override void Create()
        {
            if(sunShaftsPS == null)
               sunShaftsPS = Shader.Find("Hidden/Universal Render Pipeline/SunShaftsComposite");

            sunShaftsPass = new SunShaftsPass(RenderPassEvent.BeforeRenderingPostProcessing, sunShaftsPS);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            sunShaftsPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(sunShaftsPass);
        }
    }
}
