---
title: "Scriptable Render Pipeline (SRP)"
date: "2026-01-30"
tags: [Unity, SRP, 渲染管线, URP, HDRP]
---

# Scriptable Render Pipeline (SRP)

## 问题描述
Unity中的Scriptable Render Pipeline (SRP)是什么？有哪些类型？如何使用和定制？

## 回答

### 1. 问题分析

**SRP的基本概念**：
Scriptable Render Pipeline (SRP)是Unity 2018.1引入的可脚本化渲染管线系统，允许开发者完全控制Unity的渲染过程。

**SRP的核心优势**：
- **高度可定制**：开发者可以完全控制渲染流程的每一个环节
- **性能优化**：针对不同平台和硬件进行专门优化
- **跨平台一致性**：在不同平台上实现一致的视觉效果
- **现代化渲染**：支持PBR、后处理、GPU Instancing等现代渲染技术

**SRP的类型**：

| 类型 | 全称 | 特点 | 适用场景 |
|------|------|------|----------|
| **URP** | Universal Render Pipeline | 高性能、跨平台、轻量级 | 移动游戏、VR/AR、2D游戏、快速迭代 |
| **HDRP** | High Definition Render Pipeline | 高质量、真实感、功能丰富 | 主机游戏、PC游戏、高质量视觉效果 |
| **自定义SRP** | Custom SRP | 完全自定义、高度灵活 | 特殊渲染需求、技术演示、教育研究 |

**SRP的工作原理**：
1. 渲染管线被拆分为可配置的脚本
2. 开发者可以控制相机渲染、光照计算、后处理等所有环节
3. 渲染过程通过ScriptableObject进行配置
4. 利用C# Job System和Burst Compiler优化性能

---

### 2. 案例演示

#### 2.1 自定义基础SRP实现

```csharp
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField] private bool useDynamicBatching = true;
    [SerializeField] private bool useGPUInstancing = true;
    [SerializeField] private bool useSRPBatcher = true;
    [SerializeField] private ShadowSettings shadows = default;
    
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(
            useDynamicBatching, useGPUInstancing, useSRPBatcher, shadows
        );
    }
    
    [System.Serializable]
    public class ShadowSettings
    {
        [Min(0.001f)] public float maxDistance = 100f;
        [Range(1, 4)] public int cascadeCount = 2;
        [Range(0f, 1f)] public float cascadeRatio1 = 0.1f;
        [Range(0f, 1f)] public float cascadeRatio2 = 0.25f;
        [Range(0f, 1f)] public float cascadeRatio3 = 0.5f;
    }
}

public class CustomRenderPipeline : RenderPipeline
{
    private bool useDynamicBatching;
    private bool useGPUInstancing;
    private bool useSRPBatcher;
    private CustomRenderPipelineAsset.ShadowSettings shadowSettings;
    
    private CameraRenderer renderer = new CameraRenderer();
    
    public CustomRenderPipeline(
        bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher,
        CustomRenderPipelineAsset.ShadowSettings shadowSettings
    )
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.useSRPBatcher = useSRPBatcher;
        this.shadowSettings = shadowSettings;
        
        // 启用SRP批处理
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        // 设置默认着色器
        Shader.globalRenderPipeline = "CustomRenderPipeline";
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            renderer.Render(
                context, camera, useDynamicBatching, useGPUInstancing, shadowSettings
            );
        }
    }
}

public class CameraRenderer
{
    private const string bufferName = "Render Camera";
    private CommandBuffer buffer = new CommandBuffer { name = bufferName };
    private ScriptableRenderContext context;
    private Camera camera;
    private CullingResults cullingResults;
    
    private static int frameBufferId = Shader.PropertyToID("_CameraFrameBuffer");
    
    public void Render(
        ScriptableRenderContext context, Camera camera, bool useDynamicBatching, 
        bool useGPUInstancing, CustomRenderPipelineAsset.ShadowSettings shadowSettings
    )
    {
        this.context = context;
        this.camera = camera;
        
        PrepareBuffer();
        
        if (!Cull())
        {
            return;
        }
        
        Setup();
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }
    
    private void PrepareBuffer()
    {
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
    }
    
    private bool Cull()
    {
        ScriptableCullingParameters cullingParams;
        if (!camera.TryGetCullingParameters(out cullingParams))
        {
            return false;
        }
        
        cullingResults = context.Cull(ref cullingParams);
        return true;
    }
    
    private void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags clearFlags = camera.clearFlags;
        buffer.ClearRenderTarget(
            clearFlags <= CameraClearFlags.Depth, 
            clearFlags == CameraClearFlags.Color, 
            clearFlags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear
        );
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }
    
    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettings = new DrawingSettings(
            ShaderTagId.opacity, sortingSettings
        )
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        
        context.DrawSkybox(camera);
        
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }
    
    private void DrawUnsupportedShaders()
    {
        var sortingSettings = new SortingSettings(camera);
        var drawingSettings = new DrawingSettings(
            new ShaderTagId("SRPDefaultUnlit"), sortingSettings
        );
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }
    
    private void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }
    
    private void Submit()
    {
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        context.Submit();
    }
    
    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
}
```

#### 2.2 URP基本配置和使用

**步骤1：安装URP包**
1. 打开Package Manager
2. 搜索"Universal RP"
3. 点击安装

**步骤2：创建URP配置**

```csharp
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class URPConfig : MonoBehaviour
{
    [SerializeField] private UniversalRenderPipelineAsset urpAsset;
    
    private void Start()
    {
        // 切换到URP
        GraphicsSettings.renderPipelineAsset = urpAsset;
        
        // 配置URP质量设置
        ConfigureURPSettings();
    }
    
    private void ConfigureURPSettings()
    {
        // 获取URP质量设置
        UniversalRenderPipelineQualitySettings qualitySettings = 
            UniversalRenderPipelineAsset.currentQualitySettings;
        
        if (qualitySettings != null)
        {
            // 配置质量级别
            for (int i = 0; i < qualitySettings.qualityLevels.Length; i++)
            {
                var level = qualitySettings.qualityLevels[i];
                
                // 根据质量级别设置不同的参数
                switch (i)
                {
                    case 0: // 低质量
                        level.renderScale = 0.5f;
                        level.msaaSampleCount = 1;
                        break;
                    case 1: // 中质量
                        level.renderScale = 0.75f;
                        level.msaaSampleCount = 2;
                        break;
                    case 2: // 高质量
                        level.renderScale = 1.0f;
                        level.msaaSampleCount = 4;
                        break;
                }
            }
        }
    }
}
```

**步骤3：URP后处理配置**

```csharp
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class PostProcessingConfig : MonoBehaviour
{
    [SerializeField] private VolumeProfile postProcessProfile;
    
    private void Start()
    {
        // 创建后处理Volume
        Volume volume = gameObject.AddComponent<Volume>();
        volume.profile = postProcessProfile;
        volume.isGlobal = true;
        
        // 配置后处理效果
        ConfigurePostProcessEffects();
    }
    
    private void ConfigurePostProcessEffects()
    {
        if (postProcessProfile != null)
        {
            // 获取并配置Bloom效果
            if (postProcessProfile.TryGet(out Bloom bloom))
            {
                bloom.intensity.value = 1.5f;
                bloom.threshold.value = 0.8f;
                bloom.softKnee.value = 0.5f;
                bloom.clamp.value = 65535f;
                bloom.diffusion.value = 7f;
                bloom.anamorphicRatio.value = 1f;
                bloom.color.value = Color.white;
                bloom.fastMode.value = false;
                bloom.active = true;
            }
            
            // 获取并配置Depth of Field效果
            if (postProcessProfile.TryGet(out DepthOfField dof))
            {
                dof.focusDistance.value = 10f;
                dof.aperture.value = 0.5f;
                dof.focalLength.value = 50f;
                dof.active = true;
            }
        }
    }
}
```

#### 2.3 HDRP高级光照配置

```csharp
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class HDRPLightingConfig : MonoBehaviour
{
    [SerializeField] private HDAdditionalLightData sunLight;
    [SerializeField] private VolumeProfile hdrpProfile;
    
    private void Start()
    {
        ConfigureSunLight();
        ConfigureAtmosphericEffects();
    }
    
    private void ConfigureSunLight()
    {
        if (sunLight != null)
        {
            // 配置太阳光照
            sunLight.intensity.value = 100000f;
            sunLight.color.value = new Color(1, 0.95f, 0.9f);
            sunLight.angularDiameter.value = 0.53f; // 太阳实际角度
            sunLight.enableShadows = true;
            sunLight.shadowResolution = ShadowResolution.VeryHigh;
            sunLight.shadowDimmer.value = 0.8f;
        }
    }
    
    private void ConfigureAtmosphericEffects()
    {
        if (hdrpProfile != null)
        {
            // 配置大气效果
            if (hdrpProfile.TryGet(out AtmosphericScattering atmosphere))
            {
                atmosphere.sunSize.value = 0.04675f;
                atmosphere.sunSizeConvergence.value = 5.2f;
                atmosphere.skyTint.value = new Color(0.5f, 0.6f, 0.7f);
                atmosphere.groundTint.value = new Color(0.3f, 0.3f, 0.3f);
                atmosphere.multiScatteringFactor.value = 0.5f;
                atmosphere.active = true;
            }
            
            // 配置体积云
            if (hdrpProfile.TryGet(out VolumetricClouds clouds))
            {
                clouds.cloudType.value = VolumetricClouds.CloudType.Dynamic;
                clouds.densityMultiplier.value = 1f;
                clouds.coverage.value = 0.5f;
                clouds.layerScale.value = 1000f;
                clouds.active = true;
            }
        }
    }
}
```

#### 2.4 自定义SRP渲染特性

```csharp
using UnityEngine;
using UnityEngine.Rendering;

// 自定义渲染通道
public class CustomRenderPass : ScriptableRenderPass
{
    private RenderTargetIdentifier source;
    private RenderTargetHandle destination;
    
    private Material blitMaterial;
    private float intensity;
    
    public CustomRenderPass(Material material, float intensity)
    {
        this.blitMaterial = material;
        this.intensity = intensity;
        
        // 设置渲染时机
        renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }
    
    public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination)
    {
        this.source = source;
        this.destination = destination;
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("Custom Render Pass");
        
        // 设置材质参数
        blitMaterial.SetFloat("_Intensity", intensity);
        
        // 执行blit操作
        if (destination == RenderTargetHandle.CameraTarget)
        {
            Blit(cmd, source, source, blitMaterial);
        }
        else
        {
            Blit(cmd, source, destination.Identifier(), blitMaterial);
        }
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
    
    public override void FrameCleanup(CommandBuffer cmd)
    {
        if (destination != RenderTargetHandle.CameraTarget)
        {
            cmd.ReleaseTemporaryRT(destination.id);
        }
    }
}

// 自定义SRP特性
[System.Serializable]
public class CustomRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private Material blitMaterial;
    [SerializeField] private float intensity = 1.0f;
    
    private CustomRenderPass renderPass;
    private RenderTargetHandle renderTextureHandle;
    
    public override void Create()
    {
        renderPass = new CustomRenderPass(blitMaterial, intensity);
        renderTextureHandle.Init("CustomRenderTexture");
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderPass.Setup(renderer.cameraColorTarget, renderTextureHandle);
        renderer.EnqueuePass(renderPass);
    }
}
```

---

### 3. 注意事项

#### 3.1 常见问题与解决方案

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| **材质不兼容** | 着色器使用了内置渲染管线的特性 | 转换为URP/HDRP兼容的着色器，使用Shader Graph |
| **性能下降** | 配置不当，开启了过多昂贵特性 | 逐步添加特性并使用Profiler监控性能 |
| **平台兼容性** | 某些特性在特定平台不支持 | 使用平台特定的质量设置，禁用不支持的特性 |
| **内存占用高** | 后处理和纹理分辨率过高 | 降低纹理分辨率，使用压缩格式，优化后处理设置 |
| **光照计算错误** | 光照设置与SRP不匹配 | 重新配置光照，使用SRP兼容的光照组件 |

#### 3.2 性能优化建议

**URP性能优化**：
```csharp
public class URPOptimizer : MonoBehaviour
{
    private void Start()
    {
        // 启用SRP Batcher
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        
        // 启用GPU Instancing
        QualitySettings.realtimeGICPUUsage = 0;
        
        // 优化阴影
        if (UniversalRenderPipeline.asset != null)
        {
            var urpAsset = UniversalRenderPipeline.asset;
            urpAsset.shadowDistance = 50f;
            urpAsset.shadowmapResolution = ShadowmapSize.Medium;
            urpAsset.cascade2Split = 0.25f;
            urpAsset.cascade4Split = new Vector3(0.1f, 0.35f, 0.65f);
        }
        
        // 优化后处理
        OptimizePostProcessing();
    }
    
    private void OptimizePostProcessing()
    {
        // 动态调整后处理质量
        if (SystemInfo.graphicsMemorySize < 2048)
        {
            // 低显存设备
            QualitySettings.SetQualityLevel(0);
        }
        else if (SystemInfo.graphicsMemorySize < 4096)
        {
            // 中显存设备
            QualitySettings.SetQualityLevel(1);
        }
        else
        {
            // 高显存设备
            QualitySettings.SetQualityLevel(2);
        }
    }
}
```

**HDRP性能优化**：
```csharp
public class HDRPOptimizer : MonoBehaviour
{
    private void Start()
    {
        // 配置HDRP质量设置
        if (HighDefinitionRenderPipeline.asset != null)
        {
            var hdrpAsset = HighDefinitionRenderPipeline.asset;
            
            // 根据设备性能调整设置
            if (SystemInfo.processorCount < 4 || SystemInfo.graphicsMemorySize < 4096)
            {
                // 低端设备
                hdrpAsset.renderPipelineSettings.lightingQuality = LightingQualityLevel.Medium;
                hdrpAsset.renderPipelineSettings.screenSpaceReflectionSettings.quality = ScreenSpaceReflectionQualityLevel.Low;
                hdrpAsset.renderPipelineSettings.rayTracingQuality = RayTracingQualityLevel.Off;
            }
            else
            {
                // 高端设备
                hdrpAsset.renderPipelineSettings.lightingQuality = LightingQualityLevel.High;
                hdrpAsset.renderPipelineSettings.screenSpaceReflectionSettings.quality = ScreenSpaceReflectionQualityLevel.High;
                hdrpAsset.renderPipelineSettings.rayTracingQuality = RayTracingQualityLevel.Medium;
            }
        }
    }
}
```

#### 3.3 最佳实践

1. **项目规划阶段选择合适的SRP**：
   - 移动游戏和快速迭代选择URP
   - 高质量视觉效果选择HDRP
   - 特殊渲染需求选择自定义SRP

2. **逐步添加渲染特性**：
   - 从基础渲染开始，逐步添加光照、后处理等特性
   - 每次添加后使用Profiler检查性能影响

3. **使用Shader Graph**：
   - 利用节点式着色器编辑器快速创建SRP兼容的着色器
   - 避免手写复杂的着色器代码

4. **平台特定优化**：
   - 为不同平台创建专门的质量设置
   - 利用平台特定的渲染API（如Metal、Vulkan、DirectX 12）

5. **测试和验证**：
   - 在目标硬件上进行实际测试
   - 使用Frame Debugger分析渲染性能
   - 监控内存使用和GPU占用

6. **团队协作**：
   - 建立SRP配置的版本控制流程
   - 制定材质和光照的美术规范
   - 定期审查渲染性能

#### 3.4 迁移和转换

**从内置渲染管线迁移到SRP**：
1. **备份项目**：在迁移前创建完整备份
2. **选择合适的SRP**：根据项目需求选择URP或HDRP
3. **安装SRP包**：通过Package Manager安装相应的包
4. **创建配置**：创建并配置SRP Asset
5. **转换材质**：使用Material Upgrader转换现有材质
6. **更新代码**：修改与渲染相关的代码
7. **测试和调整**：在各平台上测试并优化

**材质转换工具**：
```csharp
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public class MaterialConverter : EditorWindow
{
    [MenuItem("Tools/Convert Materials to URP")]
    public static void ShowWindow()
    {
        GetWindow<MaterialConverter>("Material Converter");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Convert Materials to URP", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Convert Selected Materials"))
        {
            ConvertSelectedMaterials();
        }
        
        if (GUILayout.Button("Convert All Materials"))
        {
            ConvertAllMaterials();
        }
    }
    
    private static void ConvertSelectedMaterials()
    {
        foreach (var obj in Selection.objects)
        {
            if (obj is Material material)
            {
                ConvertMaterial(material);
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log("Selected materials converted to URP.");
    }
    
    private static void ConvertAllMaterials()
    {
        string[] materialPaths = AssetDatabase.FindAssets("t:Material");
        
        foreach (var path in materialPaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(path);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material != null)
            {
                ConvertMaterial(material);
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log("All materials converted to URP.");
    }
    
    private static void ConvertMaterial(Material material)
    {
        // 使用URP的材质转换器
        var converter = new UniversalRenderPipelineMaterialUpgrader();
        converter.TryUpgradeMaterial(material);
    }
}
```

---

## 总结

Scriptable Render Pipeline (SRP)是Unity现代渲染技术的核心，通过以下优势彻底改变了Unity的渲染系统：

**技术价值**：
- 完全控制渲染流程，实现自定义视觉效果
- 针对不同硬件和平台优化性能
- 支持现代渲染技术和PBR工作流
- 提供统一的跨平台渲染解决方案

**实际应用**：
- **URP**：适合移动游戏、VR/AR和快速迭代的项目
- **HDRP**：适合追求高质量视觉效果的主机和PC游戏
- **自定义SRP**：适合特殊渲染需求和技术研究

**未来发展**：
- 持续改进性能和功能
- 增强对新硬件和API的支持
- 简化学习曲线和使用流程
- 与Unity其他系统（如DOTS）的深度集成

通过合理选择和配置SRP，开发者可以在性能和视觉效果之间找到最佳平衡点，为不同类型的游戏项目提供最佳的渲染解决方案。