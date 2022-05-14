using System.IO;
using System.Text;

namespace FinalTracker.Web.API
{
    public interface IWebServiceDataProvider
    {
        string ContentType { get; }
        Encoding Encoding { get; }
        void Serialize(Stream s);

    }
}
