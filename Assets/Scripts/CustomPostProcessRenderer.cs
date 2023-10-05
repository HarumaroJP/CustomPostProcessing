using CustomPostProcessing;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CustomPostProcessRenderer : ScriptableRendererFeature
{
    [SerializeField] private Shader customPostProcessShader;
    [SerializeField] private Shader colorTransitionShader;
    [SerializeField] private Shader monochromeShader;

    private CustomPostProcessPass customPass;
    private ColorTransitionPass colorTransitionPass;
    private MonochromePass monochromePass;

    public override void Create()
    {
        customPass = new CustomPostProcessPass(customPostProcessShader);
        colorTransitionPass = new ColorTransitionPass(colorTransitionShader);
        monochromePass = new MonochromePass(monochromeShader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(customPass);
        renderer.EnqueuePass(colorTransitionPass);
        renderer.EnqueuePass(monochromePass);
    }
}