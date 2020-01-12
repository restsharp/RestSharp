using System;
using System.Security.Cryptography;
using NUnit.Framework;
using RestSharp.Extensions;

namespace RestSharp.Tests
{
    [TestFixture]
    public class RSACryptoServiceProviderExtensionsTests
    {
        [Test]
        public void FromXmlStringImpl_GivenInvalidPrivateKeyXml_ThrowsInvalidOperationException()
        {
            const string samplePrivateKeyXml =
                "<something></something>";

            using (var provider = new RSACryptoServiceProvider())
            {
                var exception = Assert.Throws<InvalidOperationException>(
                    () =>
                        RSACryptoServiceProviderExtensions.FromXmlStringImpl(provider, samplePrivateKeyXml)
                );
                Assert.AreEqual("Invalid XML RSA key.", exception.Message);
            }
        }

#if !NETCOREAPP
        [Test]
        public void FromXmlStringImpl_GivenPrivateKeyXml_GivesSameResultAsDotNetImplementation()
        {
            const string samplePrivateKeyXml =
                "<RSAKeyValue><Modulus>twJgSXtGu3QQKComA/6wgcTPFS6cky+EHA+fCAZm+Suz0KpiYqvk4LHV+MQQvVy1TpWjpC1iXtEa5BfMS8zDLfrXaXA6RSZ3QEw8YfmmMrKDwUULIORgqcW8Uybalp5fMdbOieAQNXpOLNjnjPZVmFrQvB+CzfltYo82aEiOTjk=</Modulus><Exponent>AQAB</Exponent><P>8x4Omo3kOOExZP/XbtWLHlW7WfEtJNXIATzYlpOQAM1+mwJ7qBAP2umzudUdfXJECMKyv1e+eVeb0WatIsj+vw==</P><Q>wLTwSuM+KG57O4VTddyBSXRHLJvahfWlB1VettJvcqgQk2zK4XwoZU7POjq5fx6kfAUyAYaaxHfwKhKBIy1pBw==</Q><DP>F3LRs8R1u6q0qeonLDB6f42DSXSChyf7Z2sn9LX80KcBTBAcPyR1cwbRZ94PPxczSqkEtoHPBEMX60V883rxXw==</DP><DQ>UQ/LxLSygO94hyEeaoXHHM784Zbt5Uvfj6YpoV4D44cu8dThwtgnZfYw1Z2+Serp5gGJd3rXv610KT5/c/y2IQ==</DQ><InverseQ>jV3wG0+jRpbnkpYLBMVFmLlhJ68oZnpI+fbVnm5mBMr3Rzytz2HfgaGpmI6MY+ni9JV0pfntKNT6uo/Jji34gQ==</InverseQ><D>D4MZDEFxvmPZFr5z2HTXGzjGYMJBrUwiw4ojbbe1NLuakz5N9pUhYlZQj7R2wsY/6/hNFZZvNyA8SkcmHuqtRGyEmE9JOzRA5YhxkC6rfy9oTR2ybIrv9mUGU7P76PBPO2VQJdIIgAdTXMIz8o3IOStINpEkGWzptQ1yxZ8Apx0=</D></RSAKeyValue>";

            using (var customBasedProvider = new RSACryptoServiceProvider())
            using (var dotnetBasedProvider = new RSACryptoServiceProvider())
            {
                RSACryptoServiceProviderExtensions.FromXmlStringImpl(customBasedProvider, samplePrivateKeyXml);
                dotnetBasedProvider.FromXmlString(samplePrivateKeyXml);

                var dotnetBasedParameters = customBasedProvider.ExportParameters(true);
                var customBasedParameters = customBasedProvider.ExportParameters(true);

                Assert.AreEqual(dotnetBasedParameters.D, customBasedParameters.D);
                Assert.AreEqual(dotnetBasedParameters.DP, customBasedParameters.DP);
                Assert.AreEqual(dotnetBasedParameters.DQ, customBasedParameters.DQ);
                Assert.AreEqual(dotnetBasedParameters.Exponent, customBasedParameters.Exponent);
                Assert.AreEqual(dotnetBasedParameters.InverseQ, customBasedParameters.InverseQ);
                Assert.AreEqual(dotnetBasedParameters.Modulus, customBasedParameters.Modulus);
                Assert.AreEqual(dotnetBasedParameters.P, customBasedParameters.P);
                Assert.AreEqual(dotnetBasedParameters.Q, customBasedParameters.Q);
            }
        }
#endif

        [Test]
        public void FromXmlStringImpl_GivenPrivateKeyXmlWithUnknownNode_ThrowsInvalidOperationException()
        {
            const string samplePrivateKeyXml =
                "<RSAKeyValue><pi>unexpected</pi></RSAKeyValue>";

            using (var provider = new RSACryptoServiceProvider())
            {
                var exception = Assert.Throws<InvalidOperationException>(
                    () =>
                        RSACryptoServiceProviderExtensions.FromXmlStringImpl(provider, samplePrivateKeyXml)
                );
                Assert.AreEqual("Unknown node name: pi", exception.Message);
            }
        }
    }
}