using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.IO;

namespace FinalTracker.WebAPI
{
    public class WebServicesServer : IDisposable
    {
        private string _host;
        private ushort _port;
        private string _protocol;
        private Dictionary<string, IWebServiceDataProvider> endPoints = new Dictionary<string, IWebServiceDataProvider>();
        private readonly object endPointMutex = new object();
        HttpListener listener;

        private Thread listenerThread;

        public void RegisterEndPoint(string path, IWebServiceDataProvider dataprovider)
        {
            lock (endPointMutex)
            {
                endPoints[path] = dataprovider;
            }
        }

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
                    Debug.WriteLine("Waiting for request", "WebServicesServer");
                    listenerContext = listener.GetContext();
                    Debug.WriteLine("Received Request", "WebServicesServer");

                    HttpListenerRequest request = listenerContext.Request;
                    HttpListenerResponse response = listenerContext.Response;

                    if (request.HttpMethod != "GET")
                    {
                        response.StatusCode = 405;
                        sendResponse(request, response);
                        continue;
                    }

                    IWebServiceDataProvider endPoint;
                    lock (endPointMutex)
                    {
                        endPoint = endPoints[request.Url.AbsolutePath];
                    }

                    if (endPoint == null)
                    {
                        response.StatusCode = 404;
                        sendResponse(request, response);
                        continue;
                    }

                    ThreadPool.QueueUserWorkItem((_) => { handleResponse(endPoint, request, response); });
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
            finally
            {
                listener?.Stop();
                listener?.Abort();
                listener?.Close();
            }

            Debug.WriteLine("Leaving Listener Thread", "WebServicesServer");
        }

        public WebServicesServer(string host, ushort port)
        {
            _protocol = "http";
            _host = host;
            _port = port;

            listenerThread = new Thread(new ThreadStart(HandleConnections));
            listenerThread.Start();
        }

        private void handleResponse(IWebServiceDataProvider endPoint, HttpListenerRequest request, HttpListenerResponse response)
        {
            response.StatusCode = 200;
            response.ContentType = endPoint.ContentType;
            response.ContentEncoding = endPoint.Encoding;
            endPoint.Serialize(response.OutputStream);
            sendResponse(request, response);
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

        ~WebServicesServer()
        {
            Dispose();
        }
    }
}
