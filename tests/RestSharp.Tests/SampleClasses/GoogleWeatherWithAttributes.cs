using System.Collections.Generic;
using RestSharp.Deserializers;

namespace RestSharp.Tests.SampleClasses
{
    public class GoogleWeatherApi
    {
        public string Version { get; set; }

        public GoogleWeather Weather { get; set; }
    }

    public class GoogleWeather : List<ForecastConditions>
    {
        public string ModuleId { get; set; }

        public string TabId { get; set; }

        public string MobileRow { get; set; }

        public string MobileZipped { get; set; }

        public string Row { get; set; }

        public string Section { get; set; }

        [DeserializeAs(Name = "forecast_information")]
        public ForecastInformation Forecast { get; set; }

        [DeserializeAs(Name = "current_conditions")]
        public CurrentConditions Current { get; set; }
    }

    public class GoogleDataElement
    {
        public string Data { get; set; }
    }

    public class ForecastInformation
    {
        public GoogleDataElement City { get; set; }

        public GoogleDataElement PostalCode { get; set; }

        public GoogleDataElement ForecastDate { get; set; }

        public GoogleDataElement UnitSystem { get; set; }
    }

    public class CurrentConditions
    {
        public GoogleDataElement Condition { get; set; }

        public GoogleDataElement TempC { get; set; }

        public GoogleDataElement Humidity { get; set; }

        public GoogleDataElement Icon { get; set; }

        public GoogleDataElement WindCondition { get; set; }
    }

    public class ForecastConditions
    {
        public GoogleDataElement DayOfWeek { get; set; }

        public GoogleDataElement Condition { get; set; }

        public GoogleDataElement Low { get; set; }

        public GoogleDataElement High { get; set; }

        public GoogleDataElement Icon { get; set; }
    }
}
