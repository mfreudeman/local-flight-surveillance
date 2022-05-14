using System;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace FinalTracker.Web
{
    public abstract class HTTPServer : IDisposable
    {
        private string _host;
        private ushort _port;
        private string _protocol;
        private HttpListener listener;
        private Thread listenerThread;

        public void HandleConnections()
        {
            listener = new HttpListener();
            string uriPrefix = string.Format("{0}://{1}:{2}/", _protocol, _host, _port);
            listener.Prefixes.Add(uriPrefix);
            listener.Start();
            Debug.WriteLine("Listening on " + uriPrefix, "WebServicesServer");

            bool running = true;

            try
            {
                while (running)
                {
                    HttpListenerContext listenerContext = null;
                    listenerContext = listener.GetContext();

                    HttpListenerRequest request = listenerContext.Request;
                    HttpListenerResponse response = listenerContext.Response;

                    ThreadPool.QueueUserWorkItem((_) => {
                        handleResponse(request, ref response);
                        sendResponse(request, response);
                    });
                }
            }
            catch (ThreadAbortException)
            {
                Debug.WriteLine("Listener Thread Aborted", "WebServicesServer");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "WebServicesServer");
            }

            Debug.WriteLine("Leaving Listener Thread", "WebServicesServer");
        }

        protected abstract void handleResponse(HttpListenerRequest request, ref HttpListenerResponse response);

        public HTTPServer(string host, ushort port)
        {
            _protocol = "http";
            _host = host;
            _port = port;

            listenerThread = new Thread(new ThreadStart(HandleConnections));
            listenerThread.Start();
        }

        private void sendResponse(HttpListenerRequest request, HttpListenerResponse response)
        {
            log(request, response);
            response.Close();
        }

        private static void log(HttpListenerRequest request, HttpListenerResponse response)
        {
            Debug.WriteLine(string.Format("[{0}][HTTP/{1}.{2}] {3} {4}", //[200][HTTP/1.1] GET /path/to/resource
                                          response.StatusCode,
                                          request.ProtocolVersion.Major,
                                          request.ProtocolVersion.Minor,
                                          request.HttpMethod,
                                          request.Url.AbsolutePath
                                        ), "WebServicesServer");
        }

        public void Dispose()
        {
            listener?.Abort();
            listener?.Close();
            listenerThread?.Abort();
            listenerThread?.Join();
            listener = null;
            listenerThread = null;
        }

        ~HTTPServer()
        {
            Dispose();
        }
    }
}
