using System.Collections.Generic;

namespace BreatheKlere.REST
{
    public class DistanceMatrix
    {
        public List<string> destination_addresses { get; set; }
        public List<string> origin_addresses { get; set; }
        public List<Row> rows { get; set; }
    }

    public class Element
    {
        public DistanceDuration distance { get; set; }
        public DistanceDuration duration { get; set; }
        public string status { get; set; }
    }

    public class Row 
    {
        public List<Element> elements { get; set; }
    }
}
