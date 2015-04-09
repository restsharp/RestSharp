using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Tests.SampleClasses
{
    public class xml_api_reply
    {
        public string version { get; set; }
        public Weather weather { get; set; }
    }

    public class Weather : List<forecast_conditions>
    {
        public string module_id { get; set; }
        public string tab_id { get; set; }
        public string mobile_row { get; set; }
        public string mobile_zipped { get; set; }
        public string row { get; set; }
        public string section { get; set; }
        public Forecast_information forecast_information { get; set; }
        public Current_conditions current_conditions { get; set; }
        //public T forecast_conditions { get; set; }
    }

    public class DataElement
    {
        public string data { get; set; }
    }

    public class Forecast_information
    {
        public DataElement city { get; set; }
        public DataElement postal_code { get; set; }
        public DataElement forecast_date { get; set; }
        public DataElement unit_system { get; set; }
    }

    public class Current_conditions
    {
        public DataElement condition { get; set; }
        public DataElement temp_c { get; set; }
        public DataElement humidity { get; set; }
        public DataElement icon { get; set; }
        public DataElement wind_condition { get; set; }
    }

    public class forecast_conditions
    {
        public DataElement day_of_week { get; set; }
        public DataElement condition { get; set; }
        public DataElement low { get; set; }
        public DataElement high { get; set; }
        public DataElement icon { get; set; }
    }
}
