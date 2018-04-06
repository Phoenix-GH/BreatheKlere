using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BreatheKlere.REST
{
    public class RESTService : IRestService
    {
        string baseURL = "https://maps.googleapis.com/maps/api/";
        HttpClient client;
        public RESTService()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(baseURL);
            client.MaxResponseContentBufferSize = 2560000;
        }

        public async Task<Direction> GetDirection(string origin, string destination, string mode)
        {
            string url = baseURL + "directions/json?key=" + Config.google_maps_ios_api_key + "&origin=" + origin + "&destination=" + destination + "&mode=" + mode;
            var uri = new Uri(url);
            try
            {
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                var resultResponse = JsonConvert.DeserializeObject<Direction>(content);
                return resultResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"             GetDirection ERROR {0}", ex.Message);
            }
            return null;
        }

        public async Task<DistanceMatrix> GetDistance(string origin, string destination, string mode="driving")
        {
            string url = baseURL + "distancematrix/json?key=" + Config.google_maps_ios_api_key + "&origins=" + origin + "&destinations=" + destination + "&mode=" + mode;
            var uri = new Uri(url);
            try
            {
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                var resultResponse = JsonConvert.DeserializeObject<DistanceMatrix>(content);
                return resultResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"             GetDistance ERROR {0}", ex.Message);
            }
            return null;
        }

        public async Task<GeoResult> GetGeoResult(string locationName)
        {
            string url = baseURL + "geocode/json?key=" + Config.google_maps_ios_api_key + "&address=" + locationName;
            var uri = new Uri(url);
            try
            {
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                var resultResponse = JsonConvert.DeserializeObject<GeoResult>(content);
                return resultResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"             GeoResult ERROR {0}", ex.Message);
            }
            return null;
        }

    }
}
