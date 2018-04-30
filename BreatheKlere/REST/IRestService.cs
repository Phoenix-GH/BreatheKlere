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
    }
}
