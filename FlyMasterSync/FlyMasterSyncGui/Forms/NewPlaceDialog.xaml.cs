using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FlyMasterSerial.Data;
using FlyMasterSyncGui.Annotations;
using FlyMasterSyncGui.Database;

namespace FlyMasterSyncGui.Forms
{
    class NewPlaceViewModel : INotifyPropertyChanged
    {
        private string _placeName = "NoName";
        private string _originalName = "NoName";
        private FlightLogPoint _point = new FlightLogPoint() { Latitude = "123321233W", Longitude = "87093821S"};
        private string _googleMapsLink = "http://www.google.com";
        public string GoogleMapsLink
        {
            get
            {
                if (Point != null)
                {
                    string lat = Point.LatToDecimal().ToString(CultureInfo.InvariantCulture);
                    string lon = Point.LonToDecimal().ToString(CultureInfo.InvariantCulture);
                    return string.Format(@"https://www.google.it/maps/@{0},{1},15z/data=!3m1!1e3", lat, lon);
                }
                else
                {
                    return "";
                }
            }
            set
            {
                _googleMapsLink = value;
                OnPropertyChanged("GoogleMapsLink");
            }
        }

        public string PlaceName
        {
            get { return _placeName; }
            set
            {
                _placeName = value;
                OnPropertyChanged("PlaceName");
            }
        }

        public FlightLogPoint Point
        {
            get { return _point; }
            set
            {
                _point = value;
                OnPropertyChanged("Point");
                OnPropertyChanged("GoogleMapsLink");
            }
        }

        public string OriginalName
        {
            get { return _originalName; }
            set
            {
                _originalName = value;
                OnPropertyChanged("OriginalName");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Interaction logic for NewPlaceDialog.xaml
    /// </summary>
    public partial class NewPlaceDialog : Window
    {
        private NewPlaceViewModel _data;
        

        private NewPlaceDialog()
        {
            InitializeComponent();
        }

        public NewPlaceDialog(FlightLogPoint point, string name = null)
        {
            InitializeComponent();
            _data = DataContext as NewPlaceViewModel;
            _data.Point = point;
            _data.PlaceName = name;
            _data.OriginalName = name;       // Storing the original name in case the user wants to rollback 
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void AddPlaceButton_Click(object sender, RoutedEventArgs e)
        {
            var placeDb = PlacesDB.GetInstance();
            try
            {
                placeDb.Add(_data.PlaceName, _data.Point);
            }
            catch (AlreadyDefinedPlaceException ex)
            {
                var result = MessageBox.Show(ex.Message+Environment.NewLine+"Do you want to replace the old name with the new one?", "Place already existing!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    placeDb.FindPlace(_data.Point).Name = _data.PlaceName;
                    TracksDb.GetInstance().UpdatePlacesName();
                    placeDb.Save();
                }
                else
                {
                    _data.PlaceName = _data.OriginalName;
                }
            }
            
            this.Close();
        }

        
    }
}
