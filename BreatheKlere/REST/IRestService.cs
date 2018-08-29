using System.Threading.Tasks;
namespace BreatheKlere.REST
{
    public interface IRestService
    {
        Task<GeoResult> GetGeoResult(string locationName);
        Task<Direction> GetDirection(string origin, string destination, string mode);
        Task<DistanceMatrix> GetDistance(string origin, string destination, string mode);
        Task<Place> GetPlaces(string locationName, string location);
        Task<MQDirection> GetMQDirection(string from, string to, string mode);
        Task<MQAlternativeDirection> GetMQAlternativeDirection(string from, string to, string mode);

        Task<Login> Login(string UN, string PW, string DID);
        Task<Login> Register(string N, string UN, string PW, string PC, string G, string DID);
        Task<Base> SaveRoute(string DID);
    }
}
