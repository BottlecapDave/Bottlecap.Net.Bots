using Bottlecap.Net.Bots.Data;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Bottlecap.Net.Bots.Web
{
    public class GeolocationService
    {
        public async Task<IEnumerable<Address>> GetGeoLocationsAsync(string apiKey, string location)
        {
            var client = new RestClient("https://maps.googleapis.com");

            var request = new RestRequest($"maps/api/geocode/json?address={WebUtility.UrlEncode(location)}&key={apiKey}");
            request.AddHeader("Content-Type", "application/json");

            var response = await client.GetAsync<GeolocationResponse>(request);
            return response?.results;
        }

        private class GeolocationResponse
        {
            public IEnumerable<Address> results { get; set; }
        }
    }
}
