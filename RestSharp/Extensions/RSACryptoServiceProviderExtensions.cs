using System;
using System.Security.Cryptography;
using System.Xml;

namespace RestSharp.Extensions
{
    public static class RSACryptoServiceProviderExtensions
    {
        /// <summary>
        /// Imports the specified XML String into the crypto service provider
        /// </summary>
        /// <remarks>
        ///  .NET Core 2.0 doesn't provide an implementation of RSACryptoServiceProvider.FromXmlString/ToXmlString, so we have to do it ourselves.
        /// Source: https://gist.github.com/Jargon64/5b172c452827e15b21882f1d76a94be4/
        /// </remarks>
        public static void FromXmlString2(this RSACryptoServiceProvider rsa, string xmlString)
        {
#if !NETSTANDARD2_0
            rsa.FromXmlString(xmlString);
#else
            FromXmlStringImpl(rsa, xmlString);
#endif
        }

        internal static void FromXmlStringImpl(RSACryptoServiceProvider rsa, string xmlString)
        {
            var parameters = new RSAParameters();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (!xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                throw new InvalidOperationException("Invalid XML RSA key.");
            }


            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Modulus":
                        parameters.Modulus = Convert.FromBase64String(node.InnerText);
                        break;
                    case "Exponent":
                        parameters.Exponent = Convert.FromBase64String(node.InnerText);
                        break;
                    case "P":
                        parameters.P = Convert.FromBase64String(node.InnerText);
                        break;
                    case "Q":
                        parameters.Q = Convert.FromBase64String(node.InnerText);
                        break;
                    case "DP":
                        parameters.DP = Convert.FromBase64String(node.InnerText);
                        break;
                    case "DQ":
                        parameters.DQ = Convert.FromBase64String(node.InnerText);
                        break;
                    case "InverseQ":
                        parameters.InverseQ = Convert.FromBase64String(node.InnerText);
                        break;
                    case "D":
                        parameters.D = Convert.FromBase64String(node.InnerText);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown node name: " + node.Name);
                }
            }

            rsa.ImportParameters(parameters);
        }
    }
}
