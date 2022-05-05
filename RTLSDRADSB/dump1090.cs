using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace RTLSDRADSB
{
    using DeviceListType = List<Tuple<int, string, string, bool>>;

    public class dump1090
    {
        private const string DUMP1090_FILENAME = @"dump1090\dump1090.exe";
        private const string DEBUGCATEGORYNAME = @"dump1090";

        public delegate void DeviceListReceivedEventHandler(object sender, DeviceListType deviceList);

        public DeviceListReceivedEventHandler DeviceListReceived;
        public EventHandler ProcessExited;

        public Process ApplicationProcess { get; set; }
        public DeviceListType DeviceList { get => _deviceList; }

        public dump1090(string args = @"")
        {
            try
            {
                ApplicationProcess = new Process();
                ApplicationProcess.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DUMP1090_FILENAME);
                ApplicationProcess.StartInfo.Arguments = args;
                ApplicationProcess.StartInfo.CreateNoWindow = true;
                ApplicationProcess.StartInfo.UseShellExecute = false;
                ApplicationProcess.StartInfo.RedirectStandardOutput = true;
                ApplicationProcess.StartInfo.RedirectStandardError = true;
                ApplicationProcess.EnableRaisingEvents = true;
                ApplicationProcess.OutputDataReceived += OnOutputReceived;
                ApplicationProcess.ErrorDataReceived += OnOutputReceived;
                ApplicationProcess.Exited += ApplicationProcessExited;

                Debug.WriteLine(@"FileName: " + ApplicationProcess.StartInfo.FileName, DEBUGCATEGORYNAME);
                killDump1090(); // TODO: It is possible to run dual-band using multiple d1090 instances. This should just be for dev purposes.

                Debug.WriteLine(@"Starting Process", DEBUGCATEGORYNAME);
                ApplicationProcess.Start();
                ApplicationProcess.BeginOutputReadLine();
                ApplicationProcess.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, DEBUGCATEGORYNAME);
            }
        }

        private static void killDump1090()
        {
            Process[] dump1090Processes = Process.GetProcessesByName(@"dump1090");
            foreach (Process dump1090Process in dump1090Processes)
            {
                Debug.WriteLine("Killing existing dump1090 process " + dump1090Process.Id, DEBUGCATEGORYNAME);
                dump1090Process.Kill();
            }
        }

        private void ApplicationProcessExited(object sender, EventArgs e)
        {
            Debug.WriteLine("Exited.", DEBUGCATEGORYNAME);
            if (ProcessExited != null)
            {
                ProcessExited(sender, e);
            }
        }

        ~dump1090()
        {
            ApplicationProcess?.Kill();
            ApplicationProcess?.Dispose();
        }

        private void OnOutputReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug.WriteLine(e.Data, DEBUGCATEGORYNAME);

                if (_foundDevices)
                {
                    Match mc = Regex.Match(e.Data, @"(\d): (.+), SN: (.?)\s?(\(currently selected\))?");
                    if (mc.Success)
                    {
                        if (mc.Groups.Count > 0)
                        {
                            string index = mc.Groups[1].Value;
                            string name = mc.Groups[2].Value;
                            string serial = mc.Groups[3].Value;
                            string selected = mc.Groups[4].Value;
                            int indexNum;
                            if (int.TryParse(index, out indexNum))
                            {
                                _deviceList.Add(new Tuple<int, string, string, bool>(indexNum, name, serial, !string.IsNullOrEmpty(selected)));
                            }
                            
                        }
                    }
                    else
                    { // Once the pattern is not matched, we've reached the line after the last device.
                        _foundDevices = false;
                        if (_deviceList.Count > 0)
                        {
                            DeviceListReceived?.Invoke(this, _deviceList);
                        }
                    }
                }

                if (Regex.Match(e.Data, @"Found \d device\(s\):").Success)
                {
                    _foundDevices = true;
                    _deviceList = new DeviceListType();
                }
            }
        }

        private bool _foundDevices = false;
        private DeviceListType _deviceList = new DeviceListType();
    }
}
