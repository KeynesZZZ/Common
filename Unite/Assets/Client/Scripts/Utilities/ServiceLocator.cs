using System;
using System.Collections.Generic;

namespace BingoClient.Utilities
{
    /// <summary>
    /// 服务定位器 - 实现依赖注入模式
    /// 提供全局服务注册和获取功能，避免组件间的直接依赖
    /// </summary>
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        private readonly Dictionary<Type, object> _services = new();

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLocator();
                }
                return _instance;
            }
        }

        public void RegisterService<T>(T service)
        {
            var serviceType = typeof(T);
            _services[serviceType] = service;
        }

        public T GetService<T>()
        {
            var serviceType = typeof(T);
            if (_services.TryGetValue(serviceType, out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
        }

        public bool TryGetService<T>(out T service)
        {
            var serviceType = typeof(T);
            if (_services.TryGetValue(serviceType, out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = default;
            return false;
        }
    }
}