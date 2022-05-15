using RTLSDRADSB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NLua;
using LFS.Data.API.AeroAPI;
using LFS.Data.API.AeroAPI.Model;
using System.Diagnostics;

/* TODO
 * X Add GMap to MainWindow
 * Add points from MakeFinalPolygon to map.
 * Draw lines between points of the final cone.
 * 
 * Show results from ADSBexchange API on map.
 * 
 * Change color of airplanes if they're within final polygon.
 * 
 * Track aircraft by callsign, seen before, last seen
 *      Track distanceMoved, lastDistanceMoved (lagged 1 update)
 *      Remove aircraft last seen > 2 minutes or distanceMoved + lastDistanceMoved < 50ft
 *      Seen in flight: Alt above 500 + field elevation
 *      if in final cone or (in runway box and seen in flight) then
 *          arriving
 *      else if in runway box then
 *          departing
 *          
 * Move to FlightTracker class, control UI with events.
 * Events:
 *      OnAircraftDeparting - Add departing card
 *      OnAircraftDeparted - Remove departing card
 *      OnAircraftArriving - Add arrival card
 *      OnAircraftVacated - Remove arrival card
 *      OnAircraftShortFinal - Highlight arrival card
 *      OnAircraftLost - Remove from any UI element
 *      OnAircraftInRange - Add to in range list, order by first seen?, remove from any in range list once on arrival card stack
 */





 /*  
 * AeroAPI Data Received
 *  Lookup cached aircraft
 *  Add flight data to cached aircraft
 * 
 */

namespace FinalTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RTLSDRADSB.AircraftReceiver acftRcvr;

        static Lua luaState;

        string aeroAPIKey;

        IDictionary<string, Aircraft> aircraftCache = new ConcurrentDictionary<string, Aircraft>();

        ConcurrentBag<Airport> activeAirports = new ConcurrentBag<Airport>();

        public MainWindow()
        {
            InitializeComponent();

            luaState = new Lua();

            aeroAPIKey = File.ReadAllText("aeroapi.key");

            acftRcvr = new RTLSDRADSB.AircraftReceiver(1);
            acftRcvr.OnAircraftDataReceived += OnModeSDataReceived;

            Airport airportJFK = new Airport("John F Kennedy International Airport", "JFK", "KJFK");
            airportJFK.SetRunway(new Runway("4L", new LFS.Data.GeoPoint(40.622021, -73.785584383), 38.0, 5.0));
            airportJFK.SetRunway(new Runway("4R", new LFS.Data.GeoPoint(40.625425, -73.770347216), 38.0, 5.0));
            Airport airportLGA = new Airport("LaGuardia Airport", "LGA", "KLGA");
            airportLGA.SetRunway(new Runway("4", new LFS.Data.GeoPoint(40.76916463, -73.88411955), 39.5, 10.0));
            activeAirports.Add(airportJFK);
            activeAirports.Add(airportLGA);
            FinalMap.SetAirport(airportLGA);
            FinalMap.SetAircraftReceiver(acftRcvr);

            //PointUnknown();

            /*
            LFS.Data.API.AeroAPI.AeroAPI aeroAPI = new LFS.Data.API.AeroAPI.AeroAPI(new System.Net.Http.HttpClient(), "ai0h9VbuvhJ4vNznTUsVq3eVAtlAg3Q0");
            var jsonObj = aeroAPI.AirportsFlightsScheduledArrivalsRequest("KJFK", null, LFS.Data.API.AeroAPI.Model.EAirportFlightsScheduledArrivalsRequestType.AIRLINE, 5, null);
            var num_pages = (int)jsonObj["num_pages"];
            Console.WriteLine("num_pages = {0}", num_pages);
            */
        }

        private void OnModeSDataReceived(RTLSDRADSB.Aircraft modeSAircraftData)
        {
            bool modeSCodeKnown = aircraftCache.ContainsKey(modeSAircraftData.ModeSCode);
            Aircraft aircraft;

            if (modeSCodeKnown)
            {
                aircraft = aircraftCache[modeSAircraftData.ModeSCode];
            }
            else
            {
                aircraft = new Aircraft();
                aircraft.ModeSData = modeSAircraftData;
            }

            if (AircraftHasLocationData())
            {
                // Detect runway poly intersections
                foreach (var airport in activeAirports)
                {
                    Runway usingRunway = null;
                    LFS.Data.GeoPoint aircraftPos = new LFS.Data.GeoPoint(double.Parse(aircraft.ModeSData.Latitude), double.Parse(aircraft.ModeSData.Longitude));
                    if (aircraft.ModeSData.IsOnGround)
                    {
                        usingRunway = airport.GetRunwayOn(aircraftPos);
                    }
                    else
                    {
                        usingRunway = airport.GetFinal(aircraftPos);

                        if (usingRunway != null && IsArriving())
                        {
                            aircraft.Arriving = true; // On final and descending, never set to false once detected.
                        }
                    }

                    if (usingRunway != null)
                    {
                        aircraft.Runway = usingRunway;
                        aircraft.Airport = airport;
                    }
                }

                aircraft.GoAround = IsGoingAround();
            }

            if (aircraft.Flight == null && !string.IsNullOrEmpty(aircraft.ModeSData.Callsign))
            {
                aircraft.Flight = new Flight(); // Set an empty object so we don't call the API again for this aircraft.
                AeroAPI.FlightsByFlightIdentifier(aeroAPIKey, aircraft.ModeSData.Callsign).ContinueWith(OnFlightDataReceived);
            }

            aircraftCache[modeSAircraftData.ModeSCode] = aircraft;

            AfterAircraftUpdate();


            bool IsArriving()
            {
                return double.Parse(aircraft.ModeSData.VerticalRate) < 0d;
            }

            bool IsGoingAround()
            {
                return aircraft.Arriving && double.Parse(aircraft.ModeSData.Altitude) < 3000.0 && double.Parse(aircraft.ModeSData.VerticalRate) > 100d;
            }

            bool AircraftHasLocationData()
            {
                return !string.IsNullOrEmpty(aircraft.ModeSData.Latitude) && !string.IsNullOrEmpty(aircraft.ModeSData.Longitude);
            }
        }

        private void AfterAircraftUpdate()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                UpdateFinalItems();
                UpdateGoAroundItems();
            }));
        }

        private void OnFlightDataReceived(Task<ResultList<Flight>> resultTask)
        {
            var flightsList = resultTask.Result.Results;
            if (flightsList != null && flightsList.Count > 0)
            {
                Flight inProgressFlight = LFS.Data.API.AeroAPI.Helper.FlightsHelper.FirstInProgress(flightsList);
                if (inProgressFlight != null)
                {
                    var aircraft = aircraftCache
                        .Where(a => a.Value.ModeSData.Callsign == inProgressFlight.Ident)
                        .First();
                    aircraft.Value.Flight = inProgressFlight;
                    aircraftCache[aircraft.Key] = aircraft.Value;
                }
            }
        }

        private void UpdateFinalItems()
        {
            List<Tuple<double, OnFinalDisplayItem>> onFinalItems = new List<Tuple<double, OnFinalDisplayItem>>();
            foreach (var aircraft in aircraftCache.Select(a => a.Value).Where(a => a.Runway != null))
            {
                LFS.Data.GeoPoint aircraftLocation = new LFS.Data.GeoPoint(double.Parse(aircraft.ModeSData.Latitude), double.Parse(aircraft.ModeSData.Longitude));
                double distance = LFS.Data.Algorithms.Haversine(aircraft.Runway.ThresholdLocation, aircraftLocation);
                double speed, altitude, verticalRate;

                ExtractPropsFromAircraft(aircraft.ModeSData, out speed, out altitude, out verticalRate);

                onFinalItems.Add(MakeFinalItem(
                    aircraft.ModeSData,
                    aircraft.Flight,
                    aircraft.Airport,
                    aircraft.Runway,
                    distance,
                    speed));
            }

            SetOnFinalItems(onFinalItems);
        }

        private static Tuple<double, OnFinalDisplayItem> MakeFinalItem(RTLSDRADSB.Aircraft aircraft, Flight flight, Airport OnFinalAirport, Runway OnFinalRunway, double distanceOut, double speed)
        {
            string registration = "";
            string typeCode = "";
            long duration = 0;
            long routeDistance = 0;
            string originCode = "";

            if (flight != null)
            {
                registration = flight.Registration;
                typeCode = flight.AircraftType;
                duration = flight.FiledETE == null ? 0L : (long)flight.FiledETE;
                routeDistance = flight.RouteDistance == null ? 0L : (long)flight.RouteDistance;
                originCode = flight.Origin.Code;
            }

            return new Tuple<double, OnFinalDisplayItem>(distanceOut, new OnFinalDisplayItem()
            {
                Callsign = string.IsNullOrEmpty(aircraft.Callsign) ? string.Format("#{0}#", aircraft.ModeSCode) : aircraft.Callsign,
                Registration = registration,
                TypeCode = typeCode,
                SecondsOut = speed > 0 ? (long)(distanceOut / speed) : 0,
                DistanceOut = (long)Math.Round(distanceOut, 0),
                DistanceOutUnits = "nm",
                Duration = duration,
                Distance = routeDistance,
                DistanceUnits = "nm",
                OriginCode = originCode,
                Runway = string.Format("{0} {1}", OnFinalAirport.IATACode, OnFinalRunway.Name)
            });
        }

        private void SetOnFinalItems(List<Tuple<double, OnFinalDisplayItem>> onFinalItems)
        {
            onFinalItems.Sort((a, b) => a.Item1 > b.Item1 ? 1 : -1);
            FinalStackPanel.Children.Clear();
            foreach (var item in onFinalItems)
            {
                FinalStackPanel.Children.Add(item.Item2);
            }
        }

        private static void ExtractPropsFromAircraft(RTLSDRADSB.Aircraft aircraft, out double speed, out double altitude, out double verticalRate)
        {
            if (!double.TryParse(aircraft.GroundSpeed, out speed))
            {
                speed = 0;
            }
            if (!double.TryParse(aircraft.Altitude, out altitude))
            {
                altitude = 0;
            }
            if (!double.TryParse(aircraft.VerticalRate, out verticalRate))
            {
                verticalRate = 0;
            }
        }

        private void UpdateGoAroundItems()
        {
            aircraftCache.Where(a => a.Value.GoAround).Select(a => a.Value).ToList().ForEach(aircraft =>
            {
                GoAroundStackPanel.Children.Add(new GoAroundDisplayItem()
                {
                    Callsign = string.IsNullOrEmpty(aircraft.ModeSData.Callsign) ? string.Format("#{0}#", aircraft.ModeSData.ModeSCode) : aircraft.ModeSData.Callsign,
                    Registration = aircraft.Flight?.Registration,
                    TypeCode = aircraft.Flight?.AircraftType,
                    OriginCode = aircraft.Flight?.Origin?.CodeICAO,
                    Runway = string.Format("{0} {1}", aircraft.Airport?.ICAOCode, aircraft.Runway?.Name)
                });
            });
        }

        private void FinalMap_Loaded(object sender, RoutedEventArgs e)
        {
            GMap.NET.WindowsPresentation.GMapControl map = (GMap.NET.WindowsPresentation.GMapControl)sender;
            map.Zoom = 10.0;
        }
        /*
       private void PointInside()
       {
           ResultText = "= Inside";
           ForegroundColor = BRUSH_GREEN;
       }

       private void PointOutside()
       {
           ResultText = "= Outside";
           ForegroundColor = BRUSH_RED;
       }

       private void PointUnknown()
       {
           ResultText = "=";
           ForegroundColor = BRUSH_BLACK;
       }

       private void CalcPoint()
       {
           try
           {
               Point point1 = new Point(double.Parse(P1Lat.Text), double.Parse(P1Lon.Text));
               Point point2 = new Point(double.Parse(P2Lat.Text), double.Parse(P2Lon.Text));
               Point point3 = new Point(double.Parse(P3Lat.Text), double.Parse(P3Lon.Text));
               Point point4 = new Point(double.Parse(P4Lat.Text), double.Parse(P4Lon.Text));
               Point hit = new Point(double.Parse(HitLat.Text), double.Parse(HitLon.Text));

               if (LFS.Data.Algorithms.PointInPolygon(hit, new Point[] { point1, point2, point3, point4}))
               {
                   PointInside();
               }
               else
               {
                   PointOutside();
               }
           }
           catch (Exception)
           {
               PointUnknown();
           }
       }

       public string ResultText
       {
           get => (string)GetValue(ResultTextProperty);
           set => SetValue(ResultTextProperty, value);
       }

       public Brush ForegroundColor
       {
           get => (Brush)GetValue(ForegroundColorProperty);
           set => SetValue(ForegroundColorProperty, value);
       }

       private void TextChanged(object sender, TextChangedEventArgs e)
       {
           CalcPoint();
       }

       private void Grid_Loaded(object sender, RoutedEventArgs e)
       {
           Binding ResultTextBinding = new Binding("ResultText");
           ResultTextBinding.Mode = BindingMode.OneWay;
           ResultTextBinding.Source = this;

           Binding ResultForegroundBinding = new Binding("ForegroundColor");
           ResultForegroundBinding.Mode = BindingMode.OneWay;
           ResultForegroundBinding.Source = this;

           BindingOperations.SetBinding(HitInside, ContentProperty, ResultTextBinding);
           BindingOperations.SetBinding(HitInside, ForegroundProperty, ResultForegroundBinding);
       }*/
        }
    }
