using System.IO;

namespace Bottlecap.Net.Bots.Resources
{
    public interface IResourceManager
    {
        void Initialise(Stream stream);

        string GetResource(string key);
    }
}
