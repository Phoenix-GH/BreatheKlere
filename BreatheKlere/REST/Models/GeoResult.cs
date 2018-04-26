using System;
using System.Collections.Generic;

namespace BreatheKlere.REST
{
    public class GeoResult
    {
        public List<ResultItem> results { get; set; }
        public string status { get; set; }
    }

    public class ResultItem 
    {
        public List<AddressComponent> address_components { get; set; }
        public string formatted_address { get; set; }
        public Geometry geometry { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }

    }

    public class AddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }
    }

    public class Geometry
    {
        public Object bounds { get; set; }
        public Location location { get; set; }
        public string location_type { get; set; }
        public Object viewport { get; set; }
    }

    public class Location {
        public double lat { get; set; }
        public double lng { get; set; }
    }
}
