using System.Threading.Tasks;

namespace Bottlecap.Components.Bots
{
    public interface IBotAction
    {
        Task<IBotResponse> ProcessAsync(IBot bot);
    }
}
