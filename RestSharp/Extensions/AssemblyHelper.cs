using System;
using System.Reflection;

namespace RestSharp.Extensions
{
    public static class AssemblyHelper
    {
        public static Version GetVersion(Type type)
        {
#if WINDOWS_UWP || NETSTANDARD
        var asm = type.GetTypeInfo().Assembly;
        return new AssemblyName(asm.FullName).Version;    
#else
            return new AssemblyName(Assembly.GetExecutingAssembly().FullName).Version;
#endif
        }
    }
}