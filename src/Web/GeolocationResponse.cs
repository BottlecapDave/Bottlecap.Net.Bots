using Bottlecap.Components.Bots.Data;
using System.Collections.Generic;

namespace Bottlecap.Components.Bots.Web
{
    public class GeolocationResponse
    {
        public IEnumerable<Address> results { get; set; }
    }
}