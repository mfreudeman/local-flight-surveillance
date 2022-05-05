using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LFS.Data.API.AeroAPI.Model
{
    public enum EAirportFlightsScheduledArrivalsRequestType
    {
        NONE = 0,
        GENERAL_AVIATION = 1,
        AIRLINE = 2
    }

    internal interface IAirportFlightsScheduledArrivalsRequest
    {
        string AirportCode { get; set; }
        string AirlineCode { get; set; }
        EAirportFlightsScheduledArrivalsRequestType FlightType { get; set; }
        int MaxPages { get; set; }
        string Cursor { get; set; }
    }
}
