using System;

namespace UnityEngine.Rendering.Universal
{
	public enum SunShaftsResolution
    {
        Low = 0,
        Normal = 1,
    }

    public enum ShaftsScreenBlendMode
    {
        Screen = 0,
        Add = 1,
    }
	
    [Serializable, VolumeComponentMenu("Post-processing/Sun Shafts")]
    public sealed class SunShafts : VolumeComponent, IPostProcessComponent
    {
        public SunShaftsResolutionParameter resolution = new SunShaftsResolutionParameter { value = SunShaftsResolution.Normal };
        public ShaftsScreenBlendModeParameter screenBlendMode = new ShaftsScreenBlendModeParameter { value = ShaftsScreenBlendMode.Add };

        public ClampedIntParameter radialBlurIterations = new ClampedIntParameter(0, 0, 2);
        public FloatParameter sunShaftBlurRadius = new FloatParameter(2.5f);

        [Range(0, 1)]
        public FloatParameter maxRadius = new FloatParameter(0.75f);

        public BoolParameter lastBlur = new BoolParameter(false);

        public bool IsActive() => radialBlurIterations.value > 0;

        public bool IsTileCompatible() => false;
    }

    [Serializable]
    public sealed class SunShaftsResolutionParameter : VolumeParameter<SunShaftsResolution> { }

    [Serializable]
    public sealed class ShaftsScreenBlendModeParameter : VolumeParameter<ShaftsScreenBlendMode> { }
}
