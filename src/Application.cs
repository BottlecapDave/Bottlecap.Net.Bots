using Bottlecap.Dispatching;
using System.Threading.Tasks;

namespace Bottlecap.Components.Bots
{
    public abstract class Application : Bottlecap.Application
    {
        protected override Task InitialiseAsync()
        {
            DependencyService.Current.Register<IDispatcher, BaseDispatcher>(new BaseDispatcher());

            return Task.FromResult(true);
        }
    }
}
