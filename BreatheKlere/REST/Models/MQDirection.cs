using System;
using System.Collections.Generic;

namespace BreatheKlere.REST
{
    public class MQDirection
    {
        public MPRoute route { get; set; }
        public Object info { get; set; }
    }

    public class MQAlternativeDirection
    {
        public AlternativeRoute route { get; set; }
        public Object info { get; set; }
    }

    public class AlternativeRoute:MPRoute
    {
        public List<MQDirection> alternateRoutes { get; set; }
    }

    public class MPRoute
    {
        public bool hasTollRoad { get; set; }
        public bool hasBridge { get; set; }
        public BoundingBox boundingBox { get; set; }
        public float distance { get; set; }
        public MQShape shape;
        public bool hasTunnel { get; set; }
        public bool hasHighway { get; set; }
        public List<Object> computedWaypoints;
        public Object routeError { get; set; }
        public string formattedTime { get; set; }
        public string sessionId { get; set; }
        public Object bestFit { get; set; }
        public float realTime { get; set; }
        public bool hasSeasonalClosure { get; set; }
        public bool hasCountryCross { get; set; }
        public Object mapState { get; set; }

        public float fuelUsed { get; set; }
        public List<MQLeg> legs { get; set; }
        public Object options { get; set; }
        public List<MQLocation> locations { get; set; }
        public bool hasUnpaved { get; set; }

        public float time { get; set; }
        public List<int> locationSequence { get; set; }


        public bool hasFerry { get; set; }
    }

    public class BoundingBox
    {
        Location ul { get; set; }
        Location lr { get; set; }
    }

    public class MQLocation
    {
        public Location latLng { get; set; }
        public string adminArea4 { get; set; }
        public string adminArea5Type { get; set; }
        public string adminArea4Type { get; set; }
        public string adminArea5 { get; set; }
        public string street { get; set; }
        public string adminArea1 { get; set; }
        public string adminArea3 { get; set; }
        public string type { get; set; }
        public Location displaylatLng { get; set; }
        public string linkId { get; set; }
        public string postalCode { get; set; }
        public string sideOfStreet { get; set; }
        public bool dragPoint { get; set; }
        public string adminArea1Type { get; set; }
        public string geocodeQuality { get; set; }
        public string geocodeQualityCode { get; set; }
        public string adminArea3Type { get; set; }
    }

    public class MQLeg
    {
        public bool hasTollRoad { get; set; }
        public bool hasBridge { get; set; }
        public int index { get; set; }
        public List<Object> roadGradeStrategy { get; set; }
        public bool hasHighWay { get; set; }
        public bool hasUnpaved { get; set; }
        public float distance { get; set; }
        public float time { get; set; }
        public int origIndex { get; set; }
        public bool hasSeasonalClosure { get; set; }
        public string origNarrative { get; set; }
        public bool hasCountryCross { get; set; }
        public string formattedTime { get; set; }
        public string destNarrative { get; set; }
        public int destIndex { get; set; }
        public List<MQManeuver> maneuvers { get; set; }
        public bool hasFerry { get; set; }
    }

    public class MQManeuver
    {
        public List<Object> signs { get; set; }
        public int index { get; set; }
        public List<Object> maneuverNotes { get; set; }
        public int direction { get; set; }
        public string narrative { get; set; }
        public string iconUrl { get; set; }
        public float distance { get; set; }
        public float time { get; set; }
        public List<Object> linkIds { get; set; }
        public List<string> streets { get; set; }
        public int attributes { get; set; }
        public string transportMode { get; set; }
        public string formattedTime { get; set; }
        public string directionName { get; set; }
        public string mapUrl { get; set; }
        public Location startPoint { get; set; }
        public int turnType { get; set; }
    }
    public class MQShape
    {
        public List<int> legIndexes { get; set; }
        public List<int> maneuverIndexes { get; set; }
        public string shapePoints { get; set; }
    }
}
