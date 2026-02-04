using UnityEngine;
using System.Collections.Generic;

namespace BingoGame.Utilities
{
    /// <summary>
    /// 服务定位器
    /// 用于全局服务访问（依赖注入）
    /// </summary>
    public class ServiceLocator
    {
        private static Dictionary<System.Type, object> services = new Dictionary<System.Type, object>();

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="service">服务实例</param>
        public static void RegisterService<T>(T service) where T : class
        {
            var serviceType = typeof(T);
            
            if (services.ContainsKey(serviceType))
            {
                Debug.LogWarning($"服务 {serviceType.Name} 已存在，将被覆盖");
            }
            
            services[serviceType] = service;
            Debug.Log($"注册服务: {serviceType.Name}");
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>服务实例</returns>
        public static T GetService<T>() where T : class
        {
            var serviceType = typeof(T);
            
            if (services.TryGetValue(serviceType, out var service))
            {
                return service as T;
            }
            
            Debug.LogWarning($"服务 {serviceType.Name} 未注册");
            return null;
        }

        /// <summary>
        /// 检查服务是否已注册
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>是否已注册</returns>
        public static bool HasService<T>() where T : class
        {
            return services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 注销服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        public static void UnregisterService<T>() where T : class
        {
            var serviceType = typeof(T);
            
            if (services.ContainsKey(serviceType))
            {
                services.Remove(serviceType);
                Debug.Log($"注销服务: {serviceType.Name}");
            }
        }

        /// <summary>
        /// 清除所有服务
        /// </summary>
        public static void ClearAllServices()
        {
            services.Clear();
            Debug.Log("清除所有服务");
        }
    }
}
