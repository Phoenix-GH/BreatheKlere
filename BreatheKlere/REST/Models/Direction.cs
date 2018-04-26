using System;
using System.Collections.Generic;

namespace BreatheKlere.REST
{
    public class Direction
    {
        public List<GeocodedWaypoint> geocoded_waypoints { get; set; }
        public List<Route> routes { get; set; }
        public string status { get; set; }
    }

    public class GeocodedWaypoint 
    {
        public string geocoder_status { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }
    }

    public class Route
    {
        public Object bounds { get; set; }
        public string copyrights { get; set; }
        public List<Leg> legs { get; set; }
        public Polyline overview_polyline { get; set; }
        public string summary { get; set; }
        public List<Object> warnings { get; set; }
        public List<Object> waypoint_order { get; set; }
    }

    public class BasicStep
    {
        public DistanceDuration distance { get; set; }
        public DistanceDuration duration { get; set; }
        public Location end_location { get; set; }
        public Location start_location { get; set; }

    }

    public class Leg : BasicStep {
        public string end_address { get; set; }
        public string start_address { get; set; }
        public List<Object> traffic_speed_entry { get; set; }
        public List<Object> via_endpoint { get; set; }
        public List<Step> steps { get; set; }
    }

    public class Step : BasicStep
    {
        public string html_instructions { get; set; }
        public string maneuver { get; set; }
        public Polyline polyline { get; set; }
        public string travel_mode { get; set; }
    }

    public class DistanceDuration{
        public string text { get; set; }
        public double value { get; set; }
    }

    public class Polyline{
        public string points { get; set; }
    }
}
