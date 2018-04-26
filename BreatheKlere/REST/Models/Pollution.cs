using System.Collections.Generic;

namespace BreatheKlere.REST
{
    public class Pollution
    {
        public List<string> lat { get; set; }
        public List<string> lon { get; set; }
        public List<string> num { get; set; }
        public List<string> val { get; set; }
        public List<string> dist { get; set; }
    }

    public class PollutionRequest
    {
        public float RAD { get; set; }
        public List<List<string>> PAIRS { get; set; }
    }
}
