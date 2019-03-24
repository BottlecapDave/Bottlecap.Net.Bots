using Bottlecap.Net.Bots.Alexa.Data;
using Bottlecap.Net.Bots.Web;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace Bottlecap.Net.Bots.Alexa.Web
{
    public class AddressService : GeolocationService
    {
        public const string COUNTRY_AND_POSTALCODE_URL_FORMAT = "v1/devices/{0}/settings/address/countryAndPostalCode";
        public const string FULL_ADDRESS_URL_FORMAT = "v1/devices/{0}/settings/address";

        public async Task<Address> GetAddressAsync(string consentToken, string baseUri, string deviceId, bool onlyRequestPostalCode)
        {
            var client = new RestClient(baseUri);

            var request = new RestRequest(String.Format(onlyRequestPostalCode ? COUNTRY_AND_POSTALCODE_URL_FORMAT : FULL_ADDRESS_URL_FORMAT,
                                                        deviceId));

            request.AddHeader("Authorization", $"Bearer {consentToken}");
            request.AddHeader("Content-Type", "application/json");

            var response = await client.GetAsync<Address>(request);
            return response;
        }
    }
}
