using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LFS.Data.API.AeroAPI.Model
{
    internal class FlightsResultList : ResultList<Flight>
    {
        public override IList<Flight> Results { get => Flights; set => Flights = value; }

        [JsonPropertyName("flights")]
        public IList<Flight> Flights { get; set; }

    }
}
