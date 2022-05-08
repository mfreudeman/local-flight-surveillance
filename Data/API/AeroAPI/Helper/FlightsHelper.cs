using System.Collections.Generic;
using System.Linq;
using LFS.Data.API.AeroAPI.Model;

namespace LFS.Data.API.AeroAPI.Helper
{
    public static class FlightsHelper
    {
        public static Flight FirstInProgress(IList<Flight> flights)
        {
            return flights.Where(flight => flight.ProgressPercent > 0 && flight.ProgressPercent < 100).FirstOrDefault();
        }
    }
}
