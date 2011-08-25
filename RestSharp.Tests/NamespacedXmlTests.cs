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
using RestSharp.Deserializers;
using RestSharp.Tests.SampleClasses.Lastfm;
using Xunit;

namespace RestSharp.Tests
{
	public class NamespacedXmlTests
	{
		private const string GuidString = "AC1FC4BC-087A-4242-B8EE-C53EBE9887A5";

		[Fact]
		public void Can_Deserialize_Elements_With_Namespace() {
			var doc = CreateElementsXml();

			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			d.Namespace = "http://restsharp.org";
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);
			Assert.Equal(new Guid(GuidString), p.UniqueId);
			Assert.Equal(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
			Assert.Equal(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);

			Assert.NotNull(p.Friends);
			Assert.Equal(10, p.Friends.Count);

			Assert.NotNull(p.BestFriend);
			Assert.Equal("The Fonz", p.BestFriend.Name);
			Assert.Equal(1952, p.BestFriend.Since);
		}

		[Fact]
		public void Can_Deserialize_Elements_With_Namespace_Autodetect_Namespace() {
			var doc = CreateElementsXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);
			Assert.Equal(new Guid(GuidString), p.UniqueId);
			Assert.Equal(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
			Assert.Equal(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);

			Assert.NotNull(p.Friends);
			Assert.Equal(10, p.Friends.Count);

			Assert.NotNull(p.BestFriend);
			Assert.Equal("The Fonz", p.BestFriend.Name);
			Assert.Equal(1952, p.BestFriend.Since);
		}

		[Fact]
		public void Can_Deserialize_Attributes_With_Namespace() {
			var doc = CreateAttributesXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			d.Namespace = "http://restsharp.org";
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);
			Assert.Equal(new Guid(GuidString), p.UniqueId);
			Assert.Equal(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
			Assert.Equal(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);

			Assert.NotNull(p.BestFriend);
			Assert.Equal("The Fonz", p.BestFriend.Name);
			Assert.Equal(1952, p.BestFriend.Since);
		}

		[Fact]
		public void Ignore_Protected_Property_That_Exists_In_Data() {
			var doc = CreateElementsXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			d.Namespace = "http://restsharp.org";
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Null(p.IgnoreProxy);
		}

		[Fact]
		public void Ignore_ReadOnly_Property_That_Exists_In_Data() {
			var doc = CreateElementsXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			d.Namespace = "http://restsharp.org";
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Null(p.ReadOnlyProxy);
		}

		[Fact]
		public void Can_Deserialize_Names_With_Underscores_With_Namespace() {
			var doc = CreateUnderscoresXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			d.Namespace = "http://restsharp.org";
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);
			Assert.Equal(new Guid(GuidString), p.UniqueId);
			Assert.Equal(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
			Assert.Equal(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);

			Assert.NotNull(p.Friends);
			Assert.Equal(10, p.Friends.Count);

			Assert.NotNull(p.BestFriend);
			Assert.Equal("The Fonz", p.BestFriend.Name);
			Assert.Equal(1952, p.BestFriend.Since);

			Assert.NotNull(p.Foes);
			Assert.Equal(5, p.Foes.Count);
			Assert.Equal("Yankees", p.Foes.Team);
		}

		[Fact]
		public void Can_Deserialize_List_Of_Primitives_With_Namespace() {
			var doc = CreateListOfPrimitivesXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			d.Namespace = "http://restsharp.org";
			var a = d.Deserialize<List<artist>>(response);

			Assert.Equal(2, a.Count);
			Assert.Equal("first", a[0].Value);
			Assert.Equal("second", a[1].Value);
		}

		private static string CreateListOfPrimitivesXml() {
			var doc = new XDocument();
			var ns = XNamespace.Get("http://restsharp.org");
			var root = new XElement(ns + "artists");
			root.Add(new XElement(ns + "artist", "first"));
			root.Add(new XElement(ns + "artist", "second"));
			doc.Add(root);
			return doc.ToString();
		}

		private static string CreateUnderscoresXml() {
			var doc = new XDocument();
			var ns = XNamespace.Get("http://restsharp.org");
			var root = new XElement(ns + "Person");
			root.Add(new XElement(ns + "Name", "John Sheehan"));
			root.Add(new XElement(ns + "Start_Date", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XAttribute(ns + "Age", 28));
			root.Add(new XElement(ns + "Percent", 99.9999m));
			root.Add(new XElement(ns + "Big_Number", long.MaxValue));
			root.Add(new XAttribute(ns + "Is_Cool", false));
			root.Add(new XElement(ns + "Ignore", "dummy"));
			root.Add(new XAttribute(ns + "Read_Only", "dummy"));
			root.Add(new XAttribute(ns + "Unique_Id", new Guid(GuidString)));
			root.Add(new XElement(ns + "Url", "http://example.com"));
			root.Add(new XElement(ns + "Url_Path", "/foo/bar"));

			root.Add(new XElement(ns + "Best_Friend",
						new XElement(ns + "Name", "The Fonz"),
						new XAttribute(ns + "Since", 1952)
					));

			var friends = new XElement(ns + "Friends");
			for (int i = 0; i < 10; i++) {
				friends.Add(new XElement(ns + "Friend",
								new XElement(ns + "Name", "Friend" + i),
								new XAttribute(ns + "Since", DateTime.Now.Year - i)
							));
			}
			root.Add(friends);

			var foes = new XElement(ns + "Foes");
			foes.Add(new XAttribute(ns + "Team", "Yankees"));
			for (int i = 0; i < 5; i++) {
				foes.Add(new XElement(ns + "Foe", new XElement(ns + "Nickname", "Foe" + i)));
			}
			root.Add(foes);

			doc.Add(root);
			return doc.ToString();
		}

		private static string CreateElementsXml() {
			var doc = new XDocument();
			var ns = XNamespace.Get("http://restsharp.org");
			var root = new XElement(ns + "Person");
			root.Add(new XElement(ns + "Name", "John Sheehan"));
			root.Add(new XElement(ns + "StartDate", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XElement(ns + "Age", 28));
			root.Add(new XElement(ns + "Percent", 99.9999m));
			root.Add(new XElement(ns + "BigNumber", long.MaxValue));
			root.Add(new XElement(ns + "IsCool", false));
			root.Add(new XElement(ns + "Ignore", "dummy"));
			root.Add(new XElement(ns + "ReadOnly", "dummy"));
			root.Add(new XElement(ns + "UniqueId", new Guid(GuidString)));
			root.Add(new XElement(ns + "Url", "http://example.com"));
			root.Add(new XElement(ns + "UrlPath", "/foo/bar"));

			root.Add(new XElement(ns + "BestFriend",
						new XElement(ns + "Name", "The Fonz"),
						new XElement(ns + "Since", 1952)
					));

			var friends = new XElement(ns + "Friends");
			for (int i = 0; i < 10; i++) {
				friends.Add(new XElement(ns + "Friend",
								new XElement(ns + "Name", "Friend" + i),
								new XElement(ns + "Since", DateTime.Now.Year - i)
							));
			}
			root.Add(friends);

			doc.Add(root);
			return doc.ToString();
		}

		private static string CreateAttributesXml() {
			var doc = new XDocument();
			var ns = XNamespace.Get("http://restsharp.org");
			var root = new XElement(ns + "Person");
			root.Add(new XAttribute(ns + "Name", "John Sheehan"));
			root.Add(new XAttribute(ns + "StartDate", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XAttribute(ns + "Age", 28));
			root.Add(new XAttribute(ns + "Percent", 99.9999m));
			root.Add(new XAttribute(ns + "BigNumber", long.MaxValue));
			root.Add(new XAttribute(ns + "IsCool", false));
			root.Add(new XAttribute(ns + "Ignore", "dummy"));
			root.Add(new XAttribute(ns + "ReadOnly", "dummy"));
			root.Add(new XAttribute(ns + "UniqueId", new Guid(GuidString)));
			root.Add(new XAttribute(ns + "Url", "http://example.com"));
			root.Add(new XAttribute(ns + "UrlPath", "/foo/bar"));

			root.Add(new XElement(ns + "BestFriend",
						new XAttribute(ns + "Name", "The Fonz"),
						new XAttribute(ns + "Since", 1952)
					));

			doc.Add(root);
			return doc.ToString();
		}
	}
}
