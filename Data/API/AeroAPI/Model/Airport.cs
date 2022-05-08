using System.Text.Json.Serialization;

namespace LFS.Data.API.AeroAPI.Model
{
    public class Airport
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("code_icao")]
        public string CodeICAO { get; set; }

        [JsonPropertyName("code_iata")]
        public string CodeIATA { get; set; }

        [JsonPropertyName("code_lid")]
        public string CodeLId { get; set; }

        [JsonPropertyName("airport_info_url")]
        public string AirportInfoURL { get; set; }
    }
}
