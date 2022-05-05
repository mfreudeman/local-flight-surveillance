using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LFS.Data.API.AeroAPI.Model
{
    internal class AirportFlightsScheduledArrivalsRequest : IAirportFlightsScheduledArrivalsRequest, IRequest
    {
        public AirportFlightsScheduledArrivalsRequest(string airportCode, string cursor = null)
        {
            AirportCode = airportCode;
            Cursor = cursor;
        }

        public string AirportCode { get; set; }
        public string AirlineCode { get; set; }
        public EAirportFlightsScheduledArrivalsRequestType FlightType { get; set; }
        public int MaxPages { get; set; }
        public string Cursor { get; set; }

        public Uri Uri {
            get
            {
                UriBuilder builder = new UriBuilder(AeroAPI.API_URL);
                builder.Path = string.Format("{0}/airports/{1}/flights/scheduled_arrivals", AeroAPI.API_BASE_PATH, AirportCode);

                List<Tuple<string, string>> paramList = new List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("airline", AirlineCode),
                    new Tuple<string, string>("type", FlightType == EAirportFlightsScheduledArrivalsRequestType.GENERAL_AVIATION ? "General_Aviation" : "Airline"),
                    new Tuple<string, string>("max_pages", MaxPages.ToString()),
                    new Tuple<string, string>("cursor", Cursor)
                };
                StringBuilder sb = new StringBuilder();
                bool first = true;
                foreach (var param in paramList)
                {
                    if (param.Item2 != null)
                    {
                        sb.AppendFormat("{2}{0}={1}", param.Item1, param.Item2, first ? "" : "&");
                        first = false;
                    }
                }
                builder.Query = sb.ToString();

                return builder.Uri;
            } 
        }
    }
}
