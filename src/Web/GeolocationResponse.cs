using Bottlecap.Net.Bots.Data;
using System.Collections.Generic;

namespace Bottlecap.Net.Bots.Web
{
    public class GeolocationResponse
    {
        public IEnumerable<Address> results { get; set; }
    }
}