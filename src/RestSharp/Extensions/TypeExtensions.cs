using System;
using System.Reflection;

namespace RestSharp.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsGenericType(this Type type)
        {
#if NET35 || NET40
            return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        public static Type[] GetGenericTypeArguments(this Type type)
        {
#if NET35 || NET40
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                return type.GetGenericArguments();
            }
            return Type.EmptyTypes;
#else
            return type.GenericTypeArguments;
#endif
        }
    }
}
