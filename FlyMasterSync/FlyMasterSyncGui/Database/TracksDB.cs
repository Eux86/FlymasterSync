using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using EmoteEvents.ComplexData;
using FlyMasterSerial;
using FlyMasterSerial.Data;
using FlyMasterSyncGui.Annotations;

namespace FlyMasterSyncGui.Database
{
    public class TracksDb : INotifyPropertyChanged
    {
        private string _tracksPath;
        private string _dbPath;

        public List<string> Tracks { get; set; }
        private ObservableCollection<FlightLogDbEntry> _entries;
        private static TracksDb _instance;

        public ObservableCollection<FlightLogDbEntry> Entries
        {
            get { return _entries; }
            set
            {
                _entries = value;
                OnPropertyChanged("Entries");
            }
        }

        public static TracksDb GetInstance()
        {
            return _instance ?? (_instance = new TracksDb());
        }

        public TracksDb()
        {
            _tracksPath = Properties.Settings.Default.TracksFolder;
            _dbPath = Properties.Settings.Default.TrackDbFilePath;

            // LOAD THE ENTRIES FROM THE DB FILE!

            _entries = new ObservableCollection<FlightLogDbEntry>();
            Load();
        }

        public void Add(FlightInfo flight, List<FlightLogPoint> points)
        {
            string flightId = flight.ID;
            string trackFilePath = _tracksPath + "\\" + flightId + ".igc";
            IGCMaker.Make(points, trackFilePath);

            var place = PlacesDB.GetInstance().FindPlace(points.First());
            string takeOffName = place != null ? place.Name : "";

            _entries.Add(new FlightLogDbEntry(){FlightInfo = flight, TrackFilePath = trackFilePath, TakeOffPoint = points.First(), TakeOffName = takeOffName});

            Entries = new ObservableCollection<FlightLogDbEntry>(_entries.OrderByDescending(x=>x.FlightInfo.Date));

            Save();
        }
        
        public void RemoveTrackFile(string id)
        {
            string trackFilePath = _tracksPath + "\\" + id + ".igc";
            if (Exists(id))
            {
                File.Delete(trackFilePath);
            }
        }

        public void Remove(FlightLogDbEntry entry)
        {
            if (_entries.Contains(entry))
                _entries.Remove(entry);
            RemoveTrackFile(entry.FlightInfo.ID);
            Save();
        }

        public bool Exists(string id)
        {
            string trackFilePath = _tracksPath + "\\" + id + ".igc";
            if (!Directory.Exists(_tracksPath))
            {
                Directory.CreateDirectory(_tracksPath);
            }
            return File.Exists(trackFilePath);
        }

        public void Save()
        {
            using (StreamWriter file = new StreamWriter(_dbPath))
            {
                foreach (var entry in _entries)
                {
                    file.WriteLine(entry.SerializeToJson());
                }
            }
        }

        private void Load()
        {
            if (File.Exists(_dbPath))
            {
                using (TextReader file = new StreamReader(_dbPath))
                {
                    string line = file.ReadLine();
                    while (line != null)
                    {
                        _entries.Add(JsonSerializable.DeserializeFromJson<FlightLogDbEntry>(line));
                        line = file.ReadLine();
                    }
                }
            }
        }

        public void UpdatePlacesName()
        {
            foreach (var flightLogDbEntry in _entries)
            {
                var place = PlacesDB.GetInstance().FindPlace(flightLogDbEntry.TakeOffPoint);
                if (place!=null)
                    flightLogDbEntry.TakeOffName = place.Name;
            }
            Save();
        }

        public bool Import(string igcFileName)
        {
            var points = IGCMaker.Load(igcFileName);
            FlightInfo flightInfo = new FlightInfo();
            flightInfo.Date = points.First().Time;
            flightInfo.Duration = points.Last().Time.Subtract(points.First().Time);
            flightInfo.GenerateId(flightInfo.Date);
            if (!Exists(flightInfo.ID))
            {
                string trackPath = Properties.Settings.Default.TracksFolder + "\\" + flightInfo.ID + ".igc";
                File.Copy(igcFileName, trackPath);
                Add(flightInfo, points);
                return true;
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FlightLogDbEntry : JsonSerializable, INotifyPropertyChanged
    {
        private FlightInfo _flightFlightInfo;
        private string _trackFilePath;
        private FlightLogPoint _takeOffPoint;
        private string _takeOffName;
        private string _doaramaVisualizationId;
        private string _comments;

        public FlightInfo FlightInfo
        {
            get { return _flightFlightInfo; }
            set
            {
                _flightFlightInfo = value;
                OnPropertyChanged("FlightInfo");
            }
        }

        public string TrackFilePath
        {
            get { return _trackFilePath; }
            set
            {
                _trackFilePath = value;
                OnPropertyChanged("TrackFilePath");
            }
        }

        public FlightLogPoint TakeOffPoint
        {
            get { return _takeOffPoint; }
            set
            {
                _takeOffPoint = value;
                OnPropertyChanged("TakeOffPoint");
            }
        }

        public string TakeOffName
        {
            get { return _takeOffName; }
            set
            {
                _takeOffName = value;
                OnPropertyChanged("TakeOffName");
            }
        }

        public string DoaramaVisualizationId
        {
            get { return _doaramaVisualizationId; }
            set
            {
                _doaramaVisualizationId = value; 
                OnPropertyChanged("DoaramaVisualizationId");
            }
        }

        public string Comments
        {
            get { return _comments; }
            set
            {
                _comments = value;
                OnPropertyChanged("Comments");
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
}