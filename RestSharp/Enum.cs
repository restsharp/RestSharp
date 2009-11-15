using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp
{
	public enum ResponseFormat
	{
		Auto,
		Json,
		Xml,
		None
	}

	public enum RequestFormat
	{
		Parameters,
		Xml,
		Json
	}

	public enum Method
	{
		GET,
		POST,
		PUT,
		DELETE,
		HEAD,
		OPTIONS
	}

	public enum UrlMode
	{
		AsIs,
		ReplaceValues
	}

	public enum DateFormat
	{

	}
}
