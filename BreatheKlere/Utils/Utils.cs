using System;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Xamarin.Forms.GoogleMaps;

namespace BreatheKlere
{
    public static class Utils
    {
        public static bool IsLocationAvailable()
        {
            if (!CrossGeolocator.IsSupported)
                return false;

            return CrossGeolocator.Current.IsGeolocationAvailable;
        }
        public async static Task<Position> GetPosition() 
        {
            var locator = CrossGeolocator.Current;
            var pos = await locator.GetPositionAsync(TimeSpan.FromTicks(10000));
            Position position = new Position(pos.Latitude, pos.Longitude);
            return position;
        }
    }
}
