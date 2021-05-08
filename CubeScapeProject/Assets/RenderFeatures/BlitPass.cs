using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class BlitPass : ScriptableRenderPass
{
    public enum RenderTarget
    {
        Color,
        RenderTexture
    }

    public Material blitMaterial = null;
    public int blitShaderPassIndex = 0;
    public FilterMode filterMode { get; set; }

    private RenderTargetIdentifier source { get; set; }
    RenderTargetHandle destination { get; set; }

    RenderTargetHandle m_TemporaryColorTexture;
    string m_ProfilerTag;

    public BlitPass(RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag)
    {
        this.renderPassEvent = renderPassEvent;
        this.blitMaterial = blitMaterial;
        this.blitShaderPassIndex = blitShaderPassIndex;

        m_ProfilerTag = tag;
        m_TemporaryColorTexture.Init("_TemporaryColorTexture");
    }

    public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination)
    {
        this.source = source;
        this.destination = destination;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

        RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
        opaqueDesc.depthBufferBits = 0;

        if(destination == RenderTargetHandle.CameraTarget)
        {
            cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);
            Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, blitShaderPassIndex);
            Blit(cmd, m_TemporaryColorTexture.Identifier(), source);
        }
        else
        {
            Blit(cmd, source, destination.Identifier(), blitMaterial, blitShaderPassIndex);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        if(destination == RenderTargetHandle.CameraTarget)
        {
            cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
        }
    }

}
