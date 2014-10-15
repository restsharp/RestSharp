namespace RestSharp.Tests.Fakes
{
    using System;

    public static class FakeResolver
    {
        public static TImplementation Resolve<TType, TImplementation>() 
            where TType : class
            where TImplementation : class, TType, new()
        {
            return new TImplementation();
        }

        public static object ResolveAbstractFoo(Type arg)
        {
            return arg == typeof(AbstractFoo) ? new ActualAbstractFoo() : null;
        }
    }
}
