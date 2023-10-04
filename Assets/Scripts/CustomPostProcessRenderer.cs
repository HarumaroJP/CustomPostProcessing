using CustomPostProcessing;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CustomPostProcessRenderer : ScriptableRendererFeature
{
    [SerializeField] private Shader customPostProcessShader;
    [SerializeField] private Shader colorTransitionShader;
    private CustomPostProcessPass customPass;
    private ColorTransitionPass colorTransitionPass;

    public override void Create()
    {
        customPass = new CustomPostProcessPass(customPostProcessShader);
        colorTransitionPass = new ColorTransitionPass(colorTransitionShader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(customPass);
        renderer.EnqueuePass(colorTransitionPass);
    }
}