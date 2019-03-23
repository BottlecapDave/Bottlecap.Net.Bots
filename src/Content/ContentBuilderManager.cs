using System;
using System.Collections.Generic;

namespace Bottlecap.Components.Bots.Content
{
    public static class ContentBuilderManager
    {
        private static Dictionary<string, IContentBuilder> _RegisteredFactories = new Dictionary<string, IContentBuilder>();

        public static void Register(string key, IContentBuilder builder)
        {
            if (_RegisteredFactories.ContainsKey(key))
            {
                throw new ArgumentException(String.Format("{0} has already been registered", key));
            }

            _RegisteredFactories.Add(key, builder);
        }

        public static object Create(string key, object input)
        {
            IContentBuilder builder;
            if (_RegisteredFactories.TryGetValue(key, out builder))
            {
                return builder.Create(input);
            }

            return null;
        }
    }
}
