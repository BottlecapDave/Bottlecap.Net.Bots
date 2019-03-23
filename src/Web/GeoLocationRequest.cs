using Bottlecap.Json.Web;
using System;
using System.Net;
using System.IO;

namespace Bottlecap.Components.Bots.Web
{
    public class GeolocationRequest : JsonWebRequestItem<GeolocationResponse>
    {
        public const string URL_FORMAT = "https://maps.googleapis.com/maps/api/geocode/json?address={0}&key={1}";

        public GeolocationRequest(string apiKey, string location)
        {
            TargetUri = new Uri(String.Format(URL_FORMAT,
                                              WebUtility.UrlEncode(location),
                                              apiKey));
        }
    }
}
