using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace FinalTracker.Web.Views
{
    public class DocumentWebServer : HTTPServer, IDisposable
    {
        private Dictionary<string, string> _documentTypes;
        public DocumentWebServer(string host, ushort port) : base(host, port)
        {
            _documentTypes = new Dictionary<string, string>()
            {
                { ".html", "text/html" },
                { ".htm", "text/html" },
                { ".txt", "text/plain" },
                { ".xml", "text/xml" },
                { ".csv", "text/csv" },
                { ".css", "text/css" },
                { ".js", "application/javascript" },
                { ".json", "application/json" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".ico", "image/x-icon" },
                { ".tiff", "image/tiff" }
            };
        }

        protected override void handleResponse(HttpListenerRequest request, ref HttpListenerResponse response)
        {
            string filePath = Path.Combine(Environment.CurrentDirectory, @"public") + request.Url.AbsolutePath.Replace('/','\\');
            var data = new List<byte>();
            
            FileInfo fileInfo = new FileInfo(filePath);
            string docType;
            if (!_documentTypes.TryGetValue(fileInfo.Extension, out docType))
            {
                docType = "text/plain";
            }

            response.ContentType = docType;

            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    data.AddRange(Encoding.UTF8.GetBytes(line));
                }
            }
            using (StreamWriter sw = new StreamWriter(response.OutputStream))
            {
                sw.Write(Encoding.UTF8.GetString(data.ToArray()));
                sw.Flush();
            }
        }
    }
}
