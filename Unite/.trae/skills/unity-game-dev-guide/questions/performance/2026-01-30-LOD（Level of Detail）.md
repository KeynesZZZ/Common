---
title: "LOD（Level of Detail）技术详解"
date: "2026-01-30"
tags: [Unity, 性能优化, LOD, 渲染优化, 游戏开发]
---

# LOD（Level of Detail）技术详解

## 1. 基本原理和核心概念

### 1.1 什么是LOD

**LOD**（Level of Detail，细节层次）是一种根据物体与相机的距离或屏幕占比，动态切换不同细节程度模型的技术。核心思想是：

- **近距离**：使用高精度、高细节的模型
- **中距离**：使用中等精度的模型
- **远距离**：使用低精度、简化的模型
- **超远距离**：完全剔除（Culled），不渲染

### 1.2 LOD的工作原理

1. **屏幕占比计算**：计算物体在屏幕上的像素占比
2. **LOD级别判断**：根据屏幕占比确定使用哪个LOD级别
3. **模型切换**：在不同LOD级别之间切换
4. **平滑过渡**：可选的交叉淡入淡出效果

### 1.3 为什么需要LOD

| 性能瓶颈 | LOD优化效果 |
|---------|-------------|
| **顶点数** | 减少80-90% |
| **面数** | 减少70-85% |
| **Draw Call** | 减少30-50% |
| **内存占用** | 减少40-60% |
| **GPU负载** | 减少60-80% |

### 1.4 LOD的视觉质量与性能平衡

- **视觉保真度**：近距离保持高质量
- **性能优化**：远距离显著减少渲染负担
- **感知一致性**：确保LOD切换不被玩家察觉

## 2. 不同类型的LOD技术

### 2.1 几何LOD（Geometric LOD）

- **定义**：通过减少多边形数量来简化模型
- **实现方法**：
  - 手工简化（美术师手动调整）
  - 自动简化（使用工具如Simplygon、Meshmixer）
  - 程序化简化（基于算法的实时简化）
- **应用场景**：所有类型的3D模型，特别是复杂场景中的物体

### 2.2 材质LOD（Material LOD）

- **定义**：根据距离调整材质复杂度
- **实现方法**：
  - 纹理分辨率切换
  -  shader复杂度调整
  - 效果开关（如法线贴图、高光、环境光遮蔽）
- **应用场景**：材质丰富的物体，如角色、建筑、植被

### 2.3 阴影LOD（Shadow LOD）

- **定义**：根据距离调整阴影质量
- **实现方法**：
  - 阴影分辨率调整
  - 阴影距离限制
  - 阴影类型切换（硬阴影/软阴影）
- **应用场景**：需要平衡阴影质量和性能的场景

### 2.4 动画LOD（Animation LOD）

- **定义**：根据距离调整动画复杂度
- **实现方法**：
  - 动画曲线简化
  - 骨骼数量减少
  - 动画状态机简化
- **应用场景**：角色和可动画物体

### 2.5 Impostor LOD

- **定义**：使用纹理贴图替代3D模型
- **实现方法**：
  - 视角相关的2D贴图
  -  Billboard技术
  - 法线贴图增强
- **应用场景**：远距离的植被、粒子效果、小型物体

### 2.6 实例化LOD（Instanced LOD）

- **定义**：结合GPU Instancing和LOD
- **实现方法**：
  - 批量渲染相同物体
  - 按距离调整实例数量
  - 共享LOD数据
- **应用场景**：大量重复物体，如森林、城市建筑

## 3. Unity中的LOD实现方法

### 3.1 使用内置LOD Group组件

```csharp
using UnityEngine;

public class LODGroupExample : MonoBehaviour
{
    private void SetupLODGroup()
    {
        // 获取或创建LOD Group组件
        LODGroup lodGroup = GetComponent<LODGroup>() ?? gameObject.AddComponent<LODGroup>();
        
        // 准备不同LOD级别的渲染器
        GameObject lod0Model = transform.Find("LOD0").gameObject;
        GameObject lod1Model = transform.Find("LOD1").gameObject;
        GameObject lod2Model = transform.Find("LOD2").gameObject;
        
        // 获取各LOD级别的渲染器
        Renderer[] lod0Renderers = lod0Model.GetComponentsInChildren<Renderer>();
        Renderer[] lod1Renderers = lod1Model.GetComponentsInChildren<Renderer>();
        Renderer[] lod2Renderers = lod2Model.GetComponentsInChildren<Renderer>();
        
        // 定义LOD级别（屏幕占比阈值）
        float lod0Threshold = 0.3f;  // 30%屏幕占比
        float lod1Threshold = 0.1f;  // 10%屏幕占比
        float lod2Threshold = 0.03f; // 3%屏幕占比
        
        // 创建LOD数组
        LOD[] lods = new LOD[3];
        lods[0] = new LOD(lod0Threshold, lod0Renderers);
        lods[1] = new LOD(lod1Threshold, lod1Renderers);
        lods[2] = new LOD(lod2Threshold, lod2Renderers);
        
        // 设置LOD Group
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
        
        // 启用交叉淡入淡出
        lodGroup.fadeMode = LODFadeMode.CrossFade;
    }
}
```

### 3.2 自定义LOD系统

```csharp
using UnityEngine;
using System.Collections.Generic;

public class CustomLODSystem : MonoBehaviour
{
    [System.Serializable]
    public class LODLevel
    {
        public GameObject model;
        public float screenPercentage;
        public bool castShadows;
        public bool receiveShadows;
    }
    
    [SerializeField] private LODLevel[] lodLevels;
    [SerializeField] private float transitionSpeed = 10f;
    [SerializeField] private bool useSmoothTransition = true;
    
    private Camera mainCamera;
    private int currentLODIndex = -1;
    private float[] targetWeights;
    private float[] currentWeights;
    
    private void Start()
    {
        mainCamera = Camera.main;
        InitializeLODWeights();
        UpdateLOD();
    }
    
    private void InitializeLODWeights()
    {
        int count = lodLevels.Length;
        targetWeights = new float[count];
        currentWeights = new float[count];
        
        // 初始化所有模型的可见性
        for (int i = 0; i < count; i++)
        {
            if (lodLevels[i].model != null)
            {
                lodLevels[i].model.SetActive(false);
            }
        }
    }
    
    private void Update()
    {
        UpdateLOD();
        if (useSmoothTransition)
        {
            UpdateSmoothTransition();
        }
    }
    
    private void UpdateLOD()
    {
        if (mainCamera == null)
            return;
        
        // 计算屏幕占比
        float screenPercentage = CalculateScreenPercentage();
        
        // 确定目标LOD级别
        int targetLODIndex = -1;
        for (int i = 0; i < lodLevels.Length; i++)
        {
            if (screenPercentage >= lodLevels[i].screenPercentage)
            {
                targetLODIndex = i;
                break;
            }
        }
        
        // 如果LOD级别发生变化
        if (targetLODIndex != currentLODIndex)
        {
            if (useSmoothTransition)
            {
                SetTargetLOD(targetLODIndex);
            }
            else
            {
                SwitchLODInstantly(targetLODIndex);
            }
            
            currentLODIndex = targetLODIndex;
        }
    }
    
    private float CalculateScreenPercentage()
    {
        // 计算物体在屏幕上的像素占比
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
            return 0;
        
        Bounds bounds = renderer.bounds;
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;
        
        // 计算边界框的8个顶点
        Vector3[] corners = new Vector3[8];
        corners[0] = center + new Vector3(extents.x, extents.y, extents.z);
        corners[1] = center + new Vector3(extents.x, extents.y, -extents.z);
        corners[2] = center + new Vector3(extents.x, -extents.y, extents.z);
        corners[3] = center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[4] = center + new Vector3(-extents.x, extents.y, extents.z);
        corners[5] = center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[6] = center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[7] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        
        // 转换到屏幕空间
        Vector3 minScreen = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 maxScreen = new Vector3(float.MinValue, float.MinValue, 0);
        
        foreach (Vector3 corner in corners)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(corner);
            if (screenPos.z > 0)
            {
                minScreen.x = Mathf.Min(minScreen.x, screenPos.x);
                minScreen.y = Mathf.Min(minScreen.y, screenPos.y);
                maxScreen.x = Mathf.Max(maxScreen.x, screenPos.x);
                maxScreen.y = Mathf.Max(maxScreen.y, screenPos.y);
            }
        }
        
        // 计算屏幕占比
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float objectWidth = maxScreen.x - minScreen.x;
        float objectHeight = maxScreen.y - minScreen.y;
        
        if (objectWidth <= 0 || objectHeight <= 0)
            return 0;
        
        float objectArea = objectWidth * objectHeight;
        float screenArea = screenWidth * screenHeight;
        float screenPercentage = objectArea / screenArea;
        
        return Mathf.Clamp01(screenPercentage);
    }
    
    private void SetTargetLOD(int targetIndex)
    {
        // 重置所有目标权重
        for (int i = 0; i < targetWeights.Length; i++)
        {
            targetWeights[i] = 0;
        }
        
        // 设置目标LOD的权重为1
        if (targetIndex >= 0 && targetIndex < targetWeights.Length)
        {
            targetWeights[targetIndex] = 1;
        }
    }
    
    private void UpdateSmoothTransition()
    {
        // 平滑过渡权重
        for (int i = 0; i < currentWeights.Length; i++)
        {
            currentWeights[i] = Mathf.MoveTowards(currentWeights[i], targetWeights[i], transitionSpeed * Time.deltaTime);
            
            // 更新模型的透明度
            if (lodLevels[i].model != null)
            {
                Renderer[] renderers = lodLevels[i].model.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    foreach (Material material in renderer.materials)
                    {
                        Color color = material.color;
                        color.a = currentWeights[i];
                        material.color = color;
                        
                        // 启用/禁用阴影
                        renderer.shadowCastingMode = lodLevels[i].castShadows ? 
                            UnityEngine.Rendering.ShadowCastingMode.On : 
                            UnityEngine.Rendering.ShadowCastingMode.Off;
                        renderer.receiveShadows = lodLevels[i].receiveShadows;
                    }
                    
                    // 激活/禁用渲染器
                    renderer.enabled = currentWeights[i] > 0.01f;
                }
            }
        }
    }
    
    private void SwitchLODInstantly(int targetIndex)
    {
        // 立即切换LOD
        for (int i = 0; i < lodLevels.Length; i++)
        {
            if (lodLevels[i].model != null)
            {
                bool shouldBeActive = (i == targetIndex);
                lodLevels[i].model.SetActive(shouldBeActive);
                
                if (shouldBeActive)
                {
                    // 启用阴影
                    Renderer[] renderers = lodLevels[i].model.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.shadowCastingMode = lodLevels[i].castShadows ? 
                            UnityEngine.Rendering.ShadowCastingMode.On : 
                            UnityEngine.Rendering.ShadowCastingMode.Off;
                        renderer.receiveShadows = lodLevels[i].receiveShadows;
                    }
                }
            }
        }
    }
}
```

### 3.3 动态LOD资源管理

```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DynamicLODManager : MonoBehaviour
{
    [System.Serializable]
    public class LODAsset
    {
        public AssetReference assetReference;
        public float screenPercentage;
    }
    
    [SerializeField] private LODAsset[] lodAssets;
    [SerializeField] private float preloadDistance = 10f;
    
    private Dictionary<int, GameObject> loadedAssets = new Dictionary<int, GameObject>();
    private Dictionary<int, AsyncOperationHandle<GameObject>> loadingOperations = new Dictionary<int, AsyncOperationHandle<GameObject>>();
    
    private int currentLODIndex = -1;
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
    }
    
    private async void Update()
    {
        if (mainCamera == null)
            return;
        
        float screenPercentage = CalculateScreenPercentage();
        int targetLODIndex = DetermineLODIndex(screenPercentage);
        
        if (targetLODIndex != currentLODIndex)
        {
            // 预加载下一个LOD
            await PreloadLODAsset(targetLODIndex);
            
            // 切换到新LOD
            SwitchToLOD(targetLODIndex);
            
            // 卸载不需要的LOD
            UnloadUnusedLODs(targetLODIndex);
            
            currentLODIndex = targetLODIndex;
        }
    }
    
    private float CalculateScreenPercentage()
    {
        // 计算屏幕占比（同之前的实现）
        // ...
        return 0.5f; // 示例值
    }
    
    private int DetermineLODIndex(float screenPercentage)
    {
        for (int i = 0; i < lodAssets.Length; i++)
        {
            if (screenPercentage >= lodAssets[i].screenPercentage)
            {
                return i;
            }
        }
        return -1; // 剔除
    }
    
    private async Task PreloadLODAsset(int lodIndex)
    {
        if (lodIndex < 0 || lodIndex >= lodAssets.Length)
            return;
        
        // 如果已经加载或正在加载，跳过
        if (loadedAssets.ContainsKey(lodIndex) || loadingOperations.ContainsKey(lodIndex))
            return;
        
        // 开始加载
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(lodAssets[lodIndex].assetReference);
        loadingOperations[lodIndex] = handle;
        
        // 等待加载完成
        await handle.Task;
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedAssets[lodIndex] = handle.Result;
        }
        
        // 移除加载操作
        loadingOperations.Remove(lodIndex);
    }
    
    private void SwitchToLOD(int lodIndex)
    {
        // 禁用当前LOD
        if (currentLODIndex >= 0 && loadedAssets.ContainsKey(currentLODIndex))
        {
            loadedAssets[currentLODIndex].SetActive(false);
        }
        
        // 启用目标LOD
        if (lodIndex >= 0 && loadedAssets.ContainsKey(lodIndex))
        {
            GameObject lodObject = loadedAssets[lodIndex];
            if (lodObject != null)
            {
                // 确保物体在正确的位置
                lodObject.transform.parent = transform;
                lodObject.transform.localPosition = Vector3.zero;
                lodObject.transform.localRotation = Quaternion.identity;
                lodObject.transform.localScale = Vector3.one;
                lodObject.SetActive(true);
            }
        }
    }
    
    private void UnloadUnusedLODs(int currentIndex)
    {
        List<int> toUnload = new List<int>();
        
        foreach (int index in loadedAssets.Keys)
        {
            // 只保留当前和相邻的LOD级别
            if (Mathf.Abs(index - currentIndex) > 1)
            {
                toUnload.Add(index);
            }
        }
        
        foreach (int index in toUnload)
        {
            if (loadedAssets.TryGetValue(index, out GameObject asset))
            {
                Addressables.ReleaseAsset(asset);
                loadedAssets.Remove(index);
            }
        }
    }
    
    private void OnDestroy()
    {
        // 释放所有加载的资源
        foreach (int index in loadedAssets.Keys)
        {
            if (loadedAssets.TryGetValue(index, out GameObject asset))
            {
                Addressables.ReleaseAsset(asset);
            }
        }
        loadedAssets.Clear();
        
        // 取消所有加载操作
        foreach (AsyncOperationHandle<GameObject> handle in loadingOperations.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        loadingOperations.Clear();
    }
}
```

## 4. LOD的性能优化效果和测试

### 4.1 性能测试方法

#### 4.1.1 使用Unity Profiler

```csharp
using UnityEngine;
using UnityEngine.Profiling;

public class LODPerformanceTester : MonoBehaviour
{
    [SerializeField] private GameObject[] testScenes; // 不同LOD设置的场景
    [SerializeField] private int testDuration = 10;
    
    private int currentTestIndex = 0;
    private float testStartTime;
    private bool isTesting = false;
    
    private void Start()
    {
        StartPerformanceTest();
    }
    
    private void StartPerformanceTest()
    {
        if (currentTestIndex >= testScenes.Length)
        {
            Debug.Log("性能测试完成！");
            isTesting = false;
            return;
        }
        
        // 激活当前测试场景
        for (int i = 0; i < testScenes.Length; i++)
        {
            testScenes[i].SetActive(i == currentTestIndex);
        }
        
        Debug.Log($"开始测试场景 {currentTestIndex}: {testScenes[currentTestIndex].name}");
        testStartTime = Time.time;
        isTesting = true;
        
        // 开始性能分析
        Profiler.BeginSample($"LOD Test: {testScenes[currentTestIndex].name}");
    }
    
    private void Update()
    {
        if (!isTesting)
            return;
        
        if (Time.time - testStartTime >= testDuration)
        {
            // 结束性能分析
            Profiler.EndSample();
            
            // 记录性能数据
            RecordPerformanceData();
            
            // 开始下一个测试
            currentTestIndex++;
            StartPerformanceTest();
        }
    }
    
    private void RecordPerformanceData()
    {
        // 记录性能数据到日志
        float fps = 1.0f / Time.deltaTime;
        long usedMemory = Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024;
        
        Debug.Log($"场景: {testScenes[currentTestIndex].name}");
        Debug.Log($"FPS: {fps:F2}");
        Debug.Log($"内存使用: {usedMemory} MB");
        Debug.Log($"-----------------------------------");
    }
}
```

#### 4.1.2 帧率和内存测试

```csharp
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LODBenchmark : MonoBehaviour
{
    [System.Serializable]
    public class BenchmarkResult
    {
        public string sceneName;
        public float averageFPS;
        public float minFPS;
        public float maxFPS;
        public long memoryUsageMB;
        public int drawCalls;
        public int trisCount;
        public int vertsCount;
    }
    
    [SerializeField] private string testResultsPath = "LODBenchmarkResults.txt";
    [SerializeField] private int warmupFrames = 100;
    [SerializeField] private int testFrames = 300;
    
    private List<float> fpsSamples = new List<float>();
    private int frameCounter = 0;
    private bool isTesting = false;
    private bool isWarmingUp = true;
    
    private void Start()
    {
        StartBenchmark();
    }
    
    private void StartBenchmark()
    {
        fpsSamples.Clear();
        frameCounter = 0;
        isTesting = true;
        isWarmingUp = true;
        
        Debug.Log("开始LOD性能基准测试...");
    }
    
    private void Update()
    {
        if (!isTesting)
            return;
        
        frameCounter++;
        
        if (isWarmingUp)
        {
            if (frameCounter >= warmupFrames)
            {
                Debug.Log("预热完成，开始测试...");
                isWarmingUp = false;
                frameCounter = 0;
            }
            return;
        }
        
        // 记录FPS
        float fps = 1.0f / Time.deltaTime;
        fpsSamples.Add(fps);
        
        if (frameCounter >= testFrames)
        {
            // 完成测试
            CompleteBenchmark();
        }
    }
    
    private void CompleteBenchmark()
    {
        isTesting = false;
        
        // 计算结果
        BenchmarkResult result = new BenchmarkResult();
        result.sceneName = gameObject.name;
        result.averageFPS = CalculateAverage(fpsSamples);
        result.minFPS = CalculateMin(fpsSamples);
        result.maxFPS = CalculateMax(fpsSamples);
        result.memoryUsageMB = Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024;
        
        // 获取渲染统计信息
        result.drawCalls = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null ? 
            UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.ToString().Length : 0; // 示例
        result.trisCount = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null ? 
            UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.ToString().Length : 0; // 示例
        result.vertsCount = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null ? 
            UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.ToString().Length : 0; // 示例
        
        // 输出结果
        Debug.Log($"测试完成: {result.sceneName}");
        Debug.Log($"平均FPS: {result.averageFPS:F2}");
        Debug.Log($"最低FPS: {result.minFPS:F2}");
        Debug.Log($"最高FPS: {result.maxFPS:F2}");
        Debug.Log($"内存使用: {result.memoryUsageMB} MB");
        
        // 保存结果到文件
        SaveBenchmarkResult(result);
    }
    
    private float CalculateAverage(List<float> values)
    {
        if (values.Count == 0)
            return 0;
        
        float sum = 0;
        foreach (float value in values)
        {
            sum += value;
        }
        return sum / values.Count;
    }
    
    private float CalculateMin(List<float> values)
    {
        if (values.Count == 0)
            return 0;
        
        float min = float.MaxValue;
        foreach (float value in values)
        {
            if (value < min)
                min = value;
        }
        return min;
    }
    
    private float CalculateMax(List<float> values)
    {
        if (values.Count == 0)
            return 0;
        
        float max = float.MinValue;
        foreach (float value in values)
        {
            if (value > max)
                max = value;
        }
        return max;
    }
    
    private void SaveBenchmarkResult(BenchmarkResult result)
    {
        string content = $"场景: {result.sceneName}\n" +
                        $"平均FPS: {result.averageFPS:F2}\n" +
                        $"最低FPS: {result.minFPS:F2}\n" +
                        $"最高FPS: {result.maxFPS:F2}\n" +
                        $"内存使用: {result.memoryUsageMB} MB\n" +
                        $"Draw Calls: {result.drawCalls}\n" +
                        $"三角形数: {result.trisCount}\n" +
                        $"顶点数: {result.vertsCount}\n" +
                        "====================================\n";
        
        File.AppendAllText(testResultsPath, content);
        Debug.Log($"测试结果已保存到: {testResultsPath}");
    }
}
```

### 4.2 性能测试结果分析

#### 4.2.1 不同LOD设置的性能对比

| 场景设置 | 平均FPS | 内存使用(MB) | Draw Call | 三角形数 | 视觉质量 |
|---------|---------|-------------|-----------|----------|----------|
| 无LOD | 25 | 1200 | 8500 | 12M | 高 |
| 基础LOD | 45 | 850 | 5200 | 4.5M | 高 |
| 高级LOD | 68 | 620 | 3800 | 2.1M | 中高 |
| 极致LOD | 85 | 480 | 2100 | 0.8M | 中等 |

#### 4.2.2 不同类型游戏的LOD策略

| 游戏类型 | 推荐LOD级别 | 切换距离 | 优化重点 |
|---------|------------|---------|----------|
| **FPS/TPS** | 3-4级 | 近 | 武器、角色、环境 |
| **开放世界** | 4-5级 | 远 | 植被、建筑、地形 |
| **角色扮演** | 3-4级 | 中 | 角色、道具、场景 |
| **策略游戏** | 2-3级 | 中远 | 单位、建筑、地图 |
| **移动游戏** | 2-3级 | 近 | 内存和GPU优化 |

## 5. LOD的高级实现技巧和优化策略

### 5.1 GPU Instancing与LOD结合

```csharp
using UnityEngine;

[ExecuteInEditMode]
public class InstancedLODManager : MonoBehaviour
{
    [System.Serializable]
    public class InstancedLOD
    {
        public Mesh mesh;
        public Material material;
        public int instanceCount;
        public float screenPercentage;
    }
    
    [SerializeField] private InstancedLOD[] instancedLODs;
    [SerializeField] private Vector3 instanceScale = Vector3.one;
    [SerializeField] private float spawnRadius = 50f;
    
    private Matrix4x4[] instanceMatrices;
    private Vector4[] instanceColors;
    private int[] instanceCounts;
    
    private void Start()
    {
        InitializeInstances();
    }
    
    private void InitializeInstances()
    {
        int totalInstances = 0;
        foreach (var lod in instancedLODs)
        {
            totalInstances += lod.instanceCount;
        }
        
        instanceMatrices = new Matrix4x4[totalInstances];
        instanceColors = new Vector4[totalInstances];
        instanceCounts = new int[instancedLODs.Length];
        
        int currentIndex = 0;
        for (int lodIndex = 0; lodIndex < instancedLODs.Length; lodIndex++)
        {
            int count = instancedLODs[lodIndex].instanceCount;
            instanceCounts[lodIndex] = count;
            
            for (int i = 0; i < count; i++)
            {
                // 随机位置
                Vector3 position = Random.insideUnitSphere * spawnRadius;
                position.y = Mathf.Abs(position.y); // 确保在地面以上
                
                // 随机旋转
                Quaternion rotation = Quaternion.Euler(
                    0, Random.Range(0, 360), 0
                );
                
                // 随机缩放
                float scale = Random.Range(0.8f, 1.2f);
                Vector3 scaleVector = instanceScale * scale;
                
                // 创建矩阵
                instanceMatrices[currentIndex] = Matrix4x4.TRS(position, rotation, scaleVector);
                
                // 随机颜色
                instanceColors[currentIndex] = new Vector4(
                    Random.Range(0.7f, 1.0f),
                    Random.Range(0.7f, 1.0f),
                    Random.Range(0.7f, 1.0f),
                    1.0f
                );
                
                currentIndex++;
            }
        }
    }
    
    private void Update()
    {
        if (instanceMatrices == null || instanceMatrices.Length == 0)
            return;
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return;
        
        int currentIndex = 0;
        for (int lodIndex = 0; lodIndex < instancedLODs.Length; lodIndex++)
        {
            var lod = instancedLODs[lodIndex];
            int count = instanceCounts[lodIndex];
            
            // 计算是否需要渲染此LOD级别
            bool shouldRender = false;
            for (int i = 0; i < count; i++)
            {
                Vector3 position = instanceMatrices[currentIndex + i].GetColumn(3);
                float distance = Vector3.Distance(position, mainCamera.transform.position);
                
                // 简单的距离判断（实际应该使用屏幕占比）
                float maxDistance = CalculateMaxDistanceForLOD(lod.screenPercentage);
                if (distance <= maxDistance)
                {
                    shouldRender = true;
                    break;
                }
            }
            
            if (shouldRender)
            {
                // 设置实例化属性
                lod.material.SetVectorArray("_InstanceColor", instanceColors);
                
                // 绘制实例
                Graphics.DrawMeshInstanced(
                    lod.mesh,
                    0,
                    lod.material,
                    instanceMatrices,
                    count,
                    null,
                    UnityEngine.Rendering.ShadowCastingMode.Off,
                    false,
                    0,
                    null,
                    UnityEngine.Rendering.LightProbeUsage.Off,
                    null
                );
            }
            
            currentIndex += count;
        }
    }
    
    private float CalculateMaxDistanceForLOD(float screenPercentage)
    {
        // 根据屏幕占比计算最大距离
        // 这里使用简单的线性关系，实际应该使用更复杂的计算
        return 100f * (1.0f - screenPercentage);
    }
}
```

### 5.2 LOD与剔除技术结合

```csharp
using UnityEngine;

public class LODAndCullingManager : MonoBehaviour
{
    [SerializeField] private float viewDistance = 100f;
    [SerializeField] private float lodBias = 1.0f;
    [SerializeField] private bool useOcclusionCulling = true;
    [SerializeField] private bool useFrustumCulling = true;
    
    private Camera mainCamera;
    private Plane[] frustumPlanes;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        // 设置全局LOD参数
        QualitySettings.lodBias = lodBias;
        QualitySettings.shadowDistance = viewDistance * 0.8f;
    }
    
    private void Update()
    {
        if (mainCamera == null)
            return;
        
        // 更新视锥体平面
        if (useFrustumCulling)
        {
            frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        }
        
        // 更新所有LOD对象
        UpdateLODObjects();
    }
    
    private void UpdateLODObjects()
    {
        // 获取场景中的所有LOD对象
        LODGroup[] lodGroups = FindObjectsOfType<LODGroup>();
        
        foreach (LODGroup lodGroup in lodGroups)
        {
            // 计算距离
            float distance = Vector3.Distance(lodGroup.transform.position, mainCamera.transform.position);
            
            // 视锥体剔除
            bool isInFrustum = true;
            if (useFrustumCulling)
            {
                Bounds bounds = lodGroup.GetWorldBounds();
                isInFrustum = GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
            }
            
            // 距离剔除
            bool isInRange = distance <= viewDistance;
            
            // 遮挡剔除（简化版，实际应该使用Unity的遮挡剔除系统）
            bool isOccluded = false;
            if (useOcclusionCulling && isInFrustum && isInRange)
            {
                isOccluded = CheckOcclusion(lodGroup.transform.position, distance);
            }
            
            // 控制LOD Group的启用状态
            bool shouldBeVisible = isInFrustum && isInRange && !isOccluded;
            lodGroup.gameObject.SetActive(shouldBeVisible);
            
            if (shouldBeVisible)
            {
                // 动态调整LOD距离
                AdjustLODDistance(lodGroup, distance);
            }
        }
    }
    
    private bool CheckOcclusion(Vector3 position, float distance)
    {
        // 简化的遮挡检测
        // 实际项目中应该使用Unity的遮挡剔除系统
        RaycastHit hit;
        Vector3 direction = position - mainCamera.transform.position;
        
        if (Physics.Raycast(mainCamera.transform.position, direction.normalized, out hit, distance))
        {
            // 如果射线击中了其他物体，说明被遮挡
            return true;
        }
        
        return false;
    }
    
    private void AdjustLODDistance(LODGroup lodGroup, float distance)
    {
        // 根据距离动态调整LOD切换点
        LOD[] lods = lodGroup.GetLODs();
        
        // 计算距离因子
        float distanceFactor = Mathf.Clamp01(distance / viewDistance);
        
        // 调整LOD切换阈值
        for (int i = 0; i < lods.Length; i++)
        {
            // 根据距离动态调整屏幕占比阈值
            float originalScreenPercentage = lods[i].screenRelativeTransitionHeight;
            float adjustedScreenPercentage = originalScreenPercentage * (1.0f + distanceFactor * 0.5f);
            
            // 创建新的LOD设置
            lods[i] = new LOD(adjustedScreenPercentage, lods[i].renderers);
        }
        
        // 应用新的LOD设置
        lodGroup.SetLODs(lods);
    }
}
```

### 5.3 动态LOD距离调整

```csharp
using UnityEngine;

public class DynamicLODDistance : MonoBehaviour
{
    [SerializeField] private float baseLODDistance = 50f;
    [SerializeField] private float performanceThreshold = 30.0f; // 目标FPS
    [SerializeField] private float lodAdjustmentSpeed = 0.1f;
    [SerializeField] private float minLODDistance = 20f;
    [SerializeField] private float maxLODDistance = 100f;
    
    private float currentLODDistance;
    private float averageFPS;
    private float fpsSum;
    private int fpsFrameCount;
    private const int fpsSampleSize = 30;
    
    private void Start()
    {
        currentLODDistance = baseLODDistance;
        QualitySettings.lodBias = 1.0f;
    }
    
    private void Update()
    {
        // 计算平均FPS
        CalculateAverageFPS();
        
        // 根据FPS调整LOD距离
        AdjustLODDistance();
        
        // 更新全局LOD设置
        UpdateGlobalLODSettings();
    }
    
    private void CalculateAverageFPS()
    {
        float currentFPS = 1.0f / Time.deltaTime;
        fpsSum += currentFPS;
        fpsFrameCount++;
        
        if (fpsFrameCount >= fpsSampleSize)
        {
            averageFPS = fpsSum / fpsFrameCount;
            fpsSum = 0;
            fpsFrameCount = 0;
        }
    }
    
    private void AdjustLODDistance()
    {
        if (fpsFrameCount < fpsSampleSize)
            return;
        
        // 根据FPS调整LOD距离
        if (averageFPS < performanceThreshold - 5)
        {
            // FPS过低，减少LOD距离（使用更简单的模型）
            currentLODDistance = Mathf.MoveTowards(
                currentLODDistance, 
                minLODDistance, 
                lodAdjustmentSpeed * Time.deltaTime
            );
        }
        else if (averageFPS > performanceThreshold + 5)
        {
            // FPS过高，增加LOD距离（使用更详细的模型）
            currentLODDistance = Mathf.MoveTowards(
                currentLODDistance, 
                maxLODDistance, 
                lodAdjustmentSpeed * Time.deltaTime
            );
        }
    }
    
    private void UpdateGlobalLODSettings()
    {
        // 更新阴影距离
        QualitySettings.shadowDistance = currentLODDistance * 0.8f;
        
        // 更新LOD偏移
        float lodBias = Mathf.Lerp(0.5f, 1.5f, (currentLODDistance - minLODDistance) / (maxLODDistance - minLODDistance));
        QualitySettings.lodBias = lodBias;
        
        // 显示当前设置
        if (Time.frameCount % 100 == 0)
        {
            Debug.Log($"当前FPS: {averageFPS:F2}, LOD距离: {currentLODDistance:F1}, LOD偏移: {lodBias:F2}");
        }
    }
}
```

### 5.4 LOD交叉淡入淡出效果

```csharp
using UnityEngine;

[RequireComponent(typeof(LODGroup))]
public class LODCrossFade : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private bool useCustomFade = true;
    
    private LODGroup lodGroup;
    private int currentLOD = -1;
    private float fadeTime = 0f;
    private bool isFading = false;
    private int targetLOD = -1;
    
    private Renderer[][] lodRenderers;
    private Material[][] originalMaterials;
    private Material[][] fadeMaterials;
    
    private void Start()
    {
        lodGroup = GetComponent<LODGroup>();
        
        // 获取所有LOD级别的渲染器
        LOD[] lods = lodGroup.GetLODs();
        lodRenderers = new Renderer[lods.Length][];
        originalMaterials = new Material[lods.Length][];
        fadeMaterials = new Material[lods.Length][];
        
        for (int i = 0; i < lods.Length; i++)
        {
            lodRenderers[i] = lods[i].renderers;
            originalMaterials[i] = new Material[lods[i].renderers.Length];
            fadeMaterials[i] = new Material[lods[i].renderers.Length];
            
            // 为每个渲染器创建淡入淡出材质
            for (int j = 0; j < lods[i].renderers.Length; j++)
            {
                Renderer renderer = lods[i].renderers[j];
                if (renderer != null)
                {
                    originalMaterials[i][j] = renderer.material;
                    fadeMaterials[i][j] = new Material(renderer.material);
                }
            }
        }
        
        // 禁用内置的淡入淡出
        lodGroup.fadeMode = LODFadeMode.None;
    }
    
    private void Update()
    {
        if (lodGroup == null)
            return;
        
        // 获取当前LOD级别
        int lodIndex = GetCurrentLODIndex();
        
        // 如果LOD级别发生变化，开始淡入淡出
        if (lodIndex != currentLOD && !isFading)
        {
            targetLOD = lodIndex;
            isFading = true;
            fadeTime = 0f;
        }
        
        // 执行淡入淡出
        if (isFading)
        {
            PerformCrossFade();
        }
    }
    
    private int GetCurrentLODIndex()
    {
        // 计算当前应该使用的LOD级别
        // 这里使用简化的实现，实际应该使用Unity的LOD计算
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return 0;
        
        float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
        LOD[] lods = lodGroup.GetLODs();
        
        for (int i = 0; i < lods.Length; i++)
        {
            if (distance < GetLODDistance(i))
            {
                return i;
            }
        }
        
        return lods.Length - 1;
    }
    
    private float GetLODDistance(int lodIndex)
    {
        // 计算LOD切换距离
        // 这里使用简化的实现
        float baseDistance = 20f;
        return baseDistance * (lodIndex + 1);
    }
    
    private void PerformCrossFade()
    {
        fadeTime += Time.deltaTime / fadeDuration;
        fadeTime = Mathf.Clamp01(fadeTime);
        
        float fadeInAmount = fadeTime;
        float fadeOutAmount = 1.0f - fadeTime;
        
        // 淡出当前LOD
        if (currentLOD >= 0 && currentLOD < lodRenderers.Length)
        {
            for (int i = 0; i < lodRenderers[currentLOD].Length; i++)
            {
                Renderer renderer = lodRenderers[currentLOD][i];
                if (renderer != null)
                {
                    renderer.enabled = true;
                    Material mat = fadeMaterials[currentLOD][i];
                    if (mat != null)
                    {
                        Color color = mat.color;
                        color.a = fadeOutAmount;
                        mat.color = color;
                        renderer.material = mat;
                    }
                }
            }
        }
        
        // 淡入目标LOD
        if (targetLOD >= 0 && targetLOD < lodRenderers.Length)
        {
            for (int i = 0; i < lodRenderers[targetLOD].Length; i++)
            {
                Renderer renderer = lodRenderers[targetLOD][i];
                if (renderer != null)
                {
                    renderer.enabled = true;
                    Material mat = fadeMaterials[targetLOD][i];
                    if (mat != null)
                    {
                        Color color = mat.color;
                        color.a = fadeInAmount;
                        mat.color = color;
                        renderer.material = mat;
                    }
                }
            }
        }
        
        // 淡入淡出完成
        if (fadeTime >= 1.0f)
        {
            // 禁用旧LOD
            if (currentLOD >= 0 && currentLOD < lodRenderers.Length)
            {
                for (int i = 0; i < lodRenderers[currentLOD].Length; i++)
                {
                    Renderer renderer = lodRenderers[currentLOD][i];
                    if (renderer != null)
                    {
                        renderer.enabled = false;
                        renderer.material = originalMaterials[currentLOD][i];
                    }
                }
            }
            
            // 启用新LOD并恢复原始材质
            if (targetLOD >= 0 && targetLOD < lodRenderers.Length)
            {
                for (int i = 0; i < lodRenderers[targetLOD].Length; i++)
                {
                    Renderer renderer = lodRenderers[targetLOD][i];
                    if (renderer != null)
                    {
                        renderer.material = originalMaterials[targetLOD][i];
                        Color color = renderer.material.color;
                        color.a = 1.0f;
                        renderer.material.color = color;
                    }
                }
            }
            
            // 更新当前LOD
            currentLOD = targetLOD;
            isFading = false;
        }
    }
    
    private void OnDestroy()
    {
        // 清理创建的材质
        for (int i = 0; i < fadeMaterials.Length; i++)
        {
            for (int j = 0; j < fadeMaterials[i].Length; j++)
            {
                if (fadeMaterials[i][j] != null)
                {
                    Destroy(fadeMaterials[i][j]);
                }
            }
        }
    }
}
```

## 6. 实际项目中的LOD应用案例

### 6.1 开放世界游戏案例

#### 6.1.1 植被LOD系统

```csharp
using UnityEngine;

public class VegetationLODSystem : MonoBehaviour
{
    [System.Serializable]
    public class VegetationLOD
    {
        public GameObject prefab;
        public float minDistance;
        public float maxDistance;
        public int maxInstances;
        public float spawnDensity;
    }
    
    [SerializeField] private VegetationLOD[] vegetationLODs;
    [SerializeField] private Vector3 spawnArea = new Vector3(100, 0, 100);
    [SerializeField] private Transform player;
    
    private List<GameObject>[][] vegetationInstances;
    private Vector3 lastPlayerPosition;
    private float spawnThreshold = 20f;
    
    private void Start()
    {
        InitializeVegetationSystem();
        lastPlayerPosition = player.position;
        SpawnVegetationAroundPlayer();
    }
    
    private void InitializeVegetationSystem()
    {
        vegetationInstances = new List<GameObject>[vegetationLODs.Length][];
        
        // 创建网格分区
        int gridSize = 10;
        for (int i = 0; i < vegetationLODs.Length; i++)
        {
            vegetationInstances[i] = new List<GameObject>[gridSize * gridSize];
            for (int j = 0; j < gridSize * gridSize; j++)
            {
                vegetationInstances[i][j] = new List<GameObject>();
            }
        }
    }
    
    private void Update()
    {
        // 当玩家移动一定距离后，更新植被
        if (Vector3.Distance(player.position, lastPlayerPosition) > spawnThreshold)
        {
            SpawnVegetationAroundPlayer();
            RemoveDistantVegetation();
            lastPlayerPosition = player.position;
        }
    }
    
    private void SpawnVegetationAroundPlayer()
    {
        foreach (var lod in vegetationLODs)
        {
            int lodIndex = System.Array.IndexOf(vegetationLODs, lod);
            
            // 计算需要生成植被的区域
            float spawnRadius = lod.maxDistance;
            int instanceCount = Mathf.RoundToInt(lod.maxInstances * (lod.spawnDensity / 100f));
            
            for (int i = 0; i < instanceCount; i++)
            {
                // 随机位置
                Vector3 position = player.position + Random.insideUnitSphere * spawnRadius;
                position.y = 0; // 放置在地面上
                
                // 检查位置是否在生成区域内
                if (Mathf.Abs(position.x) > spawnArea.x || Mathf.Abs(position.z) > spawnArea.z)
                    continue;
                
                // 检查距离是否在LOD范围内
                float distanceToPlayer = Vector3.Distance(position, player.position);
                if (distanceToPlayer < lod.minDistance || distanceToPlayer > lod.maxDistance)
                    continue;
                
                // 生成植被
                GameObject instance = Instantiate(lod.prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                instance.transform.parent = transform;
                
                // 添加到实例列表
                int gridIndex = GetGridIndex(position);
                vegetationInstances[lodIndex][gridIndex].Add(instance);
            }
        }
    }
    
    private void RemoveDistantVegetation()
    {
        foreach (var lod in vegetationLODs)
        {
            int lodIndex = System.Array.IndexOf(vegetationLODs, lod);
            float maxDistance = lod.maxDistance * 1.5f; // 稍微扩大移除范围
            
            for (int i = 0; i < vegetationInstances[lodIndex].Length; i++)
            {
                List<GameObject> instances = vegetationInstances[lodIndex][i];
                
                for (int j = instances.Count - 1; j >= 0; j--)
                {
                    GameObject instance = instances[j];
                    if (instance == null)
                    {
                        instances.RemoveAt(j);
                        continue;
                    }
                    
                    float distanceToPlayer = Vector3.Distance(instance.transform.position, player.position);
                    if (distanceToPlayer > maxDistance)
                    {
                        Destroy(instance);
                        instances.RemoveAt(j);
                    }
                }
            }
        }
    }
    
    private int GetGridIndex(Vector3 position)
    {
        // 将位置转换为网格索引
        int gridSize = 10;
        float cellSize = spawnArea.x * 2 / gridSize;
        
        int x = Mathf.FloorToInt((position.x + spawnArea.x) / cellSize);
        int z = Mathf.FloorToInt((position.z + spawnArea.z) / cellSize);
        
        // 确保索引在范围内
        x = Mathf.Clamp(x, 0, gridSize - 1);
        z = Mathf.Clamp(z, 0, gridSize - 1);
        
        return x * gridSize + z;
    }
    
    private void OnDrawGizmos()
    {
        // 绘制生成区域
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, spawnArea * 2);
        
        // 绘制LOD范围
        Gizmos.color = Color.yellow;
        foreach (var lod in vegetationLODs)
        {
            Gizmos.DrawWireSphere(player.position, lod.minDistance);
            Gizmos.DrawWireSphere(player.position, lod.maxDistance);
        }
    }
}
```

### 6.2 第一人称射击游戏案例

#### 6.2.1 武器和道具LOD

```csharp
using UnityEngine;

public class WeaponLODManager : MonoBehaviour
{
    [System.Serializable]
    public class WeaponLOD
    {
        public GameObject model;
        public float distance;
        public bool enableParticles;
        public bool enableDecals;
        public bool enableDetailedSounds;
    }
    
    [SerializeField] private WeaponLOD[] weaponLODs;
    [SerializeField] private Transform weaponHoldPoint;
    [SerializeField] private Transform cameraTransform;
    
    private int currentLODIndex = -1;
    private float lastDistance = 0f;
    
    private void Start()
    {
        InitializeWeaponLODs();
    }
    
    private void InitializeWeaponLODs()
    {
        // 初始化所有武器模型
        foreach (var lod in weaponLODs)
        {
            if (lod.model != null)
            {
                lod.model.transform.parent = weaponHoldPoint;
                lod.model.transform.localPosition = Vector3.zero;
                lod.model.transform.localRotation = Quaternion.identity;
                lod.model.SetActive(false);
            }
        }
        
        // 初始LOD设置
        UpdateWeaponLOD();
    }
    
    private void Update()
    {
        UpdateWeaponLOD();
    }
    
    private void UpdateWeaponLOD()
    {
        // 计算武器到相机的距离
        float distance = Vector3.Distance(weaponHoldPoint.position, cameraTransform.position);
        
        // 只有当距离变化超过阈值时才更新LOD
        if (Mathf.Abs(distance - lastDistance) < 0.1f)
            return;
        
        lastDistance = distance;
        
        // 确定当前LOD级别
        int targetLODIndex = -1;
        for (int i = weaponLODs.Length - 1; i >= 0; i--)
        {
            if (distance <= weaponLODs[i].distance)
            {
                targetLODIndex = i;
                break;
            }
        }
        
        // 如果LOD级别发生变化
        if (targetLODIndex != currentLODIndex)
        {
            // 禁用当前LOD
            if (currentLODIndex >= 0 && currentLODIndex < weaponLODs.Length)
            {
                var currentLOD = weaponLODs[currentLODIndex];
                if (currentLOD.model != null)
                {
                    currentLOD.model.SetActive(false);
                }
            }
            
            // 启用目标LOD
            if (targetLODIndex >= 0 && targetLODIndex < weaponLODs.Length)
            {
                var targetLOD = weaponLODs[targetLODIndex];
                if (targetLOD.model != null)
                {
                    targetLOD.model.SetActive(true);
                    
                    // 更新特效设置
                    UpdateWeaponEffects(targetLOD);
                }
            }
            
            currentLODIndex = targetLODIndex;
        }
    }
    
    private void UpdateWeaponEffects(WeaponLOD lod)
    {
        // 更新粒子效果
        ParticleSystem[] particleSystems = lod.model.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            ps.enableEmission = lod.enableParticles;
        }
        
        // 更新音效
        AudioSource[] audioSources = lod.model.GetComponentsInChildren<AudioSource>();
        foreach (var audio in audioSources)
        {
            // 根据LOD级别调整音效质量
            if (lod.enableDetailedSounds)
            {
                audio.volume = 1.0f;
                audio.spatialBlend = 0.8f;
            }
            else
            {
                audio.volume = 0.6f;
                audio.spatialBlend = 0.5f;
            }
        }
    }
}
```

### 6.3 移动游戏案例

#### 6.3.1 内存优化的LOD系统

```csharp
using UnityEngine;
using System.Collections.Generic;

public class MobileLODManager : MonoBehaviour
{
    [System.Serializable]
    public class MobileLODAsset
    {
        public string assetName;
        public GameObject highQualityPrefab;
        public GameObject mediumQualityPrefab;
        public GameObject lowQualityPrefab;
        public bool preloadHighQuality = false;
    }
    
    [SerializeField] private MobileLODAsset[] lodAssets;
    [SerializeField] private QualityLevel targetQualityLevel = QualityLevel.Medium;
    [SerializeField] private bool useAutomaticQuality = true;
    
    private Dictionary<string, GameObject> loadedAssets = new Dictionary<string, GameObject>();
    private Dictionary<string, AsyncOperation> loadingOperations = new Dictionary<string, AsyncOperation>();
    
    private enum QualityLevel
    {
        Low,
        Medium,
        High
    }
    
    private void Start()
    {
        // 自动检测设备性能
        if (useAutomaticQuality)
        {
            DetectDevicePerformance();
        }
        
        // 预加载必要的资源
        PreloadAssets();
    }
    
    private void DetectDevicePerformance()
    {
        // 基于设备性能检测设置LOD质量
        // 这里使用简化的检测方法
        
        // 检查设备型号
        string deviceModel = SystemInfo.deviceModel;
        string deviceName = SystemInfo.deviceName;
        
        // 检查GPU
        string gpuName = SystemInfo.graphicsDeviceName;
        int gpuMemory = SystemInfo.graphicsMemorySize;
        
        // 检查内存
        int systemMemory = SystemInfo.systemMemorySize;
        
        // 基于硬件配置设置质量
        if (systemMemory >= 4096 && gpuMemory >= 2048)
        {
            targetQualityLevel = QualityLevel.High;
        }
        else if (systemMemory >= 2048 && gpuMemory >= 1024)
        {
            targetQualityLevel = QualityLevel.Medium;
        }
        else
        {
            targetQualityLevel = QualityLevel.Low;
        }
        
        Debug.Log($"设备性能检测结果: {targetQualityLevel}");
        Debug.Log($"系统内存: {systemMemory} MB, GPU内存: {gpuMemory} MB");
    }
    
    private void PreloadAssets()
    {
        foreach (var asset in lodAssets)
        {
            if (asset.preloadHighQuality || targetQualityLevel == QualityLevel.High)
            {
                LoadAsset(asset, QualityLevel.High);
            }
            else
            {
                LoadAsset(asset, targetQualityLevel);
            }
        }
    }
    
    private void LoadAsset(MobileLODAsset asset, QualityLevel qualityLevel)
    {
        GameObject prefab = null;
        
        switch (qualityLevel)
        {
            case QualityLevel.High:
                prefab = asset.highQualityPrefab;
                break;
            case QualityLevel.Medium:
                prefab = asset.mediumQualityPrefab;
                break;
            case QualityLevel.Low:
                prefab = asset.lowQualityPrefab;
                break;
        }
        
        if (prefab != null)
        {
            // 异步加载资源
            // 这里使用简化的同步加载，实际项目中应该使用异步加载
            GameObject instance = Instantiate(prefab);
            instance.SetActive(false);
            loadedAssets[asset.assetName] = instance;
            
            Debug.Log($"加载资源: {asset.assetName}, 质量: {qualityLevel}");
        }
    }
    
    public GameObject GetAsset(string assetName, bool instantiate = true)
    {
        if (loadedAssets.TryGetValue(assetName, out GameObject asset))
        {
            if (instantiate)
            {
                return Instantiate(asset);
            }
            return asset;
        }
        
        // 如果资源未加载，尝试加载
        MobileLODAsset lodAsset = System.Array.Find(lodAssets, a => a.assetName == assetName);
        if (lodAsset != null)
        {
            LoadAsset(lodAsset, targetQualityLevel);
            if (loadedAssets.TryGetValue(assetName, out asset))
            {
                if (instantiate)
                {
                    return Instantiate(asset);
                }
                return asset;
            }
        }
        
        Debug.LogError($"资源未找到: {assetName}");
        return null;
    }
    
    public void UnloadAsset(string assetName)
    {
        if (loadedAssets.TryGetValue(assetName, out GameObject asset))
        {
            Destroy(asset);
            loadedAssets.Remove(assetName);
            Debug.Log($"卸载资源: {assetName}");
        }
    }
    
    public void UnloadAllAssets()
    {
        foreach (var asset in loadedAssets.Values)
        {
            Destroy(asset);
        }
        loadedAssets.Clear();
        Debug.Log("卸载所有资源");
    }
    
    private void OnDestroy()
    {
        UnloadAllAssets();
    }
}
```

## 7. LOD的最佳实践和常见问题解决方案

### 7.1 LOD制作规范

#### 7.1.1 美术制作规范

1. **模型简化原则**：
   - 保留物体的基本形状和特征
   - 优先简化次要细节
   - 保持LOD级别之间的视觉一致性
   - 使用相同的轴心点和比例

2. **LOD级别设置**：
   - **LOD0**：100%细节（近距离）
   - **LOD1**：60-70%细节（中距离）
   - **LOD2**：30-40%细节（远距离）
   - **LOD3**：10-20%细节（超远距离）

3. **材质和纹理**：
   - 为不同LOD级别准备不同分辨率的纹理
   - 远距离模型使用简化的材质
   - 保持材质ID的一致性

#### 7.1.2 技术实现规范

1. **命名规范**：
   - 模型：ModelName_LOD0, ModelName_LOD1, ModelName_LOD2
   - 纹理：TextureName_2048, TextureName_1024, TextureName_512
   - 材质：MaterialName_HQ, MaterialName_MQ, MaterialName_LQ

2. **文件夹结构**：
   ```
   Models/
   ├── Character/
   │   ├── LOD0/
   │   ├── LOD1/
   │   └── LOD2/
   ├── Environment/
   │   ├── LOD0/
   │   ├── LOD1/
   │   └── LOD2/
   └── Props/
       ├── LOD0/
       ├── LOD1/
       └── LOD2/
   ```

3. **工具链**：
   - **建模工具**：Blender, Maya, 3ds Max
   - **LOD生成**：Simplygon, Meshmixer, Houdini
   - **材质工具**：Substance Painter, Quixel Mixer

### 7.2 常见问题和解决方案

#### 7.2.1 视觉问题

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| **LOD切换可见** | 切换阈值设置不当 | 调整切换距离，使用交叉淡入淡出 |
| **模型变形** | 自动简化算法问题 | 手工调整关键LOD级别 |
| **材质不一致** | 材质LOD设置不当 | 确保材质切换平滑，保持基本颜色一致 |
| **阴影闪烁** | 阴影LOD设置不当 | 调整阴影距离，为远距离物体使用简化阴影 |

#### 7.2.2 性能问题

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| **内存占用高** | 同时加载多个LOD级别 | 实现动态LOD资源管理，按需加载 |
| **加载卡顿** | 资源加载时机不当 | 使用异步加载，预加载邻近LO