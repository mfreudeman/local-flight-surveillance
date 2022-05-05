using System;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace LFS.Data
{
    public static class ADSBexchangeAPI
    {
        private static string APIKEY = "681b1f5b73msh3424377f4d06e01p17bc7bjsn089d27691e50";
        private static string APIHOST = "adsbexchange-com1.p.rapidapi.com";
        private static TimeSpan MinTimeBetweenRequests = new TimeSpan(0, 0, 45);
        private static DateTime LastRequestTime = DateTime.MinValue;

        public static JObject AircraftWithinRadius(double CenterLatitude, double CenterLongitude, uint Radius = 25 )
        {
            if (DateTime.Now - LastRequestTime < MinTimeBetweenRequests)
            {
                return null;
            }

            RestClient client = new RestClient(String.Format("https://{3}/v2/lat/{0}/lon/{1}/dist/{2}/", CenterLatitude, CenterLongitude, Radius, APIHOST));
            
            RestRequest request = new RestRequest();
            request.AddHeader("X-RapidAPI-Host", APIHOST);
            request.AddHeader("X-RapidAPI-Key", APIKEY);
            Task<RestResponse> responseTask = client.ExecuteAsync(request);
            
            responseTask.Wait();

            RestResponse response = responseTask.Result;

            LastRequestTime = DateTime.Now;

            return new JObject(response.Content);
        }
    }
}
