using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LFS.Data.API.AeroAPI.Model
{
    public class Flight
    {
        [JsonPropertyName("ident")]
        public string Ident { get; set; }

        [JsonPropertyName("ident_icao")]
        public string IdentICAO { get; set; }

        [JsonPropertyName("ident_iata")]
        public string IdentIATA { get; set; }

        [JsonPropertyName("fa_flight_id")]
        public string FaFlightId { get; set; }

        [JsonPropertyName("operator")]
        public string Operator { get; set; }

        [JsonPropertyName("operator_icao")]
        public string OperatorICAO { get; set; }

        [JsonPropertyName("operator_iata")]
        public string OperatorIATA { get; set; }

        [JsonPropertyName("flight_number")]
        public string FlightNumber { get; set; }

        [JsonPropertyName("registration")]
        public string Registration { get; set; }

        [JsonPropertyName("atc_ident")]
        public string ATCIdent { get; set; }

        [JsonPropertyName("inbound_fa_flight_id")]
        public string InboundFaFlightId { get; set; }

        [JsonPropertyName("codeshares")]
        public IList<string> Codeshares { get; set; }

        [JsonPropertyName("codeshares_iata")]
        public IList<string> CodesharesIATA { get; set; }

        [JsonPropertyName("blocked")]
        public bool IsBlocked { get; set; }

        [JsonPropertyName("diverted")]
        public bool IsDiverted { get; set; }

        [JsonPropertyName("cancelled")]
        public bool IsCancelled { get; set; }

        [JsonPropertyName("position_only")]
        public bool IsPositionOnly { get; set; }

        [JsonPropertyName("origin")]
        public Airport Origin { get; set; }

        [JsonPropertyName("destination")]
        public Airport Destination { get; set; }

        [JsonPropertyName("departure_delay")]
        public long? DepartureDelay { get; set; }

        [JsonPropertyName("arrival_delay")]
        public long? ArrivalDelay { get; set; }

        [JsonPropertyName("filed_ete")]
        public long? FiledETE { get; set; }
        
        [JsonPropertyName("scheduled_out")]
        public DateTime? ScheduledOut { get; set; }

        [JsonPropertyName("estimated_out")]
        public DateTime? EstimatedOut { get; set; }

        [JsonPropertyName("actual_out")]
        public DateTime? ActualOut { get; set; }

        [JsonPropertyName("scheduled_off")]
        public DateTime? ScheduledOff { get; set; }

        [JsonPropertyName("estimated_off")]
        public DateTime? EstimatedOff { get; set; }

        [JsonPropertyName("actual_off")]
        public DateTime? ActualOff { get; set; }

        [JsonPropertyName("scheduled_in")]
        public DateTime? ScheduledIn { get; set; }

        [JsonPropertyName("estimated_in")]
        public DateTime? EstimatedIn { get; set; }

        [JsonPropertyName("actual_in")]
        public DateTime? ActualIn { get; set; }

        [JsonPropertyName("progress_percent")]
        public int? ProgressPercent { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("aircraft_type")]
        public string AircraftType { get; set; }

        [JsonPropertyName("route_distance")]
        public int? RouteDistance { get; set; }

        [JsonPropertyName("filed_airspeed")]
        public int? FiledAirspeed { get; set; }

        [JsonPropertyName("filed_altitude")]
        public int? FiledAltitude { get; set; }

        [JsonPropertyName("route")]
        public string Route { get; set; }

        [JsonPropertyName("baggage_claim")]
        public string BaggageClaim { get; set; }

        [JsonPropertyName("seats_cabin_business")]
        public int? SeatsCabinBusiness { get; set; }

        [JsonPropertyName("seats_cabin_coach")]
        public int? SeatsCabinCoach { get; set; }

        [JsonPropertyName("seats_cabin_first")]
        public int? SeatsCabinFirst { get; set; }

        [JsonPropertyName("gate_origin")]
        public string GateOrigin { get; set; }

        [JsonPropertyName("gate_destination")]
        public string GateDestination { get; set; }

        [JsonPropertyName("terminal_origin")]
        public string TerminalOrigin { get; set; }

        [JsonPropertyName("terminal_destination")]
        public string TerminalDestination { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
