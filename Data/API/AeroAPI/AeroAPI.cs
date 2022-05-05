using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LFS.Data.API.AeroAPI
{
    public class AeroAPI
    {
        public const string API_AUTH_HEADER = "x-apikey";
        public const string API_URL = "https://aeroapi.flightaware.com";
        public const string API_BASE_PATH = "aeroapi";

        public AeroAPI(HttpClient client, string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = client;
        }

        public Newtonsoft.Json.Linq.JObject AirportsFlightsScheduledArrivalsRequest(
            string airportCode, 
            string airlineCode = null, 
            Model.EAirportFlightsScheduledArrivalsRequestType type = Model.EAirportFlightsScheduledArrivalsRequestType.NONE, 
            int maxPages = 0, 
            string cursor = null
        )
        {
            if (AirportsFlightsScheduledArrivalsMeter != null)
            {
                if (AirportsFlightsScheduledArrivalsMeter.CanCall())
                {
                    AirportsFlightsScheduledArrivalsMeter.Called();
                }
                else
                {
                    return null;
                }
            }

            Model.AirportFlightsScheduledArrivalsRequest request = new Model.AirportFlightsScheduledArrivalsRequest(airportCode, cursor);
            request.AirlineCode = airlineCode;
            request.FlightType = type;
            request.MaxPages = maxPages;
            return getResponse(request);
        }

        public APIMeter AirportsFlightsScheduledArrivalsMeter { get; set; }

        private string _apiKey;
        private HttpClient _httpClient;

        private Newtonsoft.Json.Linq.JObject getResponse(Model.IRequest requestData)
        {
            var responseTask = Task.Run<Newtonsoft.Json.Linq.JObject>(async () => { return await request(_httpClient, requestData); });
            responseTask.Wait();
            return responseTask.Result;
        }

        private async Task<Newtonsoft.Json.Linq.JObject> request(HttpClient client, Model.IRequest request)
        {
            if (!client.DefaultRequestHeaders.Contains(API_AUTH_HEADER))
            {
                client.DefaultRequestHeaders.Add(API_AUTH_HEADER, _apiKey);
            }

            HttpResponseMessage response = await client.GetAsync(request.Uri);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Newtonsoft.Json.Linq.JObject.Parse(await response.Content.ReadAsStringAsync());
            }
            else
            {
                return null;
            }
        }
    }
}
