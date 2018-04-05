using System.Threading.Tasks;
namespace BreatheKlere.REST
{
    public interface IRestService
    {
        Task<GeoResult> GetGeoResult(string locationName);
        Task<Direction> GetDirection(string origin, string destination);
        Task<DistanceMatrix> GetDistance(string origin, string destination, string mode);
    }
}
