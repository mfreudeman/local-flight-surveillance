using Microsoft.VisualStudio.TestTools.UnitTesting;
using FinalTracker.Web.Views;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace FinalTrackerTests
{
    [TestClass]
    public class WebViewsTest
    {
        [TestMethod]
        public void TestDocumentWebServer()
        {
            DocumentWebServer server = new DocumentWebServer("localhost", 27101);

            Thread.Sleep(10); // Wait a bit to make sure the server bind is completed.

            HttpClient client = new HttpClient();
            var get = client.GetAsync("http://localhost:27101/FinalList.html");
            get.Wait();
            var response = get.Result;

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccessStatusCode);
            var contentTask = response.Content.ReadAsStringAsync();
            contentTask.Wait();
            var content = contentTask.Result;
            Assert.IsTrue(content.Length > 0);
            Assert.AreEqual(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"), response.Content.Headers.ContentType);
        }
    }
}
