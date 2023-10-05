using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostProcessing
{
    [Serializable, VolumeComponentMenu("Custom/Monochrome")]
    public class Monochrome : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter enabled = new BoolParameter(false, overrideState: true);

        public bool IsActive() => enabled.value;

        public bool IsTileCompatible() => true;
    }
}