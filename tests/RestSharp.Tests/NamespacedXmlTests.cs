#region Licensed

//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

#endregion

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NUnit.Framework;
using RestSharp.Deserializers;
using RestSharp.Tests.SampleClasses;
using RestSharp.Tests.SampleClasses.Lastfm;

namespace RestSharp.Tests
{
    [TestFixture]
    public class NamespacedXmlTests
    {
        private const string GUID_STRING = "AC1FC4BC-087A-4242-B8EE-C53EBE9887A5";

        [Test]
        public void Can_Deserialize_Elements_With_Namespace()
        {
            string doc = CreateElementsXml();
            RestResponse response = new RestResponse { Content = doc };
            XmlDeserializer d = new XmlDeserializer { Namespace = "http://restsharp.org" };
            PersonForXml p = d.Deserialize<PersonForXml>(response);

            Assert.AreEqual("John Sheehan", p.Name);
            Assert.AreEqual(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
            Assert.AreEqual(28, p.Age);
            Assert.AreEqual(long.MaxValue, p.BigNumber);
            Assert.AreEqual(99.9999m, p.Percent);
            Assert.AreEqual(false, p.IsCool);
            Assert.AreEqual(new Guid(GUID_STRING), p.UniqueId);
            Assert.AreEqual(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
            Assert.AreEqual(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);
            Assert.NotNull(p.Friends);
            Assert.AreEqual(10, p.Friends.Count);
            Assert.NotNull(p.BestFriend);
            Assert.AreEqual("The Fonz", p.BestFriend.Name);
            Assert.AreEqual(1952, p.BestFriend.Since);
        }

        [Test]
        public void Can_Deserialize_Elements_With_Namespace_Autodetect_Namespace()
        {
            string doc = CreateElementsXml();
            RestResponse response = new RestResponse { Content = doc };
            XmlDeserializer d = new XmlDeserializer();
            PersonForXml p = d.Deserialize<PersonForXml>(response);

            Assert.AreEqual("John Sheehan", p.Name);
            Assert.AreEqual(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
            Assert.AreEqual(28, p.Age);
            Assert.AreEqual(long.MaxValue, p.BigNumber);
            Assert.AreEqual(99.9999m, p.Percent);
            Assert.AreEqual(false, p.IsCool);
            Assert.AreEqual(new Guid(GUID_STRING), p.UniqueId);
            Assert.AreEqual(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
            Assert.AreEqual(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);
            Assert.NotNull(p.Friends);
            Assert.AreEqual(10, p.Friends.Count);
            Assert.NotNull(p.BestFriend);
            Assert.AreEqual("The Fonz", p.BestFriend.Name);
            Assert.AreEqual(1952, p.BestFriend.Since);
        }

        [Test]
        public void Can_Deserialize_Attributes_With_Namespace()
        {
            string doc = CreateAttributesXml();
            RestResponse response = new RestResponse { Content = doc };
            XmlDeserializer d = new XmlDeserializer { Namespace = "http://restsharp.org" };
            PersonForXml p = d.Deserialize<PersonForXml>(response);

            Assert.AreEqual("John Sheehan", p.Name);
            Assert.AreEqual(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
            Assert.AreEqual(28, p.Age);
            Assert.AreEqual(long.MaxValue, p.BigNumber);
            Assert.AreEqual(99.9999m, p.Percent);
            Assert.AreEqual(false, p.IsCool);
            Assert.AreEqual(new Guid(GUID_STRING), p.UniqueId);
            Assert.AreEqual(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
            Assert.AreEqual(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);
            Assert.NotNull(p.BestFriend);
            Assert.AreEqual("The Fonz", p.BestFriend.Name);
            Assert.AreEqual(1952, p.BestFriend.Since);
        }

        [Test]
        public void Ignore_Protected_Property_That_Exists_In_Data()
        {
            string doc = CreateElementsXml();
            RestResponse response = new RestResponse { Content = doc };
            XmlDeserializer d = new XmlDeserializer { Namespace = "http://restsharp.org" };
            PersonForXml p = d.Deserialize<PersonForXml>(response);

            Assert.Null(p.IgnoreProxy);
        }

        [Test]
        public void Ignore_ReadOnly_Property_That_Exists_In_Data()
        {
            string doc = CreateElementsXml();
            RestResponse response = new RestResponse { Content = doc };
            XmlDeserializer d = new XmlDeserializer { Namespace = "http://restsharp.org" };
            PersonForXml p = d.Deserialize<PersonForXml>(response);

            Assert.Null(p.ReadOnlyProxy);
        }

        [Test]
        public void Can_Deserialize_Names_With_Underscores_With_Namespace()
        {
            string doc = CreateUnderscoresXml();
            RestResponse response = new RestResponse { Content = doc };
            XmlDeserializer d = new XmlDeserializer { Namespace = "http://restsharp.org" };
            PersonForXml p = d.Deserialize<PersonForXml>(response);

            Assert.AreEqual("John Sheehan", p.Name);
            Assert.AreEqual(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
            Assert.AreEqual(28, p.Age);
            Assert.AreEqual(long.MaxValue, p.BigNumber);
            Assert.AreEqual(99.9999m, p.Percent);
            Assert.AreEqual(false, p.IsCool);
            Assert.AreEqual(new Guid(GUID_STRING), p.UniqueId);
            Assert.AreEqual(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
            Assert.AreEqual(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);
            Assert.NotNull(p.Friends);
            Assert.AreEqual(10, p.Friends.Count);
            Assert.NotNull(p.BestFriend);
            Assert.AreEqual("The Fonz", p.BestFriend.Name);
            Assert.AreEqual(1952, p.BestFriend.Since);
            Assert.NotNull(p.Foes);
            Assert.AreEqual(5, p.Foes.Count);
            Assert.AreEqual("Yankees", p.Foes.Team);
        }

        [Test]
        public void Can_Deserialize_List_Of_Primitives_With_Namespace()
        {
            string doc = CreateListOfPrimitivesXml();
            RestResponse response = new RestResponse { Content = doc };
            XmlDeserializer d = new XmlDeserializer { Namespace = "http://restsharp.org" };
            List<artist> a = d.Deserialize<List<artist>>(response);

            Assert.AreEqual(2, a.Count);
            Assert.AreEqual("first", a[0].Value);
            Assert.AreEqual("second", a[1].Value);
        }

        private static string CreateListOfPrimitivesXml()
        {
            XDocument doc = new XDocument();
            XNamespace ns = XNamespace.Get("http://restsharp.org");
            XElement root = new XElement(ns + "artists");

            root.Add(new XElement(ns + "artist", "first"));
            root.Add(new XElement(ns + "artist", "second"));
            doc.Add(root);

            return doc.ToString();
        }

        private static string CreateUnderscoresXml()
        {
            XDocument doc = new XDocument();
            XNamespace ns = XNamespace.Get("http://restsharp.org");
            XElement root = new XElement(ns + "Person");

            root.Add(new XElement(ns + "Name", "John Sheehan"));
            root.Add(new XElement(ns + "Start_Date", new DateTime(2009, 9, 25, 0, 6, 1)));
            root.Add(new XAttribute(ns + "Age", 28));
            root.Add(new XElement(ns + "Percent", 99.9999m));
            root.Add(new XElement(ns + "Big_Number", long.MaxValue));
            root.Add(new XAttribute(ns + "Is_Cool", false));
            root.Add(new XElement(ns + "Ignore", "dummy"));
            root.Add(new XAttribute(ns + "Read_Only", "dummy"));
            root.Add(new XAttribute(ns + "Unique_Id", new Guid(GUID_STRING)));
            root.Add(new XElement(ns + "Url", "http://example.com"));
            root.Add(new XElement(ns + "Url_Path", "/foo/bar"));
            root.Add(new XElement(ns + "Best_Friend",
                new XElement(ns + "Name", "The Fonz"),
                new XAttribute(ns + "Since", 1952)));

            XElement friends = new XElement(ns + "Friends");

            for (int i = 0; i < 10; i++)
            {
                friends.Add(new XElement(ns + "Friend",
                    new XElement(ns + "Name", "Friend" + i),
                    new XAttribute(ns + "Since", DateTime.Now.Year - i)));
            }

            root.Add(friends);

            XElement foes = new XElement(ns + "Foes");

            foes.Add(new XAttribute(ns + "Team", "Yankees"));

            for (int i = 0; i < 5; i++)
            {
                foes.Add(new XElement(ns + "Foe", new XElement(ns + "Nickname", "Foe" + i)));
            }

            root.Add(foes);
            doc.Add(root);

            return doc.ToString();
        }

        private static string CreateElementsXml()
        {
            XDocument doc = new XDocument();
            XNamespace ns = XNamespace.Get("http://restsharp.org");
            XElement root = new XElement(ns + "Person");

            root.Add(new XElement(ns + "Name", "John Sheehan"));
            root.Add(new XElement(ns + "StartDate", new DateTime(2009, 9, 25, 0, 6, 1)));
            root.Add(new XElement(ns + "Age", 28));
            root.Add(new XElement(ns + "Percent", 99.9999m));
            root.Add(new XElement(ns + "BigNumber", long.MaxValue));
            root.Add(new XElement(ns + "IsCool", false));
            root.Add(new XElement(ns + "Ignore", "dummy"));
            root.Add(new XElement(ns + "ReadOnly", "dummy"));
            root.Add(new XElement(ns + "UniqueId", new Guid(GUID_STRING)));
            root.Add(new XElement(ns + "Url", "http://example.com"));
            root.Add(new XElement(ns + "UrlPath", "/foo/bar"));
            root.Add(new XElement(ns + "BestFriend",
                new XElement(ns + "Name", "The Fonz"),
                new XElement(ns + "Since", 1952)));

            XElement friends = new XElement(ns + "Friends");

            for (int i = 0; i < 10; i++)
            {
                friends.Add(new XElement(ns + "Friend",
                    new XElement(ns + "Name", "Friend" + i),
                    new XElement(ns + "Since", DateTime.Now.Year - i)));
            }

            root.Add(friends);
            root.Add(new XElement(ns + "FavoriteBand",
                new XElement(ns + "Name", "Goldfinger")));

            doc.Add(root);

            return doc.ToString();
        }

        private static string CreateAttributesXml()
        {
            XDocument doc = new XDocument();
            XNamespace ns = XNamespace.Get("http://restsharp.org");
            XElement root = new XElement(ns + "Person");

            root.Add(new XAttribute(ns + "Name", "John Sheehan"));
            root.Add(new XAttribute(ns + "StartDate", new DateTime(2009, 9, 25, 0, 6, 1)));
            root.Add(new XAttribute(ns + "Age", 28));
            root.Add(new XAttribute(ns + "Percent", 99.9999m));
            root.Add(new XAttribute(ns + "BigNumber", long.MaxValue));
            root.Add(new XAttribute(ns + "IsCool", false));
            root.Add(new XAttribute(ns + "Ignore", "dummy"));
            root.Add(new XAttribute(ns + "ReadOnly", "dummy"));
            root.Add(new XAttribute(ns + "UniqueId", new Guid(GUID_STRING)));
            root.Add(new XAttribute(ns + "Url", "http://example.com"));
            root.Add(new XAttribute(ns + "UrlPath", "/foo/bar"));
            root.Add(new XElement(ns + "BestFriend",
                new XAttribute(ns + "Name", "The Fonz"),
                new XAttribute(ns + "Since", 1952)));

            doc.Add(root);

            return doc.ToString();
        }
    }
}
