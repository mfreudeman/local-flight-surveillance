using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalTracker
{
    internal class Airport
    {
        public Airport (string name, string iataCode, string icaoCode)
        {
            Name = name;
            IATACode = iataCode;
            ICAOCode = icaoCode;
        }

        public readonly string ICAOCode;
        public readonly string IATACode;
        public readonly string Name;
        public Dictionary<Runway, bool> Runways { get => _runways; }

        public List<Runway> ActiveRunways
        {
            get => (
                from runway in _runways
                where runway.Value == true
                select runway.Key
            ).ToList();
        }

        public Runway GetFinal(LFS.Data.GeoPoint point)
        {
            foreach(Runway r in ActiveRunways)
            {
                if (LFS.Data.Algorithms.PointInPolygon(point, r.FinalPolygon.ToArray()))
                {
                    return r;
                }
            }
            return null;
        }

        public Runway GetRunwayOn(LFS.Data.GeoPoint point)
        {
            foreach (Runway r in ActiveRunways)
            {
                if (LFS.Data.Algorithms.PointInPolygon(point, r.RunwayPolygon.ToArray()))
                {
                    return r;
                }
            }
            return null;
        }
        
        public void SetRunway(Runway runway, bool isActive = true)
        {
            _runways[runway] = isActive;
        }

        private Dictionary<Runway, bool> _runways = new Dictionary<Runway, bool>();
    }
}
