using EventBus.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SubscribeAttributeFinder : Editor
{
    [MenuItem("Assets/Scan Subscribed Functions", false, 10)]
    private static void ScanFolder()
    {
        UnityEngine.Object[] selectedFolders = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets);

        if (selectedFolders.Length == 0)
        {
            Debug.LogWarning("未选择任何文件夹，请选择一个或多个文件夹。");
            return;
        }

        foreach (var folder in selectedFolders)
        {
            string path = AssetDatabase.GetAssetPath(folder);

            if (!Directory.Exists(path)) continue;

            string[] files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            Debug.Log($"开始扫描文件夹: {path}，找到 {files.Length} 个脚本文件。");

            foreach (string file in files)
            {
                ScanFileForAttribute(file);
            }
        }
    }

    private static void ScanFileForAttribute(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            string asmName = assembly.GetName().Name;

            try
            {
                Type[] types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.Name == fileName)
                    {
                        ExtractAttributeMethods(type,filePath);
                    }
                }
            }
            catch (Exception)
            {
                continue;
            }
        }
    }

    private static void ExtractAttributeMethods(Type type,string filePath)
    {
        MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                              BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.DeclaredOnly);

        List<MethodInfo> list = new List<MethodInfo>();

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<SubscribeAttribute>(true);
            if (attr != null)
            {
                Debug.Log($"<color=cyan>[订阅目标]</color> \n" +
                          $"路径: {type.FullName}.{method.Name}\n" +
                          $"参数: {GetParamInfo(method)}");
                list.Add(method);
            }
        }
        if (list.Count > 0)
        {
            EventBusGenerator.Execute(type, list,filePath);
        }
    }

    private static string GetParamInfo(MethodInfo method)
    {
        var ps = method.GetParameters();
        if (ps.Length == 0) return "无参数";
        return string.Join(", ", Array.ConvertAll(ps, p => $"{p.ParameterType.Name} {p.Name}"));
    }
}