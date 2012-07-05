#region License
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RestSharp.Deserializers;
using Xunit;
using RestSharp.Tests.SampleClasses;
using System.Collections.Generic;

namespace RestSharp.Tests
{
	public class XmlTests
	{
		private const string GuidString = "AC1FC4BC-087A-4242-B8EE-C53EBE9887A5";
		private string SampleDataPath = Path.Combine(Environment.CurrentDirectory, "SampleData");
		
		private string PathFor(string sampleFile)
		{
			return Path.Combine(SampleDataPath, sampleFile);
		}

		[Fact]
		public void Can_Deserialize_Lists_of_Simple_Types()
		{
			var xmlpath = PathFor("xmllists.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<SimpleTypesListSample>(new RestResponse() { Content = doc.ToString() });

			Assert.NotEmpty(output.Names);
			Assert.NotEmpty(output.Numbers);
			Assert.False(output.Names[0].Length == 0);
			Assert.False(output.Numbers.Sum() == 0);
		}

		[Fact]
		public void Can_Deserialize_To_List_Inheritor_From_Custom_Root_With_Attributes()
		{
			var xmlpath = PathFor("ListWithAttributes.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlDeserializer();
			xml.RootElement = "Calls";
			var output = xml.Deserialize<TwilioCallList>(new RestResponse { Content = doc.ToString() });

			Assert.Equal(3, output.NumPages);
			Assert.NotEmpty(output);
			Assert.Equal(2, output.Count);
		}

		[Fact]
		public void Can_Deserialize_To_Standalone_List_Without_Matching_Class_Case()
		{
			var xmlpath = PathFor("InlineListSample.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<List<Image>>(new RestResponse { Content = doc.ToString() });

			Assert.NotEmpty(output);
			Assert.Equal(4, output.Count);
		}

		[Fact]
		public void Can_Deserialize_To_Standalone_List_With_Matching_Class_Case()
		{
			var xmlpath = PathFor("InlineListSample.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<List<image>>(new RestResponse { Content = doc.ToString() });

			Assert.NotEmpty(output);
			Assert.Equal(4, output.Count);
		}

		[Fact]
		public void Can_Deserialize_Directly_To_Lists_Off_Root_Element()
		{
			var xmlpath = PathFor("directlists.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<List<Database>>(new RestResponse { Content = doc.ToString() });

			Assert.NotEmpty(output);
			Assert.Equal(2, output.Count);
		}

		[Fact]
		public void Can_Deserialize_Parentless_aka_Inline_List_Items_Without_Matching_Class_Name()
		{
			var xmlpath = PathFor("InlineListSample.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<InlineListSample>(new RestResponse { Content = doc.ToString() });

			Assert.NotEmpty(output.Images);
			Assert.Equal(4, output.Images.Count);
		}

		[Fact]
		public void Can_Deserialize_Parentless_aka_Inline_List_Items_Without_Matching_Class_Name_Using_XmlAttributeDeserializer()
		{
			var xmlpath = PathFor("InlineListSample.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlAttributeDeserializer();
			var output = xml.Deserialize<InlineListSample>(new RestResponse { Content = doc.ToString() });

			Assert.NotEmpty(output.Images);
			Assert.Equal(4, output.Images.Count);
		}

		[Fact]
		public void Can_Deserialize_Parentless_aka_Inline_List_Items_With_Matching_Class_Name()
		{
			var xmlpath = PathFor("InlineListSample.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<InlineListSample>(new RestResponse { Content = doc.ToString() });

			Assert.NotEmpty(output.images);
			Assert.Equal(4, output.images.Count);
		}

		[Fact]
		public void Can_Deserialize_Parentless_aka_Inline_List_Items_With_Matching_Class_Name_Using_XmlAttributeDeserializer()
		{
			var xmlpath = PathFor("InlineListSample.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlAttributeDeserializer();
			var output = xml.Deserialize<InlineListSample>(new RestResponse { Content = doc.ToString() });

			Assert.NotEmpty(output.images);
			Assert.Equal(4, output.images.Count);
		}

		[Fact]
		public void Can_Deserialize_Parentless_aka_Inline_List_Items_With_Matching_Class_Name_With_Additional_Property()
		{
			var xmlpath = PathFor("InlineListSample.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<InlineListSample>(new RestResponse { Content = doc.ToString() });

			Assert.Equal(4, output.Count);
		}

		[Fact]
		public void Can_Deserialize_Nested_List_Items_Without_Matching_Class_Name()
		{
			var xmlpath = PathFor("NestedListSample.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<InlineListSample>(new RestResponse { Content = doc.ToString() });

			Assert.NotEmpty(output.Images);
			Assert.Equal(4, output.Images.Count);
		}


		[Fact]
		public void Can_Deserialize_Nested_List_Items_With_Matching_Class_Name()
		{
			var xmlpath = PathFor("NestedListSample.xml");
			var doc = XDocument.Load(xmlpath);

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<InlineListSample>(new RestResponse { Content = doc.ToString() });

			Assert.NotEmpty(output.images);
			Assert.Equal(4, output.images.Count);
		}

		[Fact]
		public void Can_Deserialize_Nested_List_Without_Elements_To_Empty_List()
		{
			var doc = CreateXmlWithEmptyNestedList();

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<EmptyListSample>(new RestResponse { Content = doc });

			Assert.NotNull(output.images);
			Assert.NotNull(output.Images);
			Assert.Empty(output.images);
			Assert.Empty(output.Images);
		}

		[Fact]
		public void Can_Deserialize_Inline_List_Without_Elements_To_Empty_List()
		{
			var doc = CreateXmlWithEmptyInlineList();

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<EmptyListSample>(new RestResponse { Content = doc });

			Assert.NotNull(output.images);
			Assert.NotNull(output.Images);
			Assert.Empty(output.images);
			Assert.Empty(output.Images);
		}

		[Fact]
		public void Can_Deserialize_Empty_Elements_to_Nullable_Values()
		{
			var doc = CreateXmlWithNullValues();

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<NullableValues>(new RestResponse { Content = doc });

			Assert.Null(output.Id);
			Assert.Null(output.StartDate);
			Assert.Null(output.UniqueId);
		}

		[Fact]
		public void Can_Deserialize_Elements_to_Nullable_Values()
		{
			var culture = CultureInfo.InvariantCulture;
			var doc = CreateXmlWithoutEmptyValues(culture);
			var xml = new XmlDeserializer() {Culture = culture};
			var output = xml.Deserialize<NullableValues>(new RestResponse { Content = doc });

			Assert.NotNull(output.Id);
			Assert.NotNull(output.StartDate);
			Assert.NotNull(output.UniqueId);

			Assert.Equal(123, output.Id);
			Assert.Equal(new DateTime(2010, 2, 21, 9, 35, 00), output.StartDate);
			Assert.Equal(new Guid(GuidString), output.UniqueId);
		}

        [Fact]
        public void Can_Deserialize_TimeSpan()
        {
            var culture = CultureInfo.InvariantCulture;
            var doc = new XDocument(culture);

            TimeSpan? nullTimespan = null;
            TimeSpan? nullValueTimeSpan = new TimeSpan(21, 30, 7);

            var root = new XElement("Person");
            root.Add(new XElement("Tick", new TimeSpan(468006)));
            root.Add(new XElement("Millisecond", new TimeSpan(0, 0, 0, 0, 125)));
            root.Add(new XElement("Second", new TimeSpan(0, 0, 8)));
            root.Add(new XElement("Minute", new TimeSpan(0, 55, 2)));
            root.Add(new XElement("Hour", new TimeSpan(21, 30, 7)));
            root.Add(new XElement("NullableWithoutValue", nullTimespan));
            root.Add(new XElement("NullableWithValue", nullValueTimeSpan));

            doc.Add(root);

            var xml = new XmlDeserializer
            {
                Culture = culture,
            };

            var response = new RestResponse { Content = doc.ToString() };

            var d = new XmlDeserializer()
            {
                Culture = culture,
            };
            var payload = d.Deserialize<TimeSpanTestStructure>(response);
            Assert.Equal(new TimeSpan(468006), payload.Tick);
            Assert.Equal(new TimeSpan(0, 0, 0, 0, 125), payload.Millisecond);
            Assert.Equal(new TimeSpan(0, 0, 8), payload.Second);
            Assert.Equal(new TimeSpan(0, 55, 2), payload.Minute);
            Assert.Equal(new TimeSpan(21, 30, 7), payload.Hour);
            Assert.Null(payload.NullableWithoutValue);
            Assert.NotNull(payload.NullableWithValue);
            Assert.Equal(new TimeSpan(21, 30, 7), payload.NullableWithValue.Value);
        }

		[Fact]
		public void Can_Deserialize_Custom_Formatted_Date()
		{
			var culture = CultureInfo.InvariantCulture;
			var format = "dd yyyy MMM, hh:mm ss tt zzz";
			var date = new DateTime(2010, 2, 8, 11, 11, 11);

			var doc = new XDocument();

			var root = new XElement("Person");
			root.Add(new XElement("StartDate", date.ToString(format, culture)));

			doc.Add(root);

			var xml = new XmlDeserializer
			{
				DateFormat = format,
				Culture = culture
			};

			var response = new RestResponse { Content = doc.ToString() };
			var output = xml.Deserialize<PersonForXml>(response);

			Assert.Equal(date, output.StartDate);
		}

		[Fact]
		public void Can_Deserialize_Elements_On_Default_Root()
		{
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
			Assert.Equal(Guid.Empty, p.EmptyGuid);

			Assert.Equal(new Uri("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
			Assert.Equal(new Uri("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);

			Assert.Equal(Order.Third, p.Order);
			Assert.Equal(Disposition.SoSo, p.Disposition);

			Assert.NotNull(p.Friends);
			Assert.Equal(10, p.Friends.Count);

			Assert.NotNull(p.BestFriend);
			Assert.Equal("The Fonz", p.BestFriend.Name);
			Assert.Equal(1952, p.BestFriend.Since);
		}

		[Fact]
		public void Can_Deserialize_Attributes_On_Default_Root()
		{
			var doc = CreateAttributesXml();
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

			Assert.NotNull(p.BestFriend);
			Assert.Equal("The Fonz", p.BestFriend.Name);
			Assert.Equal(1952, p.BestFriend.Since);
		}

		[Fact]
		public void Ignore_Protected_Property_That_Exists_In_Data()
		{
			var doc = CreateElementsXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Null(p.IgnoreProxy);
		}

		[Fact]
		public void Ignore_ReadOnly_Property_That_Exists_In_Data()
		{
			var doc = CreateElementsXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Null(p.ReadOnlyProxy);
		}

		[Fact]
		public void Can_Deserialize_Names_With_Underscores_On_Default_Root()
		{
			var doc = CreateUnderscoresXml();
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

			Assert.NotNull(p.Foes);
			Assert.Equal(5, p.Foes.Count);
			Assert.Equal("Yankees", p.Foes.Team);
		}


		[Fact]
		public void Can_Deserialize_Names_With_Dashes_On_Default_Root()
		{
			var doc = CreateDashesXml();
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

			Assert.NotNull(p.Foes);
			Assert.Equal(5, p.Foes.Count);
			Assert.Equal("Yankees", p.Foes.Team);
		}

		[Fact]
		public void Can_Deserialize_Names_With_Underscores_Without_Matching_Case_On_Default_Root ()
		{
			var doc = CreateLowercaseUnderscoresXml ();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer ();
			var p = d.Deserialize<PersonForXml> (response);

			Assert.Equal ("John Sheehan", p.Name);
			Assert.Equal (new DateTime (2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal (28, p.Age);
			Assert.Equal (long.MaxValue, p.BigNumber);
			Assert.Equal (99.9999m, p.Percent);
			Assert.Equal (false, p.IsCool);
			Assert.Equal (new Guid (GuidString), p.UniqueId);
			Assert.Equal (new Uri ("http://example.com", UriKind.RelativeOrAbsolute), p.Url);
			Assert.Equal (new Uri ("/foo/bar", UriKind.RelativeOrAbsolute), p.UrlPath);

			Assert.NotNull (p.Friends);
			Assert.Equal (10, p.Friends.Count);

			Assert.NotNull (p.BestFriend);
			Assert.Equal ("The Fonz", p.BestFriend.Name);
			Assert.Equal (1952, p.BestFriend.Since);

			Assert.NotNull (p.Foes);
			Assert.Equal (5, p.Foes.Count);
			Assert.Equal ("Yankees", p.Foes.Team);
		}
        
        [Fact]
		public void Can_Deserialize_Lower_Cased_Root_Elements_With_Dashes()
		{
			var doc = CreateDashesXml();
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

			Assert.NotNull(p.Foes);
			Assert.Equal(5, p.Foes.Count);
			Assert.Equal("Yankees", p.Foes.Team);
		}

		[Fact]
		public void Can_Deserialize_Root_Elements_Without_Matching_Case_And_Dashes()
		{
			var doc = CreateLowerCasedRootElementWithDashesXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			var p = d.Deserialize<List<IncomingInvoice>>(response);

			Assert.NotNull(p);
			Assert.Equal(1, p.Count);
			Assert.Equal(45, p[0].ConceptId);
		}


		[Fact]
		public void Can_Deserialize_Eventful_Xml()
		{
			var xmlpath = PathFor("eventful.xml");
			var doc = XDocument.Load(xmlpath);
			var response = new RestResponse { Content = doc.ToString() };

			var d = new XmlDeserializer();
			var output = d.Deserialize<SampleClasses.VenueSearch>(response);

			Assert.NotEmpty(output.venues);
			Assert.Equal(3, output.venues.Count);
			Assert.Equal("Tivoli", output.venues[0].name);
			Assert.Equal("http://eventful.com/brisbane/venues/tivoli-/V0-001-002169294-8", output.venues[1].url);
			Assert.Equal("V0-001-000266914-3", output.venues[2].id);
		}

		[Fact]
		public void Can_Deserialize_Lastfm_Xml()
		{
			var xmlpath = PathFor("Lastfm.xml");
			var doc = XDocument.Load(xmlpath);
			var response = new RestResponse { Content = doc.ToString() };

			var d = new XmlDeserializer();
			var output = d.Deserialize<SampleClasses.Lastfm.Event>(response);

			//Assert.NotEmpty(output.artists);
			Assert.Equal("http://www.last.fm/event/328799+Philip+Glass+at+Barbican+Centre+on+12+June+2008", output.url);
			Assert.Equal("http://www.last.fm/venue/8777860+Barbican+Centre", output.venue.url);
		}

		[Fact]
		public void Can_Deserialize_Google_Weather_Xml()
		{
			var xmlpath = PathFor("GoogleWeather.xml");
			var doc = XDocument.Load(xmlpath);
			var response = new RestResponse { Content = doc.ToString() };

			var d = new XmlDeserializer();
			var output = d.Deserialize<SampleClasses.xml_api_reply>(response);

			Assert.NotEmpty(output.weather);
			Assert.Equal(4, output.weather.Count);
			Assert.Equal("Sunny", output.weather[0].condition.data);
		}

		[Fact]
		public void Can_Deserialize_Boolean_From_Number()
		{
				var xmlpath = PathFor("boolean_from_number.xml");
				var doc = XDocument.Load(xmlpath);
				var response = new RestResponse { Content = doc.ToString() };

				var d = new XmlDeserializer();
				var output = d.Deserialize<SampleClasses.BooleanTest>(response);

				Assert.True(output.Value);
		}

		[Fact]
		public void Can_Deserialize_Boolean_From_String()
		{
				var xmlpath = PathFor("boolean_from_string.xml");
				var doc = XDocument.Load(xmlpath);
				var response = new RestResponse { Content = doc.ToString() };

				var d = new XmlDeserializer();
				var output = d.Deserialize<SampleClasses.BooleanTest>(response);

				Assert.True(output.Value);
		}

		[Fact]
		public void Can_Deserialize_Empty_Elements_With_Attributes_to_Nullable_Values()
		{
			var doc = CreateXmlWithAttributesAndNullValues();

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<NullableValues>(new RestResponse {Content = doc});

			Assert.Null(output.Id);
			Assert.Null(output.StartDate);
			Assert.Null(output.UniqueId);
		}

		[Fact]
		public void Can_Deserialize_Mixture_Of_Empty_Elements_With_Attributes_And_Populated_Elements()
		{
			var doc = CreateXmlWithAttributesAndNullValuesAndPopulatedValues();

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<NullableValues>(new RestResponse {Content = doc});

			Assert.Null(output.Id);
			Assert.Null(output.StartDate);
			Assert.Equal(new Guid(GuidString), output.UniqueId);
		}

		private static string CreateUnderscoresXml()
		{
			var doc = new XDocument();
			var root = new XElement("Person");
			root.Add(new XElement("Name", "John Sheehan"));
			root.Add(new XElement("Start_Date", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XAttribute("Age", 28));
			root.Add(new XElement("Percent", 99.9999m));
			root.Add(new XElement("Big_Number", long.MaxValue));
			root.Add(new XAttribute("Is_Cool", false));
			root.Add(new XElement("Ignore", "dummy"));
			root.Add(new XAttribute("Read_Only", "dummy"));
			root.Add(new XElement("Unique_Id", new Guid(GuidString)));
			root.Add(new XElement("Url", "http://example.com"));
			root.Add(new XElement("Url_Path", "/foo/bar"));

			root.Add(new XElement("Best_Friend",
								  new XElement("Name", "The Fonz"),
								  new XAttribute("Since", 1952)
						));

			var friends = new XElement("Friends");
			for (int i = 0; i < 10; i++)
			{
				friends.Add(new XElement("Friend",
										 new XElement("Name", "Friend" + i),
										 new XAttribute("Since", DateTime.Now.Year - i)
								));
			}
			root.Add(friends);

			var foes = new XElement("Foes");
			foes.Add(new XAttribute("Team", "Yankees"));
			for (int i = 0; i < 5; i++)
			{
				foes.Add(new XElement("Foe", new XElement("Nickname", "Foe" + i)));
			}
			root.Add(foes);

			doc.Add(root);
			return doc.ToString();
		}

		private static string CreateLowercaseUnderscoresXml()
		{
			var doc = new XDocument();
			var root = new XElement("Person");
			root.Add(new XElement("Name", "John Sheehan"));
			root.Add(new XElement("start_date", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XAttribute("Age", 28));
			root.Add(new XElement("Percent", 99.9999m));
			root.Add(new XElement("big_number", long.MaxValue));
			root.Add(new XAttribute("is_cool", false));
			root.Add(new XElement("Ignore", "dummy"));
			root.Add(new XAttribute("read_only", "dummy"));
			root.Add(new XElement("unique_id", new Guid(GuidString)));
			root.Add(new XElement("Url", "http://example.com"));
			root.Add(new XElement("url_path", "/foo/bar"));

			root.Add(new XElement("best_friend",
								  new XElement("name", "The Fonz"),
								  new XAttribute("Since", 1952)
						));

			var friends = new XElement("Friends");
			for (int i = 0; i < 10; i++)
			{
				friends.Add(new XElement("Friend",
										 new XElement("Name", "Friend" + i),
										 new XAttribute("Since", DateTime.Now.Year - i)
								));
			}
			root.Add(friends);

			var foes = new XElement("Foes");
			foes.Add(new XAttribute("Team", "Yankees"));
			for (int i = 0; i < 5; i++)
			{
				foes.Add(new XElement("Foe", new XElement("Nickname", "Foe" + i)));
			}
			root.Add(foes);

			doc.Add(root);		
			
			return doc.ToString();			
		}

		private static string CreateDashesXml()
		{
			var doc = new XDocument();
			var root = new XElement("Person");
			root.Add(new XElement("Name", "John Sheehan"));
			root.Add(new XElement("Start_Date", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XAttribute("Age", 28));
			root.Add(new XElement("Percent", 99.9999m));
			root.Add(new XElement("Big-Number", long.MaxValue));
			root.Add(new XAttribute("Is-Cool", false));
			root.Add(new XElement("Ignore", "dummy"));
			root.Add(new XAttribute("Read-Only", "dummy"));
			root.Add(new XElement("Unique-Id", new Guid(GuidString)));
			root.Add(new XElement("Url", "http://example.com"));
			root.Add(new XElement("Url-Path", "/foo/bar"));

			root.Add(new XElement("Best-Friend",
						new XElement("Name", "The Fonz"),
						new XAttribute("Since", 1952)
					));

			var friends = new XElement("Friends");
			for (int i = 0; i < 10; i++)
			{
				friends.Add(new XElement("Friend",
								new XElement("Name", "Friend" + i),
								new XAttribute("Since", DateTime.Now.Year - i)
							));
			}
			root.Add(friends);

			var foes = new XElement("Foes");
			foes.Add(new XAttribute("Team", "Yankees"));
			for (int i = 0; i < 5; i++)
			{
				foes.Add(new XElement("Foe", new XElement("Nickname", "Foe" + i)));
			}
			root.Add(foes);

			doc.Add(root);
			return doc.ToString();
		}
        
        private static string CreateLowerCasedRootElementWithDashesXml()
		{
            var doc = new XDocument();
			var root = new XElement("incoming-invoices",
				new XElement("incoming-invoice",
					new XElement("concept-id", 45)
				)
			);
			doc.Add(root);
			return doc.ToString();
		}

		private static string CreateElementsXml()
		{
			var doc = new XDocument();
			var root = new XElement("Person");
			root.Add(new XElement("Name", "John Sheehan"));
			root.Add(new XElement("StartDate", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XElement("Age", 28));
			root.Add(new XElement("Percent", 99.9999m));
			root.Add(new XElement("BigNumber", long.MaxValue));
			root.Add(new XElement("IsCool", false));
			root.Add(new XElement("Ignore", "dummy"));
			root.Add(new XElement("ReadOnly", "dummy"));

			root.Add(new XElement("UniqueId", new Guid(GuidString)));
			root.Add(new XElement("EmptyGuid", ""));

			root.Add(new XElement("Url", "http://example.com"));
			root.Add(new XElement("UrlPath", "/foo/bar"));
			root.Add(new XElement("Order", "third"));
			root.Add(new XElement("Disposition", "so-so"));

			root.Add(new XElement("BestFriend",
						new XElement("Name", "The Fonz"),
						new XElement("Since", 1952)
					));

			var friends = new XElement("Friends");
			for (int i = 0; i < 10; i++)
			{
				friends.Add(new XElement("Friend",
								new XElement("Name", "Friend" + i),
								new XElement("Since", DateTime.Now.Year - i)
							));
			}
			root.Add(friends);

			doc.Add(root);
			return doc.ToString();
		}

		private static string CreateAttributesXml()
		{
			var doc = new XDocument();
			var root = new XElement("Person");
			root.Add(new XAttribute("Name", "John Sheehan"));
			root.Add(new XAttribute("StartDate", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XAttribute("Age", 28));
			root.Add(new XAttribute("Percent", 99.9999m));
			root.Add(new XAttribute("BigNumber", long.MaxValue));
			root.Add(new XAttribute("IsCool", false));
			root.Add(new XAttribute("Ignore", "dummy"));
			root.Add(new XAttribute("ReadOnly", "dummy"));
			root.Add(new XAttribute("UniqueId", new Guid(GuidString)));
			root.Add(new XAttribute("Url", "http://example.com"));
			root.Add(new XAttribute("UrlPath", "/foo/bar"));

			root.Add(new XElement("BestFriend",
						new XAttribute("Name", "The Fonz"),
						new XAttribute("Since", 1952)
					));

			doc.Add(root);
			return doc.ToString();
		}

		private static string CreateXmlWithNullValues()
		{
			var doc = new XDocument();
			var root = new XElement("NullableValues");

			root.Add(new XElement("Id", null),
					 new XElement("StartDate", null),
					 new XElement("UniqueId", null)
				);

			doc.Add(root);

			return doc.ToString();
		}

		private static string CreateXmlWithoutEmptyValues(CultureInfo culture)
		{
			var doc = new XDocument();
			var root = new XElement("NullableValues");

			root.Add(new XElement("Id", 123),
				new XElement("StartDate", new DateTime(2010, 2, 21, 9, 35, 00).ToString(culture)),
				new XElement("UniqueId", new Guid(GuidString))
			);

			doc.Add(root);

			return doc.ToString();
		}

		private static string CreateXmlWithEmptyNestedList()
		{
			var doc = new XDocument();
			var root = new XElement("EmptyListSample");

			root.Add(new XElement("Images"));

			doc.Add(root);

			return doc.ToString();
		}

		private static string CreateXmlWithEmptyInlineList()
		{
			var doc = new XDocument();
			var root = new XElement("EmptyListSample");

			doc.Add(root);

			return doc.ToString();
		}

		private static string CreateXmlWithAttributesAndNullValues()
		{
			var doc = new XDocument();
			var root = new XElement("NullableValues");

			var idElement = new XElement("Id", null);
			idElement.SetAttributeValue("SomeAttribute", "SomeAttribute_Value");
			root.Add(idElement,
				new XElement("StartDate", null),
				new XElement("UniqueId", null)
			);

			doc.Add(root);

			return doc.ToString();
		}

		private static string CreateXmlWithAttributesAndNullValuesAndPopulatedValues()
		{
			var doc = new XDocument();
			var root = new XElement("NullableValues");

			var idElement = new XElement("Id", null);
			idElement.SetAttributeValue("SomeAttribute", "SomeAttribute_Value");
			root.Add(idElement,
				new XElement("StartDate", null),
				new XElement("UniqueId", new Guid(GuidString))
			);

			doc.Add(root);

			return doc.ToString();
		}
	}
}
