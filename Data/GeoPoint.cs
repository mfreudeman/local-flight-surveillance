using System;
using System.Windows;

namespace LFS.Data
{
    public class GeoPoint : IFormattable
    {
        private Point _point;

        public GeoPoint(double latitude, double longitude)
        {
            _point = new Point(longitude, latitude);
        }
        public GeoPoint()
        {
            _point = new Point();
        }

        public double Latitude
        {
            get { return _point.Y; }
            set { _point.Y = value; }
        }

        public double Longitude
        { 
            get { return _point.X; }
            set { _point.X = value; }
        }

        public Point Point
        {
            get { return _point; }
        }

        public override string ToString()
        {
            return _point.ToString();
        }

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return _point.ToString(formatProvider);
        }
    }
}
