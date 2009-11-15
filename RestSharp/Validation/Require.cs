using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Validation
{
	public class Require
	{
		public static void Argument(string argumentName, object argumentValue) {
			if (argumentValue == null) {
				throw new ArgumentException("Argument cannot be null.", argumentName);
			}
		}
	}
}
