﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace RestSharp.Tests
{
    public class InterfaceImplementationTests
    {
        [Fact]
        public void IRestSharp_Has_All_RestSharp_Signatures()
        {
            // Arrange
            Type restClientImplementationType = typeof(RestClient);
            Type restClientInterfaceType = typeof(IRestClient);
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            // Act
            List<string> compareResult = CompareTypes(restClientImplementationType, restClientInterfaceType,
                bindingFlags).ToList();

            compareResult.ForEach(x => Console.WriteLine("Method {0} exists in {1} but not in {2}", x,
                restClientImplementationType.FullName, restClientInterfaceType.FullName));

            // Assert
            Assert.Equal(0, compareResult.Count);
        }

        private static IEnumerable<string> CompareTypes(Type type1, Type type2, BindingFlags bindingFlags)
        {
            MethodInfo[] typeTMethodInfo = type1.GetMethods(bindingFlags);
            MethodInfo[] typeXMethodInfo = type2.GetMethods(bindingFlags);

            return typeTMethodInfo.Select(x => x.Name)
                                  .Except(typeXMethodInfo.Select(x => x.Name));
        }
    }
}
