using System;
using System.Net.Http;
using System.Threading.Tasks;
using LFS.Data.API.AeroAPI.Model;
using System.Text.Json;

namespace LFS.Data.API.AeroAPI
{
    public enum IdentType
    {
        Designator,
        Registration,
        FaFlightId
    }
    public class AeroAPI
    {
        public static ITestDataProvidor TestDataProvidor { get; set; }

        public static async Task<ResultList<Flight>> FlightsByFlightIdentifier(string apiKey, string ident, IdentType identType = IdentType.Designator)
        {
            Uri uri = new Uri(string.Format(
                "{0}/{1}/flights/{2}?ident_type={3}",
                API_URL,
                API_BASE_PATH,
                ident,
                IdentTypeString(identType)));

            return await GetAPIResponse<FlightsResultList>(uri, apiKey);
        }

        private static string API_AUTH_HEADER = "x-apikey";
        private static string API_URL = "https://aeroapi.flightaware.com";
        private static string API_BASE_PATH = "aeroapi";
        private static HttpClient _httpClient = null;

        private static string IdentTypeString(IdentType identType)
        {
            switch (identType)
            {
                case IdentType.Designator: return "designator";
                case IdentType.Registration: return "registration";
                case IdentType.FaFlightId: return "fa_flight_id";
                default: return "";
            }
        }
        private static Task<HttpResponseMessage> MakeRequest(Uri uri, string apiKey)
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
            }

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add(API_AUTH_HEADER, apiKey);

            return _httpClient.GetAsync(uri);
        }
        private static async Task<_T> GetAPIResponse<_T>(Uri requestUri, string apiKey)
        {
            System.IO.Stream responseStream;
            bool responseSuccess;

            if (TestDataProvidor == null)
            {
                var response = await MakeRequest(requestUri, apiKey);
                responseStream = await response.Content.ReadAsStreamAsync();
                responseSuccess = response.IsSuccessStatusCode;
            }
            else // Don't use the web API if a testing data providor has been set.
            {
                responseStream = TestDataProvidor.Response();
                responseSuccess = true;
                System.Threading.Thread.Sleep(100); // Bake in some extra time like if we were waiting for an http response.
            }

            if (responseSuccess)
            {
                return await JsonSerializer.DeserializeAsync<_T>(responseStream);
            }

            return default;
        }
    }
}
