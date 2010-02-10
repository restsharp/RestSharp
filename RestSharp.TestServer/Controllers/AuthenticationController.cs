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
using System.Text;
using System.Web.Mvc;

namespace RestSharp.TestServer.Controllers
{
	public class AuthenticationController : Controller
	{
		public string Basic() {
			var header = Request.Headers["Authorization"];
			if (string.IsNullOrEmpty(header)) {
				return "no authorization provided";
			}

			var parts = Encoding.ASCII.GetString(Convert.FromBase64String(header.Substring("Basic ".Length))).Split(':');
			return string.Join("|", parts);
		}
	}
}
