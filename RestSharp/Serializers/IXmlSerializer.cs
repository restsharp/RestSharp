using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Serializers
{
    public interface IXmlSerializer : ISerializer
    {
        string RootElement { get; set; }

        string Namespace { get; set; }

        string DateFormat { get; set; }
    }
}
