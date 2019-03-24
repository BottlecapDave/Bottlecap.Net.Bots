using System.Collections.Generic;

namespace Bottlecap.Net.Bots.Resources
{
    public class Resource
    {
        public string Key { get; set; }

        public IEnumerable<string> Values { get; set; }
    }
}
