using System.Collections.Generic;
using RestSharp.Deserializers;

namespace RestSharp.Tests.SampleClasses
{
    public class status
    {
        public bool truncated { get; set; }

        public string created_at { get; set; }

        public string source { get; set; }

        public bool favorited { get; set; }

        public string in_reply_to_user_id { get; set; }

        public string in_reply_to_status_id { get; set; }

        public string in_reply_to_screen_name { get; set; }

        // ignore contributors for now
        public user user { get; set; }

        // ignore geo
        public long id { get; set; }

        public string text { get; set; }
    }

    public class user
    {
        public string url { get; set; }

        public string description { get; set; }

        public string profile_text_color { get; set; }

        public int followers_count { get; set; }

        public int statuses_count { get; set; }

        public bool geo_enabled { get; set; }

        public string profile_background_image_url { get; set; }

        public bool notifications { get; set; }

        public string created_at { get; set; }

        public int friends_count { get; set; }

        public string profile_link_color { get; set; }

        public bool contributors_enabled { get; set; }

        public bool profile_background_tile { get; set; }

        public int favourites_count { get; set; }

        public string profile_background_color { get; set; }

        public string profile_image_url { get; set; }

        public string lang { get; set; }

        public bool verified { get; set; }

        public string profile_sidebar_fill_color { get; set; }

        public bool @protected { get; set; }

        public string screen_name { get; set; }

        public bool following { get; set; }

        public string location { get; set; }

        public string name { get; set; }

        public string time_zone { get; set; }

        public string profile_sidebar_border_color { get; set; }

        public long id { get; set; }

        public int utc_offset { get; set; }
    }

    public class StatusList : List<status> { }

    public class complexStatus
    {
        public bool truncated { get; set; }

        public string created_at { get; set; }

        public string source { get; set; }

        public bool favorited { get; set; }

        public string in_reply_to_user_id { get; set; }

        public string in_reply_to_status_id { get; set; }

        public string in_reply_to_screen_name { get; set; }

        // ignore contributors for now
        [DeserializeAs(Name = "user.following")]
        public bool follow { get; set; }

        // ignore geo
        public long id { get; set; }

        public string text { get; set; }
    }

    public class StatusComplexList : List<complexStatus> { }
}
