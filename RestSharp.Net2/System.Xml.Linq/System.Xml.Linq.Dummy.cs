
using System.Collections.Generic;

namespace System.Xml.Linq
{
    public class XName
    {
        public static implicit operator XName(string s)
        {
            throw new NotImplementedException();
        }


        public XNamespace Namespace
        {
            get { throw new NotImplementedException(); }
        }

        public string LocalName
        {
            get { throw new NotImplementedException(); }
        }

        public string NamespaceName
        {
            get { throw new NotImplementedException(); }
        }

        public static XName Get(string name, string ns)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(XName x, string s)
        {
            throw new NotImplementedException();
        }
        public static bool operator !=(XName x, string s)
        {
            throw new NotImplementedException();
        }


    }
    public class XNamespace
    {
        public static XNamespace None = new XNamespace();

        public XName GetName(string localName)
        {
            throw new NotImplementedException();
        }
    }

    public class XDocument
    {
        public static XDocument Parse(string content)
        {
            throw new NotImplementedException();
        }

        public XElement Root
        {
            get { throw new NotImplementedException(); }
        }

        public void Add(XElement wrapper)
        {
            throw new NotImplementedException();
        }
    }
    public class XElement
    {
        public XElement(XName asNamespaced)
        {
            throw new NotImplementedException();
        }

        public XElement(XName asNamespaced, XElement root)
        {
            throw new NotImplementedException();
        }

        public XName Name
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string Value
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool HasElements
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsEmpty
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasAttributes
        {
            get { throw new NotImplementedException(); }
        }

        public XElement Element(XName asNamespaced)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<XElement> DescendantsAndSelf()
        {
            throw new NotImplementedException();
        }

        public void Add(XAttribute instance)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<XAttribute> Attributes()
        {
            throw new NotImplementedException();
        }

        public void ReplaceAttributes(IEnumerable<XAttribute> @select)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<XElement> Elements(XName name)
        {
            throw new NotImplementedException();
        }

        public void Add(XElement instance)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<XElement> Elements()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<XElement> Descendants(XName asNamespaced)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<XElement> Descendants()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<XElement> Ancestors()
        {
            throw new NotImplementedException();
        }

        public XAttribute Attribute(XName name)
        {
            throw new NotImplementedException();
        }
    }
    public class XAttribute
    {
        public XAttribute(XName getName, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsNamespaceDeclaration
        {
            get { throw new NotImplementedException(); }
        }

        public XName Name
        {
            get { throw new NotImplementedException(); }
        }

        public object Value
        {
            get { throw new NotImplementedException(); }
        }
    }



}
