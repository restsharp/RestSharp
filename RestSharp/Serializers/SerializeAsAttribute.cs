#region License
//   Copyright 2009 John Sheehan
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
using RestSharp.Extensions;

namespace RestSharp.Serializers
{
	[global::System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class SerializeAsAttribute : Attribute
	{
		public string Name { get; set; }
		public bool Attribute { get; set; }
	}

	[global::System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class SerializeTransformAttribute : Attribute
	{
		public SerializeTransformAttribute(Transform transform) {
			Transform = transform;
		}

		public Transform Transform { get; private set; }

		public string Shazam(string input) {
			switch (Transform) {
				case Transform.CamelCase:
					return input.ToCamelCase();
				case Transform.PascalCase:
					return input.ToPascalCase();
				case Transform.LowerCase:
					return input.ToLower();
			}

			return input;
		}
	}

	public enum Transform
	{
		CamelCase,
		LowerCase,
		PascalCase
	}
}
