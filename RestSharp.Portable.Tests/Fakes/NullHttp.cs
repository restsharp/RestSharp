using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Tests.Fakes
{
	public class NullHttp : Http
	{
		public new HttpResponse Get()
		{
			return new HttpResponse();
		}
	}
}
