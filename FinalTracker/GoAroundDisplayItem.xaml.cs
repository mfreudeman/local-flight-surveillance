using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FinalTracker
{
    public partial class GoAroundDisplayItem : UserControl, INotifyPropertyChanged
    {
        public GoAroundDisplayItem()
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


        private string _callsign;
        private string _registration;
        private string _typeCode;
        private string _origin;
        private string _runway;
    }
}
