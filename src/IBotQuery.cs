using System.Collections.Generic;

namespace Bottlecap.Net.Bots
{
    public interface IBotQuery
    {
        string Name { get; }

        bool IsNativeQuery(QueryType type);

        bool HasToken(string token);

        string GetTokenValue(string token);
    }
}