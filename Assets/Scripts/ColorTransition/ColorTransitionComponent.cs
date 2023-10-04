using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostProcessing
{
    [Serializable, VolumeComponentMenu("Custom/ColorTransition")]
    public class ColorTransitionComponent : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter intensity = new ClampedFloatParameter(value: 0, min: 0, max: 1, overrideState: true);
        public FloatParameter speed = new FloatParameter(0, true);

        public bool IsActive() => intensity.value > 0;

        public bool IsTileCompatible() => true;
    }
}