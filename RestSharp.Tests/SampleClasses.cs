using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Tests
{
	public class Person
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
}
