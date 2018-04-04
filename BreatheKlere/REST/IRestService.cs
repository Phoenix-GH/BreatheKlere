using System.Threading.Tasks;
namespace BreatheKlere.REST
{
    public interface IRestService
    {
        Task<GeoResult> getGeoResult(string locationName);
        Task<Direction> getDirection(string origin, string destination);
        Task<DistanceMatrix> getDistance(string origin, string destination);
    }
}
