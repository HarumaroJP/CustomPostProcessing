﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostProcessing
{
    [System.Serializable]
    public class ColorTransitionPass : ScriptableRenderPass
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
        private readonly int intensityId = Shader.PropertyToID("_Intensity");
        private readonly int speedId = Shader.PropertyToID("_Speed");
        private readonly int saturationId = Shader.PropertyToID("_Saturation");
        private readonly int brightnessId = Shader.PropertyToID("_Brightness");
        private Material effectMaterial;

        public ColorTransitionPass(Shader shader)
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
            var customEffect = stack.GetComponent<ColorTransition>();

            // 無効だったらポストプロセスかけない
            if (!customEffect.IsActive())
                return;

            // CommandBufferを取得
            CommandBuffer cmd = CommandBufferPool.Get("ColorTransition");
            cmd.Clear();

            // カメラを描画元として設定
            latestDest = source;

            effectMaterial.SetFloat(intensityId, customEffect.intensity.value);
            effectMaterial.SetFloat(saturationId, customEffect.saturation.value);
            effectMaterial.SetFloat(brightnessId, customEffect.brightness.value);
            effectMaterial.SetFloat(speedId, customEffect.speed.value);

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