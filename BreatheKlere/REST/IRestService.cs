using System;
using System.Threading.Tasks;
using Xamarin.Forms.GoogleMaps;
namespace BreatheKlere.REST
{
    public interface IRestService
    {
        Task<Result> getGeoResult(string locationName);
    }
}
