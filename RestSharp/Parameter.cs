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

namespace RestSharp
{
	/// <summary>
	/// Parameter container for REST requests
	/// </summary>
	public class Parameter
	{
		/// <summary>
		/// Name of the parameter
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Value of the parameter
		/// </summary>
		public object Value { get; set; }
		/// <summary>
		/// Type of the parameter
		/// </summary>
		public ParameterType Type { get; set; }

		/// <summary>
		/// Return a human-readable representation of this parameter
		/// </summary>
		/// <returns>String</returns>
		public override string ToString() {
			return string.Format("{0}={1}", Name, Value);
		}
	}
}
