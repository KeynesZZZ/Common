using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class EventBusGenerator
{
    public static void Execute(Type containingType, List<MethodInfo> methods, string filePath)
    {
        if (containingType == null || methods == null || methods.Count == 0)
        {
            Debug.LogWarning("EventBusGenerator: 无效的类型或方法列表");
            return;
        }

        var source = GenerateSource(containingType, methods);
        var fileName = $"{containingType.Name}_Generated.cs";

        var outputPath = Path.GetDirectoryName(filePath);
        var fullPath = Path.Combine(outputPath, "Generated", fileName);
        var fullOutputPath = Path.Combine(Application.dataPath, "..", outputPath);

        try
        {
            if (!Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
                Debug.Log($"EventBusGenerator: 创建文件夹 {outputPath}");
            }

            var fullFilePath = Path.Combine(fullOutputPath, fileName);
            File.WriteAllText(fullFilePath, source, Encoding.UTF8);

            Debug.Log($"<color=green>EventBusGenerator: 成功生成文件 {fullPath}</color>");

            AssetDatabase.Refresh();
        }
        catch (Exception ex)
        {
            Debug.LogError($"EventBusGenerator: 生成文件失败 - {ex.Message}");
        }
    }

    private static string GenerateSource(Type containingType, List<MethodInfo> methods)
    {
        var namespaceName = containingType.Namespace ?? "";
        var className = containingType.Name;

        var sb = new StringBuilder();
       
        sb.AppendLine("using System;");
        if (!string.IsNullOrEmpty(namespaceName))
        {
            sb.AppendLine($"namespace {namespaceName}");
        }
        sb.AppendLine("{");
        sb.AppendLine($"    public partial class {className}");
        sb.AppendLine("    {");

        foreach (var method in methods)
        {
            var methodName = method.Name;
            var paramType = method.GetParameters()[0].ParameterType;
            var eventType = paramType.IsByRef ? paramType.GetElementType().FullName : paramType.FullName;
            var wrapperName = $"{methodName}_Wrapper";
            var handlerFieldName = $"{methodName}_handler";

            sb.AppendLine($"        private struct {wrapperName} : EventBus.Core.IEventListener<{eventType}>,IEquatable<{wrapperName}>");
            sb.AppendLine("        {");
            sb.AppendLine($"            public {className} Target;");
            sb.AppendLine($"            public void OnEvent(ref {eventType} e) => Target.{methodName}(ref e);");
            sb.AppendLine($"            public bool Equals({wrapperName} other) => Target == other.Target;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        private {wrapperName} {handlerFieldName};");
            sb.AppendLine();
        }

        sb.AppendLine($"        private void Generated_RegisterAll()");
        sb.AppendLine("        {");

        foreach (var method in methods)
        {
            var methodName = method.Name;
            var paramType = method.GetParameters()[0].ParameterType;
            var eventType = paramType.IsByRef ? paramType.GetElementType().FullName : paramType.FullName;
            var wrapperName = $"{methodName}_Wrapper";
            var handlerFieldName = $"{methodName}_handler";

            sb.AppendLine($"            {handlerFieldName} = new {wrapperName} {{ Target = this }};");
            sb.AppendLine($"            EventBus.Core.EventBus<{eventType}>.Register({handlerFieldName});");
        }

        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        private void Generated_UnregisterAll()");
        sb.AppendLine("        {");

        foreach (var method in methods)
        {
            var methodName = method.Name;
            var paramType = method.GetParameters()[0].ParameterType;
            var eventType = paramType.IsByRef ? paramType.GetElementType().FullName : paramType.FullName;
            var wrapperName = $"{methodName}_Wrapper";
            var handlerFieldName = $"{methodName}_handler";

            sb.AppendLine($"            EventBus.Core.EventBus<{eventType}>.Unregister({handlerFieldName});");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}