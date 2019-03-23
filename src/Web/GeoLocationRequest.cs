using System;
using System.Net;

namespace Bottlecap.Net.Bots.Web
{
    public class GeolocationRequest
    {
        public const string URL_FORMAT = "https://maps.googleapis.com/maps/api/geocode/json?address={0}&key={1}";

        public Uri TargetUri { get; private set; }

        public GeolocationRequest(string apiKey, string location)
        {
            TargetUri = new Uri(String.Format(URL_FORMAT,
                                              WebUtility.UrlEncode(location),
                                              apiKey));
        }
    }
}
