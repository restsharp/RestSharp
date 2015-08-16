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
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using NUnit.Framework;
using RestSharp.Serializers;
using RestSharp.Tests.SampleClasses;

namespace RestSharp.Tests
{
    [TestFixture]
    public class SerializerTests
    {
        public SerializerTests()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
        }

        [Test]
        public void Serializes_Properties_In_Specified_Order()
        {
            OrderedProperties ordered = new OrderedProperties
                                        {
                                            Name = "Name",
                                            Age = 99,
                                            StartDate = new DateTime(2010, 1, 1)
                                        };
            XmlSerializer xml = new XmlSerializer();
            string doc = xml.Serialize(ordered);
            XDocument expected = GetSortedPropsXDoc();

            Assert.AreEqual(expected.ToString(), doc);
        }

        [Test]
        public void Can_serialize_simple_POCO()
        {
            Person poco = new Person
                          {
                              Name = "Foo",
                              Age = 50,
                              Price = 19.95m,
                              StartDate = new DateTime(2009, 12, 18, 10, 2, 23),
                              Items = new List<Item>
                                      {
                                          new Item { Name = "One", Value = 1 },
                                          new Item { Name = "Two", Value = 2 },
                                          new Item { Name = "Three", Value = 3 }
                                      }
                          };
            XmlSerializer xml = new XmlSerializer();
            string doc = xml.Serialize(poco);
            XDocument expected = GetSimplePocoXDoc();

            Assert.AreEqual(expected.ToString(), doc);
        }

        [Test]
        public void Can_serialize_Enum()
        {
            ClassWithEnum enumClass = new ClassWithEnum { Color = Color.Red };
            XmlSerializer xml = new XmlSerializer();
            string doc = xml.Serialize(enumClass);
            XDocument expected = new XDocument();
            XElement root = new XElement("ClassWithEnum");

            root.Add(new XElement("Color", "Red"));
            expected.Add(root);

            Assert.AreEqual(expected.ToString(), doc);
        }

        [Test]
        public void Can_serialize_simple_POCO_With_DateFormat_Specified()
        {
            Person poco = new Person
                          {
                              Name = "Foo",
                              Age = 50,
                              Price = 19.95m,
                              StartDate = new DateTime(2009, 12, 18, 10, 2, 23)
                          };
            XmlSerializer xml = new XmlSerializer { DateFormat = DateFormat.ISO_8601 };
            string doc = xml.Serialize(poco);
            XDocument expected = GetSimplePocoXDocWithIsoDate();

            Assert.AreEqual(expected.ToString(), doc);
        }

        [Test]
        public void Can_serialize_simple_POCO_With_XmlFormat_Specified()
        {
            Person poco = new Person
                          {
                              Name = "Foo",
                              Age = 50,
                              Price = 19.95m,
                              StartDate = new DateTime(2009, 12, 18, 10, 2, 23),
                              IsCool = false
                          };
            XmlSerializer xml = new XmlSerializer { DateFormat = DateFormat.ISO_8601 };
            string doc = xml.Serialize(poco);
            XDocument expected = GetSimplePocoXDocWithXmlProperty();

            Assert.AreEqual(expected.ToString(), doc);
        }

        [Test]
        public void Can_serialize_simple_POCO_With_Different_Root_Element()
        {
            Person poco = new Person
                          {
                              Name = "Foo",
                              Age = 50,
                              Price = 19.95m,
                              StartDate = new DateTime(2009, 12, 18, 10, 2, 23)
                          };
            XmlSerializer xml = new XmlSerializer { RootElement = "Result" };
            string doc = xml.Serialize(poco);
            XDocument expected = GetSimplePocoXDocWithRoot();

            Assert.AreEqual(expected.ToString(), doc);
        }

        [Test]
        public void Can_serialize_simple_POCO_With_Attribute_Options_Defined()
        {
            WackyPerson poco = new WackyPerson
                               {
                                   Name = "Foo",
                                   Age = 50,
                                   Price = 19.95m,
                                   StartDate = new DateTime(2009, 12, 18, 10, 2, 23)
                               };
            XmlSerializer xml = new XmlSerializer();
            string doc = xml.Serialize(poco);
            XDocument expected = GetSimplePocoXDocWackyNames();

            Assert.AreEqual(expected.ToString(), doc);
        }

        [Test]
        public void Can_serialize_simple_POCO_With_Attribute_Options_Defined_And_Property_Containing_IList_Elements()
        {
            WackyPerson poco = new WackyPerson
                               {
                                   Name = "Foo",
                                   Age = 50,
                                   Price = 19.95m,
                                   StartDate = new DateTime(2009, 12, 18, 10, 2, 23),
                                   ContactData = new ContactData
                                                 {
                                                     EmailAddresses = new List<EmailAddress>
                                                                      {
                                                                          new EmailAddress
                                                                          {
                                                                              Address = "test@test.com",
                                                                              Location = "Work"
                                                                          }
                                                                      }
                                                 }
                               };
            XmlSerializer xml = new XmlSerializer();
            string doc = xml.Serialize(poco);
            XDocument expected = GetSimplePocoXDocWackyNamesWithIListProperty();

            Assert.AreEqual(expected.ToString(), doc);
        }

        [Test]
        public void Can_serialize_a_list_which_is_the_root_element()
        {
            PersonList pocoList = new PersonList
                                  {
                                      new Person
                                      {
                                          Name = "Foo",
                                          Age = 50,
                                          Price = 19.95m,
                                          StartDate = new DateTime(2009, 12, 18, 10, 2, 23),
                                          Items = new List<Item>
                                                  {
                                                      new Item { Name = "One", Value = 1 },
                                                      new Item { Name = "Two", Value = 2 },
                                                      new Item { Name = "Three", Value = 3 }
                                                  }
                                      },
                                      new Person
                                      {
                                          Name = "Bar",
                                          Age = 23,
                                          Price = 23.23m,
                                          StartDate = new DateTime(2009, 12, 23, 10, 23, 23),
                                          Items = new List<Item>
                                                  {
                                                      new Item { Name = "One", Value = 1 },
                                                      new Item { Name = "Two", Value = 2 },
                                                      new Item { Name = "Three", Value = 3 }
                                                  }
                                      }
                                  };
            XmlSerializer xml = new XmlSerializer();
            string doc = xml.Serialize(pocoList);
            XDocument expected = GetPeopleXDoc(CultureInfo.InvariantCulture);

            Assert.AreEqual(expected.ToString(), doc);
        }

        private class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public decimal Price { get; set; }

            public DateTime StartDate { get; set; }

            public List<Item> Items { get; set; }

            public bool? IsCool { get; set; }
        }

        private class Item
        {
            public string Name { get; set; }

            public int Value { get; set; }
        }

        private enum Color
        {
            Red,

            Blue,

            Green
        }

        private class ClassWithEnum
        {
            public Color Color { get; set; }
        }

        [SerializeAs(Name = "Person")]
        private class WackyPerson
        {
            [SerializeAs(Name = "WackyName", Attribute = true)]
            public string Name { get; set; }

            public int Age { get; set; }

            [SerializeAs(Attribute = true)]
            public decimal Price { get; set; }

            [SerializeAs(Name = "start_date", Attribute = true)]
            public DateTime StartDate { get; set; }

            [SerializeAs(Name = "contact-data")]
            public ContactData ContactData { get; set; }
        }

        [SerializeAs(Name = "People")]
        private class PersonList : List<Person> { }

        private class ContactData
        {
            public ContactData()
            {
                this.EmailAddresses = new List<EmailAddress>();
            }

            [SerializeAs(Name = "email-addresses")]
            public List<EmailAddress> EmailAddresses { get; set; }
        }

        [SerializeAs(Name = "email-address")]
        private class EmailAddress
        {
            [SerializeAs(Name = "address")]
            public string Address { get; set; }

            [SerializeAs(Name = "location")]
            public string Location { get; set; }
        }

        private static XDocument GetSimplePocoXDoc()
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("Person");

            root.Add(new XElement("Name", "Foo"),
                new XElement("Age", 50),
                new XElement("Price", 19.95m),
                new XElement("StartDate", new DateTime(2009, 12, 18, 10, 2, 23).ToString(CultureInfo.InvariantCulture)));

            XElement items = new XElement("Items");

            items.Add(new XElement("Item", new XElement("Name", "One"), new XElement("Value", 1)));
            items.Add(new XElement("Item", new XElement("Name", "Two"), new XElement("Value", 2)));
            items.Add(new XElement("Item", new XElement("Name", "Three"), new XElement("Value", 3)));

            root.Add(items);
            doc.Add(root);

            return doc;
        }

        private static XDocument GetSimplePocoXDocWithIsoDate()
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("Person");

            root.Add(new XElement("Name", "Foo"),
                new XElement("Age", 50),
                new XElement("Price", 19.95m),
                new XElement("StartDate", new DateTime(2009, 12, 18, 10, 2, 23).ToString("s")));

            doc.Add(root);

            return doc;
        }

        private static XDocument GetSimplePocoXDocWithXmlProperty()
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("Person");

            root.Add(new XElement("Name", "Foo"),
                new XElement("Age", 50),
                new XElement("Price", 19.95m),
                new XElement("StartDate", new DateTime(2009, 12, 18, 10, 2, 23).ToString("s")),
                new XElement("IsCool", false));

            doc.Add(root);

            return doc;
        }

        private static XDocument GetSimplePocoXDocWithRoot()
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("Result");
            XElement start = new XElement("Person");

            start.Add(new XElement("Name", "Foo"),
                new XElement("Age", 50),
                new XElement("Price", 19.95m),
                new XElement("StartDate", new DateTime(2009, 12, 18, 10, 2, 23).ToString(CultureInfo.InvariantCulture)));

            root.Add(start);
            doc.Add(root);

            return doc;
        }

        private static XDocument GetSimplePocoXDocWackyNames()
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("Person");

            root.Add(new XAttribute("WackyName", "Foo"),
                new XElement("Age", 50),
                new XAttribute("Price", 19.95m),
                new XAttribute("start_date", new DateTime(2009, 12, 18, 10, 2, 23).ToString(CultureInfo.InvariantCulture)));

            doc.Add(root);

            return doc;
        }

        private static XDocument GetSimplePocoXDocWackyNamesWithIListProperty()
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("Person");

            root.Add(new XAttribute("WackyName", "Foo"),
                new XElement("Age", 50),
                new XAttribute("Price", 19.95m),
                new XAttribute("start_date",
                    new DateTime(2009, 12, 18, 10, 2, 23).ToString(CultureInfo.InvariantCulture)),
                new XElement("contact-data",
                    new XElement("email-addresses",
                        new XElement("email-address",
                            new XElement("address", "test@test.com"),
                            new XElement("location", "Work")))));

            doc.Add(root);

            return doc;
        }

        private static XDocument GetSortedPropsXDoc()
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("OrderedProperties");

            root.Add(new XElement("StartDate", new DateTime(2010, 1, 1).ToString(CultureInfo.InvariantCulture)));
            root.Add(new XElement("Name", "Name"));
            root.Add(new XElement("Age", 99));

            doc.Add(root);

            return doc;
        }

        private static XDocument GetPeopleXDoc(IFormatProvider culture)
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("People");
            XElement element = new XElement("Person");
            XElement items = new XElement("Items");

            items.Add(new XElement("Item", new XElement("Name", "One"), new XElement("Value", 1)));
            items.Add(new XElement("Item", new XElement("Name", "Two"), new XElement("Value", 2)));
            items.Add(new XElement("Item", new XElement("Name", "Three"), new XElement("Value", 3)));

            element.Add(new XElement("Name", "Foo"),
                new XElement("Age", 50),
                new XElement("Price", 19.95m.ToString(culture)),
                new XElement("StartDate", new DateTime(2009, 12, 18, 10, 2, 23).ToString(culture)));

            element.Add(items);
            root.Add(element);
            element = new XElement("Person");

            element.Add(new XElement("Name", "Bar"),
                new XElement("Age", 23),
                new XElement("Price", 23.23m.ToString(culture)),
                new XElement("StartDate", new DateTime(2009, 12, 23, 10, 23, 23).ToString(culture)));

            element.Add(items);

            root.Add(element);
            doc.Add(root);

            return doc;
        }
    }
}
