using Bottlecap.Net.Bots.Alexa.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bottlecap.Net.Bots.Alexa.Web
{
    public class GetAddressRequest : Json.Web.JsonWebRequestItem<Address>
    {
        public const string COUNTRY_AND_POSTALCODE_URL_FORMAT = "{0}/v1/devices/{1}/settings/address/countryAndPostalCode";
        public const string FULL_ADDRESS_URL_FORMAT = "{0}/v1/devices/{1}/settings/address";

        private readonly string _token;

        public GetAddressRequest(string token, string baseUri, string deviceId, bool onlyRequestPostalCode)
        {
            TargetUri = new Uri(String.Format(onlyRequestPostalCode ? COUNTRY_AND_POSTALCODE_URL_FORMAT : FULL_ADDRESS_URL_FORMAT,
                                              baseUri,
                                              deviceId));

            _token = token;
        }

        public override async Task<IEnumerable<Pair<string, string>>> ConstructHeaders()
        {
            var headers = new List<Pair<string, string>>(await base.ConstructHeaders());

            headers.Add(new Pair<string, string>("Authorization", String.Format("Bearer {0}", _token)));

            return headers;
        }
    }
}
