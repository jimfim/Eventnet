using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventNet.Core
{
    public static class RedirectToWhen
    {
        private static readonly MethodInfo InternalPreserveStackTraceMethod =
            typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);


        public static void InvokeEventOptional<T>(T instance, object command)
        {
            var type = command.GetType();
            var eventMethod = GetEventMethod(instance.GetType());
            if (!eventMethod.TryGetValue(type, out var info)) return;
            try
            {
                info.Invoke(instance, new[] {command});
            }
            catch (TargetInvocationException ex)
            {
                if (InternalPreserveStackTraceMethod != null)
                {
                    InternalPreserveStackTraceMethod.Invoke(ex.InnerException, new object[0]);
                }

                throw ex.InnerException;
            }
        }

        private static Dictionary<Type, MethodInfo> GetEventMethod(IReflect type)
        {
            return type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == "When")
                .Where(m => m.GetParameters().Length == 1)
                .ToDictionary(m => m.GetParameters().First().ParameterType, m => m);
        }
    }
}