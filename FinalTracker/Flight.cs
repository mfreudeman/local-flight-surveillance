namespace FinalTracker
{
    internal class Aircraft
    {
        public bool Arriving;
        public bool GoAround;
        public LFS.Data.API.AeroAPI.Model.Flight Flight { get; set; }
        public RTLSDRADSB.Aircraft ModeSData { get; set; }
        public Airport Airport { get; set; }
        public Runway Runway { get; set; }
    }
}
