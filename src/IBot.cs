using Bottlecap.Authentication;
using System.Threading.Tasks;

namespace Bottlecap.Components.Bots
{
    public interface IBot
    {
        Token AuthToken { get; }

        IBotQuery Query { get; }

        string Locale { get; }

        Task<Pair<decimal, decimal>?> GetLocationAsync();

        Task<Bots.Data.Address> GetAddressAsync();

        Task SendProgressResponseAsync(IBotResponse response);

        Task AddContextAsync(string key, object data);

        Task<T> GetContextAsync<T>(string key) where T : class;

        void Log(string message);

        void Log(string message, params object[] parameters);
    }
}
