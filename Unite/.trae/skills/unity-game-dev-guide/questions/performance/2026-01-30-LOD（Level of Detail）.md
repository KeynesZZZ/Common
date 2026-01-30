---
title: "LOD（Level of Detail）"
date: "2026-01-30"
tags: [Unity, 性能优化, LOD, 渲染优化]
---

# LOD（Level of Detail）

## 问题描述
LOD（Level of Detail，细节层次）是什么？如何在Unity中实现和使用LOD进行性能优化？

## 回答

### 1. 问题分析

LOD是Unity中一种重要的性能优化技术，其核心思想是：

**基本原理**：根据物体与相机的距离，动态切换不同细节程度的模型。距离越远，使用越简化的模型；距离越近，使用越精细的模型。

**为什么需要LOD**：
- 远处的物体玩家看不清楚细节，使用高精度模型是性能浪费
- 减少顶点数、面数和Draw Call，显著提升渲染性能
- 在保持视觉质量的同时优化帧率

---

### 2. 案例演示

#### 2.1 在Unity中设置LOD

**步骤1：准备不同精度的模型**
```
模型层级结构：
- LOD0（高精度）：1000面，近距离显示
- LOD1（中精度）：500面，中距离显示  
- LOD2（低精度）：200面，远距离显示
- Culled（剔除）：超出范围不渲染
```

**步骤2：创建LOD Group**

```csharp
using UnityEngine;

// LOD控制器脚本示例
public class LODController : MonoBehaviour
{
    [System.Serializable]
    public class LODLevel
    {
        public Renderer[] renderers;    // 该LOD级别的渲染器
        public float screenPercentage;  // 屏幕占比阈值（0-1）
    }
    
    [SerializeField] private LODLevel[] lodLevels;
    [SerializeField] private float transitionDistance = 5f;
    
    private Camera mainCamera;
    private int currentLOD = -1;
    
    private void Start()
    {
        mainCamera = Camera.main;
        UpdateLOD();
    }
    
    private void Update()
    {
        UpdateLOD();
    }
    
    private void UpdateLOD()
    {
        float distance = Vector3.Distance(transform.position, mainCamera.transform.position);
        int newLOD = CalculateLOD(distance);
        
        if (newLOD != currentLOD)
        {
            SwitchLOD(newLOD);
            currentLOD = newLOD;
        }
    }
    
    private int CalculateLOD(float distance)
    {
        if (distance < transitionDistance) return 0;
        if (distance < transitionDistance * 2) return 1;
        if (distance < transitionDistance * 3) return 2;
        return -1; // 剔除
    }
    
    private void SwitchLOD(int lodIndex)
    {
        // 禁用所有LOD级别
        for (int i = 0; i < lodLevels.Length; i++)
        {
            foreach (var renderer in lodLevels[i].renderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }
        }
        
        // 启用当前LOD级别
        if (lodIndex >= 0 && lodIndex < lodLevels.Length)
        {
            foreach (var renderer in lodLevels[lodIndex].renderers)
            {
                if (renderer != null)
                    renderer.enabled = true;
            }
        }
    }
}
```

#### 2.2 使用Unity内置LOD Group组件

```csharp
using UnityEngine;

// 程序化创建LOD Group
public class LODGroupSetup : MonoBehaviour
{
    [SerializeField] private GameObject[] lodMeshes; // 从高到低精度的模型
    
    private void Start()
    {
        SetupLODGroup();
    }
    
    private void SetupLODGroup()
    {
        LODGroup lodGroup = gameObject.AddComponent<LODGroup>();
        
        LOD[] lods = new LOD[lodMeshes.Length];
        
        for (int i = 0; i < lodMeshes.Length; i++)
        {
            // 计算屏幕占比（从近到远递减）
            float screenPercentage = 1f / (i + 1);
            
            // 获取所有Renderer组件
            Renderer[] renderers = lodMeshes[i].GetComponentsInChildren<Renderer>();
            
            lods[i] = new LOD(screenPercentage, renderers);
        }
        
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
    }
}
```

#### 2.3 动态加载LOD资源（Addressables）

```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// 动态LOD资源管理
public class DynamicLODLoader : MonoBehaviour
{
    [SerializeField] private AssetReference[] lodAssets; // Addressable引用
    
    private GameObject[] loadedMeshes;
    private int currentLoadedLOD = -1;
    
    private async void Start()
    {
        loadedMeshes = new GameObject[lodAssets.Length];
        
        // 预加载所有LOD级别
        for (int i = 0; i < lodAssets.Length; i++)
        {
            AsyncOperationHandle<GameObject> handle = 
                Addressables.LoadAssetAsync<GameObject>(lodAssets[i]);
            
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedMeshes[i] = handle.Result;
            }
        }
    }
    
    public void SwitchToLOD(int lodLevel)
    {
        if (lodLevel == currentLoadedLOD) return;
        
        // 卸载当前模型
        if (currentLoadedLOD >= 0 && loadedMeshes[currentLoadedLOD] != null)
        {
            loadedMeshes[currentLoadedLOD].SetActive(false);
        }
        
        // 加载新LOD模型
        if (lodLevel >= 0 && lodLevel < loadedMeshes.Length && loadedMeshes[lodLevel] != null)
        {
            GameObject instance = Instantiate(loadedMeshes[lodLevel], transform);
            instance.SetActive(true);
            currentLoadedLOD = lodLevel;
        }
    }
}
```

---

### 3. 注意事项

#### 3.1 LOD设置要点

| 要点 | 说明 |
|------|------|
| **切换距离** | 根据实际场景调整，避免玩家看到明显切换 |
| **过渡效果** | 可使用Cross Fade实现平滑过渡 |
| **碰撞体** | 通常只保留最高精度的碰撞体 |
| **阴影** | 远距离物体可关闭阴影投射 |

#### 3.2 性能优化建议

```csharp
// LOD优化配置示例
public class LODOptimizer : MonoBehaviour
{
    [Header("LOD设置")]
    [SerializeField] private float lodBias = 1.0f;      // LOD偏移系数
    [SerializeField] private float maxDistance = 100f;  // 最大渲染距离
    
    [Header("优化选项")]
    [SerializeField] private bool disableShadowsAtFar = true;
    [SerializeField] private bool useOcclusionCulling = true;
    
    private void Start()
    {
        // 设置全局LOD偏移
        QualitySettings.lodBias = lodBias;
        
        // 配置LOD Group
        LODGroup lodGroup = GetComponent<LODGroup>();
        if (lodGroup != null)
        {
            // 设置最大渲染距离
            LOD[] lods = lodGroup.GetLODs();
            for (int i = 0; i < lods.Length; i++)
            {
                foreach (var renderer in lods[i].renderers)
                {
                    if (renderer != null)
                    {
                        // 远距离关闭阴影
                        if (disableShadowsAtFar && i > 1)
                        {
                            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        }
                    }
                }
            }
            lodGroup.SetLODs(lods);
        }
    }
}
```

#### 3.3 常见陷阱

1. **切换过于明显**：LOD级别之间差异过大，玩家能察觉切换
2. **距离设置不当**：切换距离太近或太远，影响性能或视觉效果
3. **碰撞体丢失**：切换LOD时误删了碰撞体组件
4. **内存占用**：同时加载所有LOD级别的资源导致内存过高

#### 3.4 最佳实践

- **美术规范**：制定明确的LOD制作规范，确保各级别模型风格一致
- **自动LOD**：使用工具（如Simplygon）自动生成LOD模型
- **分级管理**：根据物体重要性设置不同的LOD策略
- **性能测试**：使用Profiler和Frame Debugger验证LOD效果
