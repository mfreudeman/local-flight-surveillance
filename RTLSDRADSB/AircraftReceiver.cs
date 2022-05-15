using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;

namespace RTLSDRADSB
{
    public class AircraftReceiver : dump1090
    {
        public delegate void AircraftDataChanged(Aircraft aircraft);

        public AircraftDataChanged OnAircraftDataReceived;
        public AircraftDataChanged OnAircraftDataTimeout;

        public AircraftReceiver(int deviceIndex): base(string.Format("--device-index {0} --quiet --fix --net --net-ro-size 500 --net-ro-rate 5 --net-buffer 5", deviceIndex))
        {
            client = new TcpClient();
            connectionThread = new Thread(new ThreadStart(() => ConnectSBS()));
            connectionThread.Start();
        }

        ~AircraftReceiver()
        {
            if (_connected)
            {
                _connected = false;
                client.Client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            connectionThread?.Abort();
            connectionThread?.Join();
        }

        public ConcurrentDictionary<string, Aircraft> AircraftList { get => _aircraft; }

        private void PruneAircraft()
        {
            TimeSpan timeout = new TimeSpan(TimeSpan.TicksPerSecond * 15L);
            foreach(KeyValuePair<string, Aircraft> aircraft in _aircraft)
            {
                if (DateTime.Now - aircraft.Value.LastMessageTime > timeout)
                {
                    _aircraft.TryRemove(aircraft.Key, out _);
                    ThreadPool.QueueUserWorkItem((a) => { OnAircraftDataTimeout?.Invoke((Aircraft)a); }, aircraft.Value);
                }
            }
        }

        private void ConnectSBS()
        {
            byte[] buffer = new byte[2048];
            
            ClientConnect(30003);

            NetworkStream ns = client.GetStream();

            while (_connected)
            {
                int bytesRead = ns.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    _connected = false;
                    break;
                }

                string bufString = Encoding.UTF8.GetString(buffer).TrimEnd(new char[] { '\0' });
                const string msgPattern = @"(?<MessageType>SEL|ID|AIR|STA|CLK|MSG),(?<TransmissionType>[0-9]*),(?<SessionID>[0-9A-Z]*),(?<AircraftID>[0-9A-Z]*),(?<ModeSCode>[0-9A-F]*),(?<FlightID>[0-9A-Z]*),(?<GeneratedDate>(?<GeneratedDateYear>[0-9]{4})/(?<GeneratedDateMonth>[0-9]{2})/(?<GeneratedDateDay>[0-9]{2})),(?<GeneratedTime>(?<GeneratedTimeHour>[0-9]{2}):(?<GeneratedTimeMinute>[0-9]{2}):(?<GeneratedTimeSecond>[0-9.]*)),(?<LoggedDate>(?<LoggedDateYear>[0-9]{4})/(?<LoggedDateMonth>[0-9]{2})/(?<LoggedDateDay>[0-9]{2})),(?<LoggedTime>(?<LoggedTimeHour>[0-9]{2}):(?<LoggedTimeMinute>[0-9]{2}):(?<LoggedTimeSecond>[0-9.]*))(?<AircraftInfo>,(?<Callsign>[A-Z0-9 ]*),(?<Altitude>[-0-9]*),(?<GroundSpeed>[-0-9.]*),(?<Track>[-0-9.]*),(?<Latitude>[-0-9.]*),(?<Longitude>[-0-9.]*),(?<VerticalRate>[-0-9.]*),(?<Squawk>[0-7]{4})?,(?<SquawkFlag>[0-1]?),(?<EmergencyFlag>[0-1]?),(?<IdentFlag>[0-1]?),(?<IsOnGroundFlag>[0-1]?))?";
                MatchCollection matches = Regex.Matches(bufString, msgPattern);

                Debug.WriteLineIf(bufString.Length > buffer.Length - 50, string.Format("TCP Download Buffer Full"), GetType().Name);

                foreach (Match match in matches)
                {
                    string messageType = match.Groups[@"MessageType"].Value;
                    string transmissionType = match.Groups[@"TransmissionType"].Value;
                    string modeSCode = match.Groups[@"ModeSCode"].Value;
                    string flightID = match.Groups[@"FlightID"].Value;
                    string generatedDateYear = match.Groups[@"GeneratedDateYear"].Value;
                    string generatedDateMonth = match.Groups[@"GeneratedDateMonth"].Value;
                    string generatedDateDay = match.Groups[@"GeneratedDateDay"].Value;
                    string generatedTimeHour = match.Groups[@"GeneratedTimeHour"].Value;
                    string generatedTimeMinute = match.Groups[@"GeneratedTimeMinute"].Value;
                    string generatedTimeSecond = match.Groups[@"GeneratedTimeSecond"].Value.Split('.')[0];
                    string loggedDateYear = match.Groups[@"GeneratedDateYear"].Value;
                    string loggedDateMonth = match.Groups[@"GeneratedDateMonth"].Value;
                    string loggedDateDay = match.Groups[@"GeneratedDateDay"].Value;
                    string loggedTimeHour = match.Groups[@"LoggedTimeHour"].Value;
                    string loggedTimeMinute = match.Groups[@"LoggedTimeMinute"].Value;
                    string loggedTimeSecond = match.Groups[@"LoggedTimeSecond"].Value.Split('.')[0];

                    if (messageType == "MSG")
                    {
                        Aircraft aircraft = new Aircraft();
                        aircraft.ModeSCode = modeSCode;

                        string callsign = match.Groups[@"Callsign"].Value.Trim();
                        string altitude = match.Groups[@"Altitude"].Value;
                        string groundSpeed = match.Groups[@"GroundSpeed"].Value;
                        string track = match.Groups[@"Track"].Value;
                        string latitude = match.Groups[@"Latitude"].Value;
                        string longitude = match.Groups[@"Longitude"].Value;
                        string verticalRate = match.Groups[@"VerticalRate"].Value;
                        string squawk = match.Groups[@"squawk"].Value;
                        //bool squawkChange = match.Groups[@"SquawkFlag"].Value == "1";
                        bool emergency = match.Groups[@"EmergencyFlag"].Value == "1";
                        //bool ident = match.Groups[@"IdentFlag"].Value == "1";
                        bool isOnGround = match.Groups[@"IsOnGroundFlag"].Value == "1";

                        if (_aircraft.ContainsKey(modeSCode))
                        {
                            aircraft = _aircraft[modeSCode];
                        }
                        else
                        {
                            Debug.WriteLine("New Station: {0}", modeSCode);
                        }

                        if (!string.IsNullOrEmpty(callsign))
                        {
                            aircraft.Callsign = callsign;
                        }
                        if (!string.IsNullOrEmpty(altitude))
                        {
                            aircraft.Altitude = altitude;
                        }
                        if (!string.IsNullOrEmpty(groundSpeed))
                        {
                            aircraft.GroundSpeed = groundSpeed;
                        }
                        if (!string.IsNullOrEmpty(track))
                        {
                            aircraft.Track = track;
                        }
                        if (!string.IsNullOrEmpty(latitude))
                        {
                            aircraft.Latitude = latitude;
                        }
                        if (!string.IsNullOrEmpty(longitude))
                        {
                            aircraft.Longitude = longitude;
                        }
                        if (!string.IsNullOrEmpty(verticalRate))
                        {
                            aircraft.VerticalRate = verticalRate;
                        }
                        if (!string.IsNullOrEmpty(squawk))
                        {
                            aircraft.Squawk = squawk;
                        }
                        if (!string.IsNullOrEmpty(match.Groups[@"IsOnGroundFlag"].Value))
                        {
                            aircraft.IsOnGround = isOnGround;
                        }
                        if (!string.IsNullOrEmpty(match.Groups[@"EmergencyFlag"].Value))
                        {
                            aircraft.Emergency = emergency;
                        }

                        aircraft.LastMessageTime = new DateTime(
                            int.Parse(generatedDateYear),
                            int.Parse(generatedDateMonth),
                            int.Parse(generatedDateDay),
                            int.Parse(generatedTimeHour),
                            int.Parse(generatedTimeMinute),
                            int.Parse(generatedTimeSecond)
                        );

                        _aircraft[modeSCode] = aircraft;

                        ThreadPool.QueueUserWorkItem((a) => { OnAircraftDataReceived?.Invoke((Aircraft)a); }, aircraft);
                    }
                }

                PruneAircraft();

                nullBuffer();
            }

            void nullBuffer()
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }
            }
        }

        private void ClientConnect(int port)
        {
            do
            {
                try
                {
                    client.Connect(@"localhost", port);
                }
                catch (Exception) { }

                if (!client.Connected)
                {
                    Thread.Sleep(1000);
                }
            } while (!client.Connected);
            
            _connected = true;
        }

        private TcpClient client;

        private Thread connectionThread;

        private ConcurrentDictionary<string, Aircraft> _aircraft = new ConcurrentDictionary<string, Aircraft>();
        
        private bool _connected;
    }
}
