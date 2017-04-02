using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using FlyMasterSerial.Data;
using FlyMasterSerial.Exceptions;
using FlyMasterSyncGui.Annotations;
using FlyMasterSyncGui.Database;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace FlyMasterSyncGui.Forms
{
    public class FlymasterSyncViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FlightInfo> flightList = new ObservableCollection<FlightInfo>(){ new FlightInfo(){ Date = DateTime.Now, Duration = new TimeSpan(5000), ID="aslkdj"}};
        public ObservableCollection<FlightInfo> FlightList
        {
            get
            {
                return flightList;
            }
            set
            {
                flightList = value;
                OnPropertyChanged("FlightList");
            }
        }
        private string infoMessage = "Trying connecting to flymaster...";
        public string InfoMessage
        {
            get
            {
                return infoMessage;
            }
            set
            {
                infoMessage = value;
                OnPropertyChanged("InfoMessage");
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
    /// Interaction logic for FlymasterSync.xaml
    /// </summary>
    public partial class FlymasterSync : Window
    {
        FlymasterController _flymaster;
        FlymasterSyncViewModel data;
        private TracksDb _db;

        private static FlymasterSync _instance;
        private static bool _visible;

        public new static FlymasterSync Show()
        {
            if (_instance == null) _instance = new FlymasterSync();
            try
            {
                ((Window)_instance).Show();
            }
            catch (InvalidOperationException ex)
            {
                _instance = new FlymasterSync();
                ((Window) _instance).Show();
            }

            _visible = true;
            return _instance;
        }

        public new static FlymasterSync Hide()
        {
            if (_instance != null) ((Window)_instance).Hide();
            _visible = false;
            return _instance;
        }

        public FlymasterSync()
        {
            InitializeComponent();
            //this.Visibility = System.Windows.Visibility.Hidden;
            _db = TracksDb.GetInstance();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            data = this.DataContext as FlymasterSyncViewModel;
            _flymaster = new FlymasterController();
            data.FlightList.Clear();
            WaitForDeviceAndInitialize();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
        }

        private async void WaitForDeviceAndInitialize()
        {
            while (true)
            {
                data.InfoMessage = "Searching for a connected flymaster...";
                if (await _flymaster.Connect())
                {
                    break;
                }
                data.InfoMessage = "Searching for a connected flymaster... Not found (retrying in 3)";
                await Task.Delay(1000);
                data.InfoMessage = "Searching for a connected flymaster... Not found (retrying in 2)";
                await Task.Delay(1000);
                data.InfoMessage = "Searching for a connected flymaster... Not found (retrying in 1)";
                await Task.Delay(1000);
            }
            try
            {
                DeviceInfo devInfo = await _flymaster.GetDeviceInfo();
                string connectionInfo = string.Format(@"Connected to {0} (v.{1})", devInfo.Name, devInfo.Version);
                data.InfoMessage = connectionInfo + " | Loading flight list...";
                data.FlightList = await _flymaster.GetFlightList();
                data.InfoMessage = connectionInfo + " | Loaded " + data.FlightList.Count + " flights.";
                foreach (var flightInfo in data.FlightList)
                {
                    if (_db.Exists(flightInfo.ID))
                    {
                        flightInfo.Synced = true;
                    }
                }
            }
            catch (NotConnectedException ex)
            {
                WaitForDeviceAndInitialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unmanaged error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private async void CmdSyncButton_Click(object sender, RoutedEventArgs e)
        {
            CmdSyncButton.IsEnabled = false;
            string oldMessage = data.InfoMessage;
            int i = 1;
            foreach (var flightInfo in data.FlightList)
            {
                if (!_visible) return;
                data.InfoMessage = "Syncing... "+i++ +"/"+data.FlightList.Count;
                if (!_db.Exists(flightInfo.ID))
                {
                    var points = await _flymaster.GetFlightTrack(flightInfo.ID);
                    if (points.Count > 0)
                    {
                        _db.Add(flightInfo, points);
                        flightInfo.Synced = true;
                    }
                    else
                    {
                        // SOMETHING WRONG HAPPENED
                        Console.Error.WriteLine("Downloaded track with 0 points! Something wrong here!");
                    }
                }
            }
            data.InfoMessage = oldMessage;
            CmdSyncButton.IsEnabled = true;
        }

        private void FlightLogButton_Click(object sender, RoutedEventArgs e)
        {
            Forms.FlightLog fl = new Forms.FlightLog();
            fl.Show();
        }


        private void FlymasterSync_OnClosing(object sender, CancelEventArgs e)
        {
            _flymaster.Dispose();
            _visible = false;
        }
    }
}
