#region Licensed
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
using System.Collections.Generic;
using RestSharp.Serializers;

namespace RestSharp.Tests
{
	public class PersonForXml
	{
		public string Name { get; set; }
		public DateTime StartDate { get; set; }
		public int Age { get; set; }
		public decimal Percent { get; set; }
		public long BigNumber { get; set; }
		public bool IsCool { get; set; }
		public List<Friend> Friends { get; set; }
		public Friend BestFriend { get; set; }

		protected string Ignore { get; set; }
		public string IgnoreProxy { get { return Ignore; } }

		protected string ReadOnly { get { return null; } }
		public string ReadOnlyProxy { get { return ReadOnly; } }

		public FoeList Foes { get; set; }
	}

	public class PersonForJson
	{
		public string Name { get; set; }
		public DateTime StartDate { get; set; }
		public int Age { get; set; }
		public decimal Percent { get; set; }
		public long BigNumber { get; set; }
		public bool IsCool { get; set; }
		public List<Friend> Friends { get; set; }
		public Friend BestFriend { get; set; }
    public Guid Guid { get; set; }

		protected string Ignore { get; set; }
		public string IgnoreProxy { get { return Ignore; } }

		protected string ReadOnly { get { return null; } }
		public string ReadOnlyProxy { get { return ReadOnly; } }

		public Dictionary<string, Foe> Foes { get; set; }
	}

	public class Friend
	{
		public string Name { get; set; }
		public int Since { get; set; }
	}

	public class Foe
	{
		public string Nickname { get; set; }
	}

	public class FoeList : List<Foe>
	{
		public string Team { get; set; }
	}

	public class Birthdate
	{
		public DateTime Value { get; set; }
	}

	public class OrderedProperties
	{
		[SerializeAs(Index=2)]
		public string Name { get; set; }
		[SerializeAs(Index = 3)]
		public int Age { get; set; }
		[SerializeAs(Index = 1)]
		public DateTime StartDate { get; set; }
	}
}
