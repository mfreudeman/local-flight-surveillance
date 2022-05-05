using System;

namespace RTLSDRADSB
{
    public struct Aircraft
    {
        public DateTime LastMessageTime;
        public string Callsign;
        public string Altitude;
        public string GroundSpeed;
        public string Track;
        public string Latitude;
        public string Longitude;
        public string VerticalRate;
        public string Squawk;
        public bool Emergency;
        public bool IsOnGround;

        public void Dispose()
        {
        }
    }
}
