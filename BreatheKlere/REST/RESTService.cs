using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BreatheKlere.REST
{
    public class RESTService : IRestService
    {
        string baseURL = "https://maps.googleapis.com/maps/api/geocode/json";
        HttpClient client;
        public RESTService()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(baseURL);
            client.MaxResponseContentBufferSize = 2560000;
        }

        public async Task<Result> getGeoResult(string locationName)
        {
            string url = baseURL + "?key=" + Config.google_maps_ios_api_key + "&address=" + locationName;
            Debug.WriteLine("url-------------", url);
            var uri = new Uri(url);
            try
            {
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                var resultResponse = JsonConvert.DeserializeObject<Result>(content);
                return resultResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"             ResetPassword ERROR {0}", ex.Message);
            }
            return null;
        }
    }
}
