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

using System.Net;
using Xunit;

namespace RestSharp.WebTests
{
	public class StatusCodeTests
	{
		[Fact]
		public void Can_Handle_404() {
			var client = new RestClient("http://localhost:56976");

			var request = new RestRequest("StatusCode/404");

			var response = client.Execute(request);

			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public void Can_Handle_Nonexisting_Url_EndPoint() {
			var client = new RestClient("http://nonexistantdomainimguessing.org");

			var request = new RestRequest("foo");

			var response = client.Execute(request);

			Assert.Equal(ResponseStatus.Error, response.ResponseStatus);
		}
	}
}