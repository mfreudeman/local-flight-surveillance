using System;
using System.Collections.Generic;
using GMap.NET.WindowsPresentation;
using GMap.NET.MapProviders;
using GMap.NET;
using System.Windows.Media;
using System.Windows;
using System.Globalization;

namespace FinalTracker
{
    internal class Map : GMapControl
    {
        public Dictionary<string, RTLSDRADSB.Aircraft> Aircraft = new Dictionary<string, RTLSDRADSB.Aircraft>();

        public Map()
        {
            MapProvider = GoogleTerrainMapProvider.Instance;
            Position = new PointLatLng(40.64313977072645, -73.78134749564886);
            Zoom = 12;
        }

        public void SetAirport(Airport newAirport)
        {
            _airports.Add(newAirport);
        }

        public void SetAircraftReceiver(RTLSDRADSB.AircraftReceiver receiver)
        {
            _receiver = receiver;
            _receiver.OnAircraftListUpdated += OnAircraftListUpdated;
        }

        public bool IsAircraftOnFinal(RTLSDRADSB.Aircraft aircraft, out Airport onFinalAirport, out Runway onFinalRunway)
        {
            LFS.Data.GeoPoint aircraftLocation = new LFS.Data.GeoPoint(double.Parse(aircraft.Latitude), double.Parse(aircraft.Longitude));

            foreach (Airport airport in _airports)
            {
                foreach (Runway runway in airport.ActiveRunways)
                {
                    if (LFS.Data.Algorithms.PointInPolygon(aircraftLocation, runway.FinalPolygon.ToArray()))
                    {
                        onFinalAirport = airport;
                        onFinalRunway = runway;
                        return true;
                    }
                }
            }

            onFinalAirport = null;
            onFinalRunway = null;
            return false;
        }

        private void OnAircraftListUpdated()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Aircraft = new Dictionary<string, RTLSDRADSB.Aircraft>(_receiver.AircraftList);
                InvalidateVisual();
            }));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_airports.Count == 0)
            {
                return;
            }

#if DEBUG
            DrawFinalCones(drawingContext);
#endif

            DrawAircraft(drawingContext);
        }

        private void DrawAircraft(DrawingContext drawingContext)
        {
            const double leaderLineLength = 0.016666; // length in nautical miles / 60

            foreach (var aircraft in Aircraft)
            {
                if (!string.IsNullOrEmpty(aircraft.Value.Latitude) && !string.IsNullOrEmpty(aircraft.Value.Longitude))
                {
                    PointLatLng aircraftLatLon = new PointLatLng(double.Parse(aircraft.Value.Latitude), double.Parse(aircraft.Value.Longitude));
                    GPoint gPoint = FromLatLngToLocal(aircraftLatLon);
                    Point localPoint = new Point(gPoint.X, gPoint.Y);

                    drawingContext.DrawText(
                        new FormattedText(
                            string.Format(
                                "{0} {5} {6}\r\n{3} {4}\r\n{1}/{2}",
                                string.IsNullOrEmpty(aircraft.Value.Callsign) ? string.Format("#{0}#", aircraft.Key) : aircraft.Value.Callsign,
                                aircraft.Value.Latitude,
                                aircraft.Value.Longitude,
                                string.IsNullOrEmpty(aircraft.Value.Altitude) ? null : Math.Round(double.Parse(aircraft.Value.Altitude) / 100.0, 0).ToString().PadLeft(3, '0'),
                                aircraft.Value.VerticalRate,
                                aircraft.Value.Track?.PadLeft(3, '0'),
                                aircraft.Value.GroundSpeed?.PadLeft(3, '0')
                            ),
                            CultureInfo.GetCultureInfo("en-US"),
                            FlowDirection.LeftToRight,
                            new Typeface("Cascadia Mono"),
                            10,
                            Brushes.Black,
                            VisualTreeHelper.GetDpi(this).PixelsPerDip
                        ),
                        new Point(gPoint.X + 4, gPoint.Y + 4)
                    );

                    drawingContext.DrawEllipse(null, new Pen(Brushes.Black, 1.0), localPoint, 4, 4);
                    drawingContext.DrawEllipse(Brushes.Black, null, localPoint, 2.0, 2.0);

                    if (aircraft.Value.Track != null)
                    {
                        double track = double.Parse(aircraft.Value.Track);
                        double trackRad = (450.0 - track) % 360.0 * DEG2RAD;
                        double leaderEndLat = (Math.Sin(trackRad) * leaderLineLength) + aircraftLatLon.Lat;
                        double leaderEndLon = (Math.Cos(trackRad) * leaderLineLength) + aircraftLatLon.Lng;
                        GPoint gPointLeaderEnd = FromLatLngToLocal(new PointLatLng(leaderEndLat, leaderEndLon));
                        Point leaderEndPoint = new Point(gPointLeaderEnd.X, gPointLeaderEnd.Y);

                        drawingContext.DrawLine(new Pen(Brushes.Black, 1.0), localPoint, leaderEndPoint);
                    }

                }
            }
        }

        private void DrawFinalCones(DrawingContext drawingContext)
        {
            foreach (Airport airport in _airports)
            {
                foreach (var runway in airport.ActiveRunways)
                {
                    //const double halfPiRadian = Math.PI / 2;
                    double headingAsRadian = ((450.0 - runway.TrueHeading) % 360.0) * DEG2RAD;
                    double armLength = 4.0 / 60.0;
                    double lat = (Math.Sin(headingAsRadian) * armLength) + runway.ThresholdLocation.Latitude;
                    double lon = (Math.Cos(headingAsRadian) * armLength) + runway.ThresholdLocation.Longitude;
                    GPoint centerLineEnd = FromLatLngToLocal(new PointLatLng(lat, lon));
                    GPoint centerLineStart = FromLatLngToLocal(new PointLatLng(runway.ThresholdLocation.Latitude, runway.ThresholdLocation.Longitude));
                    drawingContext.DrawLine(new Pen(Brushes.Green, 2.0), new Point(centerLineStart.X, centerLineStart.Y), new Point(centerLineEnd.X, centerLineEnd.Y));

                    drawFinalCone(drawingContext, runway);
                }
            }
        }

        private void drawFinalCone(DrawingContext drawingContext, Runway runway)
        {
            Pen pen = new Pen(Brushes.Red, 2.0);

            List<GPoint> finalCone = new List<GPoint>();
            foreach (var point in runway.FinalPolygon)
            {
                finalCone.Add(FromLatLngToLocal(new PointLatLng(point.Latitude, point.Longitude)));
            }
            GPoint lastPoint = finalCone[0];
            for (int i = 1; i < finalCone.Count; i++)
            {
                GPoint curPoint = finalCone[i];

                drawingContext.DrawLine(pen, new Point(lastPoint.X, lastPoint.Y), new Point(curPoint.X, curPoint.Y));
                drawText(drawingContext, i.ToString(), new Point(curPoint.X, curPoint.Y));

                lastPoint = curPoint;
            }
            drawingContext.DrawLine(pen, new Point(lastPoint.X, lastPoint.Y), new Point(finalCone[0].X, finalCone[0].Y));
            drawText(drawingContext, "0", new Point(finalCone[0].X, finalCone[0].Y));
        }

        private void drawText(DrawingContext context, string text, Point point)
        {
            context.DrawText(
                new FormattedText(
                    text,
                    System.Globalization.CultureInfo.GetCultureInfo("en-US"),
                    FlowDirection.LeftToRight,
                    new Typeface("monospace"),
                    12.0, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip
                ), 
                point
            );
        }

        private List<Airport> _airports = new List<Airport>();
        private RTLSDRADSB.AircraftReceiver _receiver = null;

        const double DEG2RAD = Math.PI / 180.0;
    }
}
