using System;
using System.Collections.Generic;
using System.Net;

namespace FinalTracker.Web.API
{
    public class WebServicesServer : HTTPServer, IDisposable
    {
        private Dictionary<string, IWebServiceDataProvider> endPoints = new Dictionary<string, IWebServiceDataProvider>();
        private readonly object endPointMutex = new object();

        public void RegisterEndPoint(string path, IWebServiceDataProvider dataprovider)
        {
            lock (endPointMutex)
            {
                endPoints[path] = dataprovider;
            }
        }

        public WebServicesServer(string host, ushort port) : base(host, port)
        {
        }

        protected override void handleResponse(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            IWebServiceDataProvider endPoint;

            if (request.HttpMethod != "GET")
            {
                response.StatusCode = 405;
                return;
            }

            lock (endPointMutex)
            {
                endPoint = endPoints[request.Url.AbsolutePath];
            }

            if (endPoint == null)
            {
                response.StatusCode = 404;
                return;
            }

            response.StatusCode = 200;
            response.ContentType = endPoint.ContentType;
            response.ContentEncoding = endPoint.Encoding;
            endPoint.Serialize(response.OutputStream);
        }

        ~WebServicesServer()
        {
            Dispose();
        }
    }
}
