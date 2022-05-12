using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Diagnostics;

namespace FinalTracker.WebAPI
{
    public class JsonDataProvider<_T> : IWebServiceDataProvider
    {
        public JsonDataProvider(_T dataContext)
        {
            DataContext = dataContext;
        }
        _T DataContext { get; set; }
        public string ContentType { get => "application/json"; }
        public Encoding Encoding { get => Encoding.UTF8; }

        public void Serialize(Stream s)
        {
            StreamWriter sw = new StreamWriter(s, Encoding);
            string serializedObject = JsonSerializer.Serialize(DataContext);
            sw.Write(serializedObject);
            sw.Flush();
        }
        
        private long _contentLength = 0;
    }
}
