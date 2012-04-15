using System.Xml.Serialization;
using System.Collections.Generic;
using System;

namespace MTB.Worker.AcoustID
{
    public class Response
    {
        public string status { get; set; }
        public List<Result> results { get; set; }
    }

    public class Result
    {
        public List<Recording> recordings { get; set; }
        public decimal score { get; set; }
        public string id { get; set; }
    }

    public class Recording
    {
        public List<Artist> artists { get; set; }
        public int duration { get; set; }
        public List<Releasegroup> releasegroups { get; set; }
        public string title { get; set; }
        public string id { get; set; }
    }
    public class Artist
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Releasegroup
    {
        public List<Artist> artists { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public List<Release> releases { get; set; }
        public string title { get; set; }
    }

    public class Release
    {
        public int track_count { get; set; }
        public string title { get; set; }
        public string country { get; set; }
        public List<Artist> artists { get; set; }
        public Date date { get; set; }
        public int medium_count { get; set; }
        public string id { get; set; }
    }

    public class Date
    {
        public int month { get; set; }
        public int day { get; set; }
        public int year { get; set; }
        public DateTime date
        {
            get
            {
                return new DateTime(year, month, day);
            }
        }

    }
}
