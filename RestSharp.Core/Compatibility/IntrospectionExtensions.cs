namespace System.Reflection
{
    /// <summary>
    /// https://github.com/castleproject/Core/blob/netcore/src/Castle.Core/Compatibility/IntrospectionExtensions.cs
    /// </summary>
	internal static class IntrospectionExtensions
	{
#if NET35 || NET40 || PORTABLE || SILVERLIGHT || WPSL
        // This allows us to use the new reflection API which separates Type and TypeInfo
        // while still supporting .NET 3.5 and 4.0. This class matches the API of the same
        // class in .NET 4.5+, and so is only needed on .NET Framework versions before that.
        //
        // Return the System.Type for now, we will probably need to create a TypeInfo class
        // which inherits from Type like .NET 4.5+ and implement the additional methods and
        // properties.
        public static Type GetTypeInfo(this Type type)
		{
			return type;
		}

        //public static Type[] GetGenericTypeArguments(this Type typeInfo)
        //{
        //    return typeInfo.GetGenericArguments();
        //}
#else
        //public static Type[] GetGenericTypeArguments(this TypeInfo typeInfo)
        //{
        //    return typeInfo.GenericTypeArguments;
        //}
#endif
    }
}