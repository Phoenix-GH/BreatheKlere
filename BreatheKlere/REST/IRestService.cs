using System;
using System.Threading.Tasks;
using Xamarin.Forms.GoogleMaps;
namespace BreatheKlere.REST
{
    public interface IRestService
    {
        Task<GeoResult> getGeoResult(string locationName);
        Task<Direction> getDirection(string origin, string destination);
    }
}
