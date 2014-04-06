namespace RestSharp.Deserializers
{
    using System;
    using System.Collections.Generic;

    public abstract class DeserializerBase
    {
        public TypeResolverDelegate DeserializationResolver { get; set; }

        protected object CreateInstance(Type type)
        {
            // first lets try to resolve the type provided. If we can't resolve, we can fall back to the activator.
            object instance;

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) || type.GetGenericTypeDefinition() == typeof(IList<>)))
            {
                instance =
                    Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]));
            }
            else if (type.IsInterface || type.IsAbstract)
            {
                if (this.DeserializationResolver == null)
                {
                    throw new InvalidOperationException("Unable to resolve instance & abstract type. DeserializationResolver not provided.");
                }

                instance = this.DeserializationResolver(type);

                if (instance == null)
                {
                    throw new InvalidOperationException("Unable to resolve instance or abstract type [" + type + "].");
                }
            }
            else
            {
                instance = Activator.CreateInstance(type);
            }

            return instance;
        }
    }
}
