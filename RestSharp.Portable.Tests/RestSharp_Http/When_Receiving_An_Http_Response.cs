using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestSharp.Tests
{
    [Trait("Unit", "When Receiving and Http Response")]
    public class When_Receiving_An_Http_Response
    {
        [Fact]
        public void Foo()
        {
            var responseMessage = new HttpResponseMessage();
            //responseMessage.

            var httpResponse = new HttpResponse();
            httpResponse.ConvertFromResponseMessage(responseMessage);

            //assert stuff here
            //server
            //statuscode
            //reasonphrase
            //Uri
            //responsestatus

            //contentencoding
            //contentype
            //contentlength

            //cookies

            //headers
        }
    }
}
