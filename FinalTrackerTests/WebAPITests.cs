using Microsoft.VisualStudio.TestTools.UnitTesting;
using FinalTracker.WebAPI;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace FinalTrackerTests
{
    class TestDataContext
    {
        public string TestString { get; set; }
        public int TestInt { get; set; }
        public float TestFloat { get; set; }
    }

    [TestClass]
    public class WebAPITests
    {
        [TestMethod]
        public void TestWebServicesServer()
        {
            string testPath = "/test/path";
            TestDataContext dataContext = new TestDataContext()
            {
                TestString = "TestTest",
                TestInt = 42,
                TestFloat = 3.14159f
            };
            JsonDataProvider<TestDataContext> dataProvider = new JsonDataProvider<TestDataContext>(dataContext);
            HttpResponseMessage response;

            using (WebServicesServer server = new WebServicesServer("localhost", 27100))
            {
                server.RegisterEndPoint(testPath, dataProvider);

                Thread.Sleep(10); // Wait a bit for the server to be bound.

                HttpClient client = new HttpClient();
                var get = client.GetAsync("http://localhost:27100" + testPath);
                get.Wait();
                response = get.Result;
            }

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(dataProvider.ContentType, response.Content.Headers.ContentType.ToString());

            var contentTask = response.Content.ReadAsStringAsync();
            contentTask.Wait();
            string content = contentTask.Result;
            TestDataContext receivedData = JsonSerializer.Deserialize<TestDataContext>(content);

            Assert.IsNotNull(receivedData);
            Assert.AreEqual(dataContext.TestString, receivedData.TestString);
            Assert.AreEqual(dataContext.TestInt, receivedData.TestInt);
            Assert.AreEqual(dataContext.TestFloat, receivedData.TestFloat);
        }
    }
}
