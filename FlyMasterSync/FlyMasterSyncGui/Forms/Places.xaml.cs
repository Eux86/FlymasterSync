using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using FlyMasterSerial.Data;
using FlyMasterSyncGui.Annotations;
using FlyMasterSyncGui.Database;

namespace FlyMasterSyncGui.Forms
{
    /// <summary>
    /// Interaction logic for Places.xaml
    /// </summary>
    public partial class Places : Window, INotifyPropertyChanged
    {
        private PlacesDbEntry _selectedPlace;
        private FlightLogPoint _selectedPoint;
        private PlacesDB _placesDb;

        public PlacesDbEntry SelectedPlace
        {
            get { return _selectedPlace; }
            set
            {
                _selectedPlace = value; 
                OnPropertyChanged("SelectedPlace");
            }
        }

        public PlacesDB PlacesDb
        {
            get { return _placesDb; }
            set
            {
                _placesDb = value;
                OnPropertyChanged("PlacesDb");
            }
        }

        public FlightLogPoint SelectedPoint
        {
            get { return _selectedPoint; }
            set
            {
                _selectedPoint = value;
                OnPropertyChanged("SelectedPoint");
            }
        }

        private static Places _instance;
        private static bool _visible;

        

        public new static Places Show()
        {
            if (_instance == null) _instance = new Places();
            try
            {
                ((Window)_instance).Show();
            }
            catch (InvalidOperationException ex)
            {
                _instance = new Places();
                ((Window)_instance).Show();
            }

            _visible = true;
            return _instance;
        }

        public new static Places Hide()
        {
            if (_instance != null) ((Window)_instance).Hide();
            _visible = false;
            return _instance;
        }

        
        public Places()
        {
            InitializeComponent();
            PlacesDb = PlacesDB.GetInstance();
            PointsListBox.Items.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DeletePlaceButton_Click(object sender, RoutedEventArgs e)
        {
            PlacesDb.Entries.Remove(SelectedPlace);
        }

        private void DeletePointButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPlace.Points.Remove(SelectedPoint);
        }
    }
}
