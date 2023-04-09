using System.Globalization;
using System.Xml.Linq;
using RestSharp.Serializers;
using RestSharp.Serializers.Xml;
using RestSharp.Tests.Serializers.Xml.SampleClasses;

namespace RestSharp.Tests.Serializers.Xml;

public class XmlSerializerTests {
    public XmlSerializerTests() {
        Thread.CurrentThread.CurrentCulture   = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
    }

    [Fact]
    public void Can_serialize_a_list_of_items_with_interface_type() {
        var items = new NamedItems {
            Items = new List<INamed> {
                new Person {
                    Name      = "Foo",
                    Age       = 50,
                    Price     = 19.95m,
                    StartDate = new DateTime(2009, 12, 18, 10, 2, 23),
                    Items = new List<Item> { new() { Name = "One", Value = 1 } }
                },
                new Item { Name = "Two", Value   = 2 },
                new Item { Name = "Three", Value = 3 }
            }
        };

        var xml      = new XmlSerializer();
        var doc      = xml.Serialize(items);
        var expected = GetNamedItemsXDoc(CultureInfo.InvariantCulture);

        Assert.Equal(expected.ToString(), doc);
    }

    [Fact]
    public void Can_serialize_a_list_which_is_the_content_of_root_element() {
        var contacts = new Contacts {
            People = new List<Person> {
                new() {
                    Name      = "Foo",
                    Age       = 50,
                    Price     = 19.95m,
                    StartDate = new DateTime(2009, 12, 18, 10, 2, 23),
                    Items = new List<Item> {
                        new() { Name = "One", Value   = 1 },
                        new() { Name = "Two", Value   = 2 },
                        new() { Name = "Three", Value = 3 }
                    }
                },
                new() {
                    Name      = "Bar",
                    Age       = 23,
                    Price     = 23.23m,
                    StartDate = new DateTime(2009, 12, 23, 10, 23, 23),
                    Items = new List<Item> {
                        new() { Name = "One", Value   = 1 },
                        new() { Name = "Two", Value   = 2 },
                        new() { Name = "Three", Value = 3 }
                    }
                }
            }
        };

        var xml      = new XmlSerializer();
        var doc      = xml.Serialize(contacts);
        var expected = GetPeopleXDoc(CultureInfo.InvariantCulture);

        Assert.Equal(expected.ToString(), doc);
    }

    [Fact]
    public void Can_serialize_a_list_which_is_the_root_element() {
        var pocoList = new PersonList {
            new() {
                Name      = "Foo",
                Age       = 50,
                Price     = 19.95m,
                StartDate = new DateTime(2009, 12, 18, 10, 2, 23),
                Items = new List<Item> {
                    new() { Name = "One", Value   = 1 },
                    new() { Name = "Two", Value   = 2 },
                    new() { Name = "Three", Value = 3 }
                }
            },
            new() {
                Name      = "Bar",
                Age       = 23,
                Price     = 23.23m,
                StartDate = new DateTime(2009, 12, 23, 10, 23, 23),
                Items = new List<Item> {
                    new() { Name = "One", Value   = 1 },
                    new() { Name = "Two", Value   = 2 },
                    new() { Name = "Three", Value = 3 }
                }
            }
        };
        var xml      = new XmlSerializer();
        var doc      = xml.Serialize(pocoList);
        var expected = GetPeopleXDoc(CultureInfo.InvariantCulture);

        Assert.Equal(expected.ToString(), doc);
    }

    [Fact]
    public void Can_Serialize_An_Object_To_Node_With_Attribute_And_Text_Content() {
        var note = new Note {
            Id      = 1,
            Title   = Note.TITLE,
            Message = Note.MESSAGE
        };

        var xml = new XmlSerializer();
        var doc = xml.Serialize(note);

        var expected    = GetNoteXDoc();
        var expectedStr = expected.ToString();

        Assert.Equal(expectedStr, doc);
    }

    [Fact]
    public void Can_serialize_Enum() {
        var enumClass = new ClassWithEnum { Color = Color.Red };
        var xml       = new XmlSerializer();
        var doc       = xml.Serialize(enumClass);
        var expected  = new XDocument();
        var root      = new XElement("ClassWithEnum");

        root.Add(new XElement("Color", "Red"));
        expected.Add(root);

        Assert.Equal(expected.ToString(), doc);
    }

    [Fact]
    public void Can_serialize_simple_POCO() {
        var poco = new Person {
            Name      = "Foo",
            Age       = 50,
            Price     = 19.95m,
            StartDate = new DateTime(2009, 12, 18, 10, 2, 23),
            Items = new List<Item> {
                new() { Name = "One", Value   = 1 },
                new() { Name = "Two", Value   = 2 },
                new() { Name = "Three", Value = 3 }
            }
        };
        var xml      = new XmlSerializer();
        var doc      = xml.Serialize(poco);
        var expected = GetSimplePocoXDoc();

        Assert.Equal(expected.ToString(), doc);
    }

    [Fact]
    public void Can_serialize_simple_POCO_With_Attribute_Options_Defined() {
        var poco = new WackyPerson {
            Name      = "Foo",
            Age       = 50,
            Price     = 19.95m,
            StartDate = new DateTime(2009, 12, 18, 10, 2, 23)
        };
        var xml      = new XmlSerializer();
        var doc      = xml.Serialize(poco);
        var expected = GetSimplePocoXDocWackyNames();

        Assert.Equal(expected.ToString(), doc);
    }

    [Fact]
    public void Can_serialize_simple_POCO_With_Attribute_Options_Defined_And_Property_Containing_IList_Elements() {
        var poco = new WackyPerson {
            Name      = "Foo",
            Age       = 50,
            Price     = 19.95m,
            StartDate = new DateTime(2009, 12, 18, 10, 2, 23),
            ContactData = new ContactData {
                EmailAddresses = new List<EmailAddress> {
                    new() {
                        Address  = "test@test.com",
                        Location = "Work"
                    }
                }
            }
        };
        var xml      = new XmlSerializer();
        var doc      = xml.Serialize(poco);
        var expected = GetSimplePocoXDocWackyNamesWithIListProperty();

        Assert.Equal(expected.ToString(), doc);
    }

    [Fact]
    public void Can_serialize_simple_POCO_With_DateFormat_Specified() {
        var poco = new Person {
            Name      = "Foo",
            Age       = 50,
            Price     = 19.95m,
            StartDate = new DateTime(2009, 12, 18, 10, 2, 23)
        };
        var xml      = new XmlSerializer { DateFormat = DateFormat.ISO_8601 };
        var doc      = xml.Serialize(poco);
        var expected = GetSimplePocoXDocWithIsoDate();

        Assert.Equal(expected.ToString(), doc);
    }

    [Fact]
    public void Can_serialize_simple_POCO_With_Different_Root_Element() {
        var poco = new Person {
            Name      = "Foo",
            Age       = 50,
            Price     = 19.95m,
            StartDate = new DateTime(2009, 12, 18, 10, 2, 23)
        };
        var xml      = new XmlSerializer { RootElement = "Result" };
        var doc      = xml.Serialize(poco);
        var expected = GetSimplePocoXDocWithRoot();

        Assert.Equal(expected.ToString(), doc);
    }

    [Fact]
    public void Can_serialize_simple_POCO_With_XmlFormat_Specified() {
        var poco = new Person {
            Name      = "Foo",
            Age       = 50,
            Price     = 19.95m,
            StartDate = new DateTime(2009, 12, 18, 10, 2, 23),
            IsCool    = false
        };
        var xml      = new XmlSerializer { DateFormat = DateFormat.ISO_8601 };
        var doc      = xml.Serialize(poco);
        var expected = GetSimplePocoXDocWithXmlProperty();

        Assert.Equal(expected.ToString(), doc);
    }

    [Fact]
    public void Cannot_Serialize_An_Object_With_Two_Properties_With_Text_Content_Attributes() {
        var note = new WrongNote {
            Id   = 1,
            Text = "What a note."
        };

        var xml = new XmlSerializer();

        Assert.Throws<ArgumentException>(() => xml.Serialize(note));
    }

    [Fact]
    public void Serializes_Properties_In_Specified_Order() {
        var ordered = new OrderedProperties {
            Name      = "Name",
            Age       = 99,
            StartDate = new DateTime(2010, 1, 1)
        };
        var xml      = new XmlSerializer();
        var doc      = xml.Serialize(ordered);
        var expected = GetSortedPropsXDoc();

        Assert.Equal(expected.ToString(), doc);
    }

    interface INamed {
        string Name { get; set; }
    }

    class Person : INamed {
        public string Name { get; set; }

        public int Age { get; set; }

        public decimal Price { get; set; }

        public DateTime StartDate { get; set; }

        public List<Item> Items { get; set; }

        public bool? IsCool { get; set; }
    }

    class Item : INamed {
        public string Name { get; set; }

        public int Value { get; set; }
    }

    enum Color { Red, Blue, Green }

    class ClassWithEnum {
        public Color Color { get; set; }
    }

    [SerializeAs(Name = "Person")]
    class WackyPerson {
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

    class NamedItems {
        [SerializeAs(Content = true)]
        public List<INamed> Items { get; set; }
    }

    [SerializeAs(Name = "People")]
    class Contacts {
        [SerializeAs(Content = true)]
        public List<Person> People { get; set; }
    }

    [SerializeAs(Name = "People")]
    class PersonList : List<Person> { }

    class ContactData {
        public ContactData() => EmailAddresses = new List<EmailAddress>();

        [SerializeAs(Name = "email-addresses")]
        public List<EmailAddress> EmailAddresses { get; set; }
    }

    [SerializeAs(Name = "email-address")]
    class EmailAddress {
        [SerializeAs(Name = "address")]
        public string Address { get; set; }

        [SerializeAs(Name = "location")]
        public string Location { get; set; }
    }

    static XDocument GetNoteXDoc() {
        var doc  = new XDocument();
        var root = new XElement("Note");

        root.SetAttributeValue("Id", 1);
        root.Value = Note.MESSAGE;
        root.Add(new XElement("Title", Note.TITLE));

        doc.Add(root);

        return doc;
    }

    static XDocument GetSimplePocoXDoc() {
        var doc  = new XDocument();
        var root = new XElement("Person");

        root.Add(
            new XElement("Name", "Foo"),
            new XElement("Age", 50),
            new XElement("Price", 19.95m),
            new XElement(
                "StartDate",
                new DateTime(2009, 12, 18, 10, 2, 23).ToString(CultureInfo.InvariantCulture)
            )
        );

        var items = new XElement("Items");

        items.Add(new XElement("Item", new XElement("Name", "One"), new XElement("Value", 1)));
        items.Add(new XElement("Item", new XElement("Name", "Two"), new XElement("Value", 2)));
        items.Add(new XElement("Item", new XElement("Name", "Three"), new XElement("Value", 3)));

        root.Add(items);
        doc.Add(root);

        return doc;
    }

    static XDocument GetSimplePocoXDocWithIsoDate() {
        var doc  = new XDocument();
        var root = new XElement("Person");

        root.Add(
            new XElement("Name", "Foo"),
            new XElement("Age", 50),
            new XElement("Price", 19.95m),
            new XElement("StartDate", new DateTime(2009, 12, 18, 10, 2, 23).ToString("s"))
        );

        doc.Add(root);

        return doc;
    }

    static XDocument GetSimplePocoXDocWithXmlProperty() {
        var doc  = new XDocument();
        var root = new XElement("Person");

        root.Add(
            new XElement("Name", "Foo"),
            new XElement("Age", 50),
            new XElement("Price", 19.95m),
            new XElement("StartDate", new DateTime(2009, 12, 18, 10, 2, 23).ToString("s")),
            new XElement("IsCool", false)
        );

        doc.Add(root);

        return doc;
    }

    static XDocument GetSimplePocoXDocWithRoot() {
        var doc   = new XDocument();
        var root  = new XElement("Result");
        var start = new XElement("Person");

        start.Add(
            new XElement("Name", "Foo"),
            new XElement("Age", 50),
            new XElement("Price", 19.95m),
            new XElement(
                "StartDate",
                new DateTime(2009, 12, 18, 10, 2, 23).ToString(CultureInfo.InvariantCulture)
            )
        );

        root.Add(start);
        doc.Add(root);

        return doc;
    }

    static XDocument GetSimplePocoXDocWackyNames() {
        var doc  = new XDocument();
        var root = new XElement("Person");

        root.Add(
            new XAttribute("WackyName", "Foo"),
            new XElement("Age", 50),
            new XAttribute("Price", 19.95m),
            new XAttribute(
                "start_date",
                new DateTime(2009, 12, 18, 10, 2, 23).ToString(CultureInfo.InvariantCulture)
            )
        );

        doc.Add(root);

        return doc;
    }

    static XDocument GetSimplePocoXDocWackyNamesWithIListProperty() {
        var doc  = new XDocument();
        var root = new XElement("Person");

        root.Add(
            new XAttribute("WackyName", "Foo"),
            new XElement("Age", 50),
            new XAttribute("Price", 19.95m),
            new XAttribute(
                "start_date",
                new DateTime(2009, 12, 18, 10, 2, 23).ToString(CultureInfo.InvariantCulture)
            ),
            new XElement(
                "contact-data",
                new XElement(
                    "email-addresses",
                    new XElement(
                        "email-address",
                        new XElement("address", "test@test.com"),
                        new XElement("location", "Work")
                    )
                )
            )
        );

        doc.Add(root);

        return doc;
    }

    static XDocument GetSortedPropsXDoc() {
        var doc  = new XDocument();
        var root = new XElement("OrderedProperties");

        root.Add(new XElement("StartDate", new DateTime(2010, 1, 1).ToString(CultureInfo.InvariantCulture)));
        root.Add(new XElement("Name", "Name"));
        root.Add(new XElement("Age", 99));

        doc.Add(root);

        return doc;
    }

    static XDocument GetNamedItemsXDoc(IFormatProvider culture) {
        var doc     = new XDocument();
        var root    = new XElement("NamedItems");
        var element = new XElement("Person");
        var items   = new XElement("Items");

        items.Add(new XElement("Item", new XElement("Name", "One"), new XElement("Value", 1)));

        element.Add(
            new XElement("Name", "Foo"),
            new XElement("Age", 50),
            new XElement("Price", 19.95m.ToString(culture)),
            new XElement("StartDate", new DateTime(2009, 12, 18, 10, 2, 23).ToString(culture))
        );

        element.Add(items);
        root.Add(element);
        root.Add(new XElement("Item", new XElement("Name", "Two"), new XElement("Value", 2)));
        root.Add(new XElement("Item", new XElement("Name", "Three"), new XElement("Value", 3)));

        doc.Add(root);

        return doc;
    }

    static XDocument GetPeopleXDoc(IFormatProvider culture) {
        var doc     = new XDocument();
        var root    = new XElement("People");
        var element = new XElement("Person");
        var items   = new XElement("Items");

        items.Add(new XElement("Item", new XElement("Name", "One"), new XElement("Value", 1)));
        items.Add(new XElement("Item", new XElement("Name", "Two"), new XElement("Value", 2)));
        items.Add(new XElement("Item", new XElement("Name", "Three"), new XElement("Value", 3)));

        element.Add(
            new XElement("Name", "Foo"),
            new XElement("Age", 50),
            new XElement("Price", 19.95m.ToString(culture)),
            new XElement("StartDate", new DateTime(2009, 12, 18, 10, 2, 23).ToString(culture))
        );

        element.Add(items);
        root.Add(element);
        element = new XElement("Person");

        element.Add(
            new XElement("Name", "Bar"),
            new XElement("Age", 23),
            new XElement("Price", 23.23m.ToString(culture)),
            new XElement("StartDate", new DateTime(2009, 12, 23, 10, 23, 23).ToString(culture))
        );

        element.Add(items);

        root.Add(element);
        doc.Add(root);

        return doc;
    }
}