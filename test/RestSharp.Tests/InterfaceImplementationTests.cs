using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace RestSharp.Tests
{
    [TestFixture]
    public class InterfaceImplementationTests
    {
        static IEnumerable<string> CompareTypes(IReflect type1, IReflect type2, BindingFlags bindingFlags)
        {
            var typeTMethodInfo = type1.GetMethods(bindingFlags);
            var typeXMethodInfo = type2.GetMethods(bindingFlags);

            return typeTMethodInfo.Select(x => x.Name)
                .Except(typeXMethodInfo.Select(x => x.Name));
        }

        [Test]
        public void IRestSharp_Has_All_RestSharp_Signatures()
        {
            // Arrange
            var                restClientImplementationType = typeof(RestClient);
            var                restClientInterfaceType      = typeof(IRestClient);
            const BindingFlags bindingFlags                 = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            // Act
            var compareResult = CompareTypes(
                    restClientImplementationType, restClientInterfaceType,
                    bindingFlags
                )
                .ToList();

            compareResult.ForEach(
                x => Console.WriteLine(
                    "Method {0} exists in {1} but not in {2}", x,
                    restClientImplementationType.FullName, restClientInterfaceType.FullName
                )
            );

            // Assert
            Assert.AreEqual(0, compareResult.Count);
        }
    }
}