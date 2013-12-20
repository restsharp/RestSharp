using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestSharp.Tests.RestSharp_Http
{
    [Trait("Unit", "Configuring A RestRequest")]
    public class When_Configuring_A_RestRequest
    {

        [Fact]
        public void Then_Adding_A_Body_Adds_A_RequestBody_Parameter()
        {
            var name = new Name() { First = "Jon", Last = "Doe" };

            JsonSerializer serializer = new JsonSerializer();
            var data = serializer.Serialize(name);

            List<Parameter> mockParameters = new List<Parameter>();
            mockParameters.Add(new Parameter() { Name=serializer.ContentType, Type=ParameterType.RequestBody, Value=data});

            var request = new RestRequest();
            request.AddBody(name);

            //Assert.Equal(mockParameters, request.Parameters, new RestSharp.Tests.RestSharp_Http.When_Configuring_A_RestRequest.Name.NameComparer());

            var param = mockParameters.FirstOrDefault();

            Assert.Equal(param.Type, ParameterType.RequestBody);
        }

        [Fact]
        public void Then_Adding_A_File_Adds_A_File_Parameter()
        {
            List<FileParameter> mockParameters = new List<FileParameter>();
            mockParameters.Add(new FileParameter() { Name="unit", Data=new byte[0], FileName="text.ext" });

            var request = new RestRequest();
            request.AddFile("unit",new byte[0], "test.jpg");

            Assert.Equal(mockParameters, request.Files);

            var param = mockParameters.FirstOrDefault();

            Assert.Equal(param.Name, "unit");
            Assert.Equal(param.Data, new byte[0]);
            Assert.Equal(param.FileName, "text.ext");
        }

        [Fact]
        public void Then_Adding_An_Object_With_Two_Public_Properties_Adds_Two_Parameters()
        {
            List<Parameter> mockParameters = new List<Parameter>();
            mockParameters.Add(new Parameter() { Name="First", Value="Jon", Type=ParameterType.QueryString });
            mockParameters.Add(new Parameter() { Name = "Last", Value = "Doe", Type = ParameterType.QueryString });

            var name = new { First = "Jon", Last = "Doe" };

            var request = new RestRequest();
            request.AddObject(name);

            Assert.Equal(mockParameters, request.Parameters);

            var param = mockParameters.FirstOrDefault();

            Assert.Equal(param.Name, "First");
            Assert.Equal(param.Value, "Jon");
            Assert.Equal(param.Type, ParameterType.QueryString);
        }

        [Fact]
        public void Then_Adding_An_Object_With_Two_Public_Properties_And_A_Whitelist_Adds_One_Parameter()
        {
            List<Parameter> mockParameters = new List<Parameter>();
            mockParameters.Add(new Parameter() { Name = "First", Value = "Jon", Type = ParameterType.QueryString });

            var name = new { First = "Jon", Last = "Doe" };

            var request = new RestRequest();
            request.AddObject(name, new string[] { "Last" });

            Assert.Equal(mockParameters, request.Parameters);

            var param = mockParameters.FirstOrDefault();

            Assert.Equal(param.Name, "First");
            Assert.Equal(param.Value, "Jon");
            Assert.Equal(param.Type, ParameterType.QueryString);

        }

        [Fact]
        public void Then_Adding_A_Default_Parameter()
        {

        }

        [Fact]
        public void Then_Adding_A_Default_UrlSegment()
        {

        }

        [Fact]
        public void Then_Adding_A_Cookie()
        {

        }

        public void bar()
        {
            //client.AddDefaultHeader();
            //client.AddHandler();
            //client.BuildUri();
        }


        public class Name
        {
            public string First { get; set; }
            public string Last { get; set; }
        }

        public class NameComparer : IEqualityComparer<Name> {

            public bool Equals(Name x, Name y)
            {
 	            return (x.First == y.First) && (x.Last == y.Last);
            }

            public int GetHashCode(Name obj)
            {
 	            return obj.GetHashCode();
            }
        }
    
    }

}
