using System;

namespace EventBus.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SubscribeAttribute : Attribute
    {
    }
}
