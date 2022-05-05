using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace FinalTracker
{
    public partial class OnFinalDisplayItem : UserControl, INotifyPropertyChanged
    {
        public OnFinalDisplayItem()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Callsign
        {
            get => _callsign;
            set
            {
                _callsign = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Callsign)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RegistrationCallsign)));
            }
        }
        public string Registration
        {
            get => _registration;
            set
            {
                _registration = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Registration)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RegistrationCallsign)));
            }
        }
        public string RegistrationCallsign
        {
            get => _callsign == _registration || _callsign == null
                ? _registration : string.Format(@"{0} ({1})", _callsign, _registration);
        }
        public string TypeCode
        {
            get => _typeCode;
            set
            {
                _typeCode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TypeCode)));
            }
        }
        public long SecondsOut
        {
            get => _secondsOut;
            set
            {
                _secondsOut = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecondsOut)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingDurationDistance)));
            }
        }
        public long DistanceOut
        {
            get => _distanceOut;
            set
            {
                _distanceOut = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DistanceOut)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DistanceOutString)));
            }
        }
        public string DistanceOutUnits
        {
            get => _distanceOutUnit;
            set
            {
                _distanceOutUnit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DistanceOutUnits)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DistanceOutString)));
            }
        }
        public long Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Duration)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingDurationDistance)));
            }
        }
        public long Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Distance)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingDurationDistance)));
            }
        }
        public string DistanceUnits
        {
            get => _distanceUnit;
            set
            {
                _distanceUnit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DistanceUnits)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingDurationDistance)));
            }
        }
        public string OriginCode
        {
            get => _origin;
            set
            {
                _origin = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OriginCode)));
            }
        }
        public string Runway
        {
            get => _runway;
            set
            {
                _runway = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Runway)));
            }
        }
        public string DistanceOutString
        {
            get => string.Format(@"{0}{1}", _distanceOut, _distanceOutUnit);
        }
        public string RemainingDurationDistance
        {
            get
            {
                TimeSpan outTime = new TimeSpan(_secondsOut * TimeSpan.TicksPerSecond);
                TimeSpan durationTime = new TimeSpan(_duration * TimeSpan.TicksPerSecond);
                return string.Format(
                        "{0} ({1}) {2}{3}",
                        string.Format("{0}:{0}", outTime.Minutes.ToString().PadLeft(2,'0'), outTime.Seconds.ToString().PadLeft(2, '0')),
                        string.Format("{0}:{0}", durationTime.Hours.ToString().PadLeft(2, '0'), outTime.Minutes.ToString().PadLeft(2, '0')),
                        _distance,
                        _distanceUnit);
            }
        }

        private string _callsign;
        private string _registration;
        private string _typeCode;
        private long _secondsOut;
        private long _duration;
        private long _distance;
        private string _distanceUnit;
        private long _distanceOut;
        private string _distanceOutUnit;
        private string _origin;
        private string _runway;
    }
}
