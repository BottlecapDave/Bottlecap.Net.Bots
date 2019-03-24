using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bottlecap.Net.Bots.Resources
{
    public class ResourceManager : IResourceManager
    {
        private Dictionary<string, List<string>> _Resources = new Dictionary<string, List<string>>();

        public void Initialise(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var collection = JsonConvert.DeserializeObject<ResourcesCollection>(reader.ReadToEnd());
                if (collection != null)
                {
                    foreach (var resource in collection.Resources)
                    {
                        _Resources.Add(resource.Key, new List<string>(resource.Values));
                    }
                }
            }
        }

        public string GetResource(string key)
        {
            List<string> values;
            if (_Resources.TryGetValue(key, out values))
            {
                var random = new Random();
                return values[random.Next(values.Count)];
            }

            return null;
        }
    }
}
