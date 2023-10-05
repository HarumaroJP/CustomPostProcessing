using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostProcessing
{
    [System.Serializable]
    public class MonochromePass : ScriptableRenderPass
    {
        // Used to render from camera to post processings
        // back and forth, until we render the final image to
        // the camera
        private RenderTargetIdentifier source;
        private RenderTargetIdentifier destinationA;
        private RenderTargetIdentifier destinationB;
        private RenderTargetIdentifier latestDest;

        private readonly int temporaryRTIdA = Shader.PropertyToID("_TempRT");
        private readonly int temporaryRTIdB = Shader.PropertyToID("_TempRTB");
        private Material effectMaterial;

        public MonochromePass(Shader shader)
        {
            effectMaterial = CoreUtils.CreateEngineMaterial(shader);

            // Set the render pass event
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Grab the camera target descriptor. We will use this when creating a temporary render texture.
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            ScriptableRenderer renderer = renderingData.cameraData.renderer;
            source = renderer.cameraColorTarget;

            // 一時テクスチャを取得
            cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
            destinationA = new RenderTargetIdentifier(temporaryRTIdA);
            cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
            destinationB = new RenderTargetIdentifier(temporaryRTIdB);
        }

        // The actual execution of the pass. This is where custom rendering occurs.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // シーンビューだったら描画しない
            if (renderingData.cameraData.isSceneViewCamera)
                return;

            // カスタムエフェクトを取得
            VolumeStack stack = VolumeManager.instance.stack;
            var customEffect = stack.GetComponent<Monochrome>();

            // 無効だったらポストプロセスかけない
            if (!customEffect.IsActive())
                return;

            // CommandBufferを取得
            CommandBuffer cmd = CommandBufferPool.Get("Monochrome");
            cmd.Clear();

            // カメラを描画元として設定
            latestDest = source;

            RenderTargetIdentifier first = latestDest;
            RenderTargetIdentifier last = first == destinationA ? destinationB : destinationA;
            Blit(cmd, first, last, effectMaterial, 0);

            // Swap
            latestDest = last;

            // カメラに描画結果を反映
            Blit(cmd, latestDest, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // 一時テクスチャを開放
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(temporaryRTIdA);
            cmd.ReleaseTemporaryRT(temporaryRTIdB);
        }
    }
}