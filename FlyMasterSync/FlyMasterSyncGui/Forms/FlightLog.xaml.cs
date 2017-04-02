using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using DoaramaSharp;
using FlyMasterSerial;
using FlyMasterSerial.Data;
using FlyMasterSerial.Exceptions;
using FlyMasterSyncGui.Annotations;
using FlyMasterSyncGui.Database;
using Microsoft.Win32;

namespace FlyMasterSyncGui.Forms
{
    public class FlightLogVM : INotifyPropertyChanged
    {
        private TracksDb _db = new TracksDb() { Entries = new ObservableCollection<FlightLogDbEntry>()
        {
            new FlightLogDbEntry() { FlightInfo = new FlightInfo() { Date = DateTime.Now, Duration = new TimeSpan(0,36,5)}, TakeOffName = "TestPlace1", DoaramaVisualizationId = "k30XlaE"},
            new FlightLogDbEntry() { FlightInfo = new FlightInfo() { Date = DateTime.Now, Duration = new TimeSpan(1,36,5)}, TakeOffName = "TestPlace2"},
            new FlightLogDbEntry() { FlightInfo = new FlightInfo() { Date = DateTime.Now.AddMonths(-1), Duration = new TimeSpan(0,36,5)}, TakeOffName = "TestPlace3", DoaramaVisualizationId = "k30XlaE"},
            new FlightLogDbEntry() { FlightInfo = new FlightInfo() { Date = DateTime.Now.AddMonths(-1), Duration = new TimeSpan(1,36,5)}, TakeOffName = "TestPlace2"}
        }};
        private FlightLogDbEntry _selectedFlight;
        private ObservableCollection<string> _entries = new ObservableCollection<string>()
        {
            "one","two","tree"
        };
        private List<KeyValuePair<string, string>> _statistics = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("Max height", "3250m"),
            new KeyValuePair<string, string>("Max distance in a line", "1596m")
        };

        private bool _flymasterConnected;
        private DeviceInfo _deviceInfo;
        private ObservableCollection<FlightInfo> _onFlymasterFlightList = new ObservableCollection<FlightInfo>();
        private ObservableCollection<FlightInfo> _flightsToSync = new ObservableCollection<FlightInfo>();
        private bool _syncing;
        private float _syncingProgress;
        private bool _loadingFlymasterData;

        public TracksDb TracksDb
        {
            get { return _db; }
            set
            {
                _db = value;
                OnPropertyChanged("TracksDb");
            }
        }
        public FlightLogDbEntry SelectedFlight
        {
            get { return _selectedFlight; }
            set
            {
                _selectedFlight = value;
                OnPropertyChanged("SelectedFlight");
            }
        }
        public ObservableCollection<string> Entries
        {
            get { return _entries; }
            set
            {
                _entries = value;
                OnPropertyChanged("Entries");
            }
        }
        
        public List<KeyValuePair<string, string>> Statistics
        {
            get { return _statistics; }
            set
            {
                _statistics = value; 
                OnPropertyChanged("Statistics");
            }
        }

        public bool FlymasterConnected
        {
            get { return _flymasterConnected; }
            set
            {
                _flymasterConnected = value;
                OnPropertyChanged("FlymasterConnected");
            }
        }

        public DeviceInfo DeviceInfo
        {
            get { return _deviceInfo; }
            set
            {
                _deviceInfo = value; 
                OnPropertyChanged("DeviceInfo");
            }
        }

        public ObservableCollection<FlightInfo> OnFlymasterFlightList
        {
            get { return _onFlymasterFlightList; }
            set
            {
                _onFlymasterFlightList = value;
                OnPropertyChanged("OnFlymasterFlightList");
            }
        }

        public ObservableCollection<FlightInfo> FlightsToSync
        {
            get
            {
                return _flightsToSync;
            }
            set
            {
                _flightsToSync = value;
                OnPropertyChanged("FlightsToSync");
            }
        }

        public bool Syncing
        {
            get { return _syncing; }
            set
            {
                _syncing = value; 
                OnPropertyChanged("Syncing");
            }
        }

        public float SyncingProgress
        {
            get { return _syncingProgress; }
            set
            {
                _syncingProgress = value; 
                OnPropertyChanged("SyncingProgress");
            }
        }

        public bool LoadingFlymasterData
        {
            get { return _loadingFlymasterData; }
            set
            {
                _loadingFlymasterData = value; 
                OnPropertyChanged("LoadingFlymasterData");
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
    /// Interaction logic for FlightLog.xaml
    /// </summary>
    public partial class FlightLog : Window
    {
        private FlightLogVM _data;
        private FlymasterController _flymaster;

        public FlightLog()
        {
            InitializeComponent();
            _data = this.DataContext as FlightLogVM;

            this.Height = Properties.Settings.Default.FlightLogWindowHeigh;
            this.Width = Properties.Settings.Default.FlightLogWindowWidth;

            _data.TracksDb = TracksDb.GetInstance();

            CheckFlymasterConnected();
        }

        private async void CheckFlymasterConnected()
        {
            if (_data.FlightsToSync!=null)_data.FlightsToSync.Clear();
            if (_data.OnFlymasterFlightList!=null)_data.OnFlymasterFlightList.Clear();

            while (true)
            {
                Console.WriteLine("Searching for a flymaster..");
                if (_flymaster == null)
                {
                    _flymaster=new FlymasterController();
                    _flymaster.DisconnectedEvent -= _flymaster_DisconnectedEvent;
                    _flymaster.DisconnectedEvent += _flymaster_DisconnectedEvent;
                }
                _data.FlymasterConnected = await _flymaster.Connect();
                if (_data.FlymasterConnected)
                {
                    Console.WriteLine("Connected to Flymaster");
                    break;
                }
                Console.WriteLine("Flymaster not found");
                await Task.Delay(3000);
            }
            try
            {
                _data.LoadingFlymasterData = true;
                _data.DeviceInfo = await _flymaster.GetDeviceInfo();
                _data.OnFlymasterFlightList = await _flymaster.GetFlightList();
                foreach (var flightInfo in _data.OnFlymasterFlightList)
                {
                    if (_data.TracksDb.Entries.Any(x=>x.FlightInfo.ID == flightInfo.ID))
                    {
                        flightInfo.Synced = true;
                    }
                }
                _data.FlightsToSync = new ObservableCollection<FlightInfo>(_data.OnFlymasterFlightList.Where(x => !x.Synced));
                _data.LoadingFlymasterData = false;
            }
            catch (NotConnectedException ex)
            {
                CheckFlymasterConnected();
            }
        }

        void _flymaster_DisconnectedEvent()
        {
            Console.WriteLine("Flymaster Disconnected!");
            _data.FlymasterConnected = false;
            CheckFlymasterConnected();
        }

        private void FlightLog_OnLoaded(object sender, RoutedEventArgs e)
        {
            new UpdateManager();
        }

        private void FlightLog_OnClosing(object sender, CancelEventArgs e)
        {
            TracksDb.GetInstance().Save();
            PlacesDB.GetInstance().Save();

            Properties.Settings.Default.FlightLogWindowHeigh = this.Height;
            Properties.Settings.Default.FlightLogWindowWidth = this.Width;
            Properties.Settings.Default.Save();
        }

        
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            FlightLogDbEntry curItem = (FlightLogDbEntry) ((ListBoxItem)LogsListBox.ContainerFromElement((Button)sender)).Content;
            Process.Start(curItem.TrackFilePath);
        }

        private void ExMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SyncMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var window = FlymasterSync.Show();
            window.Owner = this;
        }

        private void FlightLog_OnClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AddTakeoffMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var entry = (FlightLogDbEntry) LogsListBox.SelectedItem;
            var point = entry.TakeOffPoint;
            var addPlaceWindow = new NewPlaceDialog(point,entry.TakeOffName) { Owner = this};
            addPlaceWindow.ShowDialog();
            //if (addPlaceWindow.HasReturnValue)
            //{
            //    entry.TakeOffName = addPlaceWindow.ReturnValue;
            //    _data.TracksDb.Save();
            //}
        }

        private void EditTakeoffsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var window = Places.Show();
            window.Owner = this;
        }

        
        private void RemoveSelectedFlightMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var returnValue =
            MessageBox.Show("Are you sure you want to remove the selected flight?\nThis cannot be undone",
                "Removing flight", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (returnValue == MessageBoxResult.Yes)
            {
                TracksDb.GetInstance().Remove(_data.SelectedFlight);
            }
        }

        private void ExportSelectedMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "igc";
            saveFileDialog.Filter = "IGC File | *.igc";
            saveFileDialog.FileName = _data.SelectedFlight.TakeOffName + _data.SelectedFlight.FlightInfo.ID;
            if (saveFileDialog.ShowDialog() == true)
                File.Copy(_data.SelectedFlight.TrackFilePath, saveFileDialog.FileName,true);
        }


        private void AboutMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            About.Show();
        }

        private void ImportMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "igc";
            openFileDialog.Filter = "IGC Track File | *.igc";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    if (!_data.TracksDb.Import(fileName))
                    {
                        MessageBox.Show("There is another flight for the same day at the same time!", "Flight already exist", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void SettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var window = SettingsWindow.Show();
            window.Owner = this;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            FlightLogDbEntry curItem = (FlightLogDbEntry)((ListBoxItem)LogsListBox.ContainerFromElement((Hyperlink)sender)).Content;
            Process.Start(new ProcessStartInfo(DoaramaVisualizationMaker.GetVisualizationUrl(curItem.DoaramaVisualizationId).ToString()));
        }

        private void UnlinkDoaramaButton_OnClick(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(this, "This will break the link to the current Doarama visualization.\n" +
                                  "The visualization WILL NOT be deleted from Doarama.\n" +
                                  "After this operation a new visualization can be uploaded to Doarama.\n" +
                                  "Do you want to continue?", "Unlinking doarama visualization", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (res == MessageBoxResult.Yes)
            {
                FlightLogDbEntry curItem =
                    (FlightLogDbEntry) ((ListBoxItem) LogsListBox.ContainerFromElement((Button) sender)).Content;
                curItem.DoaramaVisualizationId = null;
            }
        }

        private async void UploadHyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            hyperlink.IsEnabled = false;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.UserDoaramaApiKey))
            {
                FlightLogDbEntry curItem = (FlightLogDbEntry)((ListBoxItem)LogsListBox.ContainerFromElement((Hyperlink)sender)).Content;
                if (string.IsNullOrEmpty(curItem.DoaramaVisualizationId))
                {
                    string trackName = curItem.TakeOffName + " - " + curItem.FlightInfo.Date.ToString(@"dd/MM/yy");
                    curItem.DoaramaVisualizationId = await DoaramaVisualizationMaker.CreateVisualizationAsync(curItem.TrackFilePath, Properties.Settings.Default.UserDoaramaApiKey, trackName);
                }
                Process.Start(new ProcessStartInfo(DoaramaVisualizationMaker.GetVisualizationUrl(curItem.DoaramaVisualizationId).ToString()));
            }
            else
            {
                MessageBox.Show(this,
                    "Before being able to upload a track you have to setup your Doarama API key in the settings window",
                    "Doarama Api key not defined",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            hyperlink.IsEnabled = true;
        }

        private void FlightCommentsTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            _data.TracksDb.Save();
        }

        private async void SyncHyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            _data.Syncing = true;
            int i = 0;
            _data.SyncingProgress = 0;
            foreach (var flightInfo in _data.FlightsToSync)
            {
                var points = await _flymaster.GetFlightTrack(flightInfo.ID);
                if (points.Count > 0)
                {
                    _data.TracksDb.Add(flightInfo, points);
                    flightInfo.Synced = true;
                }
                else
                {
                    // SOMETHING WRONG HAPPENED
                    Console.Error.WriteLine("Downloaded track with 0 points! Something wrong here!");
                }
                i++;
                _data.SyncingProgress = ((float)i / _data.FlightsToSync.Count) * 100;
            }
            _data.Syncing = false;
            //_data.FlightsToSync.Clear();
        }
    }
}
