using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Tests.SampleClasses
{
	public class VenueSearch
	{
		public string total_items { get; set; }
		public string page_size { get; set; }
		public string page_count { get; set; }
		public string page_number { get; set; }
		public string page_items { get; set; }
		public string first_item { get; set; }
		public string last_item { get; set; }
		public string search_time { get; set; }
		public List<Venue> venues { get; set; }
	}

	public class PerformerSearch
	{
		public string total_items { get; set; }
		public string page_size { get; set; }
		public string page_count { get; set; }
		public string page_number { get; set; }
		public string page_items { get; set; }
		public string first_item { get; set; }
		public string last_item { get; set; }
		public string search_time { get; set; }
		public List<Performer> performers { get; set; }
	}

	public class Performer
	{
		public string id { get; set; }
		public string url { get; set; }
		public string name { get; set; }
		public string short_bio { get; set; }
		public DateTime? created { get; set; }
		public string creator { get; set; }
		public string demand_count { get; set; }
		public string demand_member_count { get; set; }
		public string event_count { get; set; }
		public ServiceImage image { get; set; }
	}

	public class Venue
	{
		public string id { get; set; }
		public string url { get; set; }
		public string name { get; set; }
		public string venue_name { get; set; }
		public string description { get; set; }
		public string venue_type { get; set; }
		public string address { get; set; }
		public string city_name { get; set; }
		public string region_name { get; set; }
		public string postal_code { get; set; }
		public string country_name { get; set; }
		public string longitude { get; set; }
		public string latitude { get; set; }
		public string event_count { get; set; }
	}

	public class ServiceImage
	{
		public ServiceImage1 thumb { get; set; }
		public ServiceImage1 small { get; set; }
		public ServiceImage1 medium { get; set; }
	}

	public class ServiceImage1
	{
		public string url { get; set; }
		public string width { get; set; }
		public string height { get; set; }
	}

	public class Event
	{
		public string id { get; set; }
		public string url { get; set; }
		public string title { get; set; }
		public string description { get; set; }
		public string start_time { get; set; }
		public string venue_name { get; set; }
		public string venue_id { get; set; }
		public List<Performer> performers { get; set; }
	}
}