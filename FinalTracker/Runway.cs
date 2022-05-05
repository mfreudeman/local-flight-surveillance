using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LFS.Data;

namespace FinalTracker
{
    internal class Runway
    {
        public Runway(string name, GeoPoint thresholdLocation, double trueHeading, double finalLength)
        {
            Name = name;
            ThresholdLocation = thresholdLocation;
            TrueHeading = trueHeading;
            FinalLength = finalLength;
            FinalPolygon = new List<GeoPoint>(Algorithms.MakeFinalPolygon(thresholdLocation, trueHeading, finalLength));
            RunwayPolygon = new List<GeoPoint>(Algorithms.MakeRunwayPolygon(thresholdLocation, trueHeading, finalLength));
        }
        public readonly string Name;
        public readonly GeoPoint ThresholdLocation;
        public readonly double TrueHeading;
        public readonly double FinalLength;
        public readonly List<GeoPoint> FinalPolygon;
        public readonly List<GeoPoint> RunwayPolygon;
    }
}
