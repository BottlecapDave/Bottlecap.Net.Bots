using System.Threading.Tasks;

namespace Bottlecap.Net.Bots
{
    public interface IBotAction
    {
        Task<IBotResponse> ProcessAsync(IBot bot);
    }
}
