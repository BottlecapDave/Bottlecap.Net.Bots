using Bottlecap.Net.Bots.Alexa.Data;
using RestSharp;
using System.Threading.Tasks;

namespace Bottlecap.Net.Bots.Alexa.Web
{
    public class DirectiveService
    {
        public const string URI = "v1/directives";

        public async Task<bool> SendDirectiveAsync(string token, string baseUri, Directive directive)
        {
            var client = new RestClient(baseUri);

            var request = new RestRequest(URI);

            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");

            await client.PostAsync<object>(request);

            return true;
        }
    }
}
