using System.IO;
using System.Text;

namespace FinalTracker.WebAPI
{
    public interface IWebServiceDataProvider
    {
        string ContentType { get; }
        Encoding Encoding { get; }
        void Serialize(Stream s);

    }
}
