using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EmoteEvents.ComplexData;
using FlyMasterSerial;
using FlyMasterSerial.Data;
using FlyMasterSerial.Helper;
using FlyMasterSyncGui.Annotations;

namespace FlyMasterSyncGui.Database
{

    public class AlreadyDefinedPlaceException : Exception
    {
        public AlreadyDefinedPlaceException(string s)
            : base(s)
        {
        }
    }

    public class PlacesDB : INotifyPropertyChanged
    {
        private readonly string _dbPath;

        private ObservableCollection<PlacesDbEntry> _entries;
        private static PlacesDB _instance;

        public ObservableCollection<PlacesDbEntry> Entries
        {
            get { return _entries; }
            set
            {
                _entries = value; 
                OnPropertyChanged("Entries");
            }
        }

        public static PlacesDB GetInstance()
        {
            return _instance ?? (_instance = new PlacesDB());
        }

        public PlacesDB()
        {
            _dbPath = Properties.Settings.Default.PlacesDbFilePath;
            _entries = new ObservableCollection<PlacesDbEntry>();

            Load();
        }

        public void Add(string name, FlightLogPoint point)
        {
            // Checks if there is another place with the same name. If there is one, than it adds the point to that place list
            var place = new List<PlacesDbEntry>(_entries).Find(x => x.Name.ToLower() == name.ToLower());
            if (place == null)
            {
                var existingPlace = FindPlace(point);
                if (existingPlace != null)
                {
                    throw new AlreadyDefinedPlaceException("The place "+name+" at coordinates "+point.Latitude+","+point.Longitude+" is already defined with name: "+existingPlace.Name);
                }
                else
                {
                    _entries.Add(new PlacesDbEntry()
                    {
                        Name = name,
                        Points = new ObservableCollection<FlightLogPoint>() {point}
                    });
                }
            }
            else
            {
                if (!place.Points.Contains(point))
                {
                    place.Points.Add(point);
                }
            }
            Save();

            TracksDb.GetInstance().UpdatePlacesName();
        }

        public void Remove(PlacesDbEntry place)
        {
            _entries.Remove(place);
            Save();
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
                        _entries.Add(JsonSerializable.DeserializeFromJson<PlacesDbEntry>(line));
                        line = file.ReadLine();
                    }
                }
            }
        }

        public PlacesDbEntry FindPlace(FlightLogPoint point)
        {
            Location unknownLocation = new Location()
            {
                Latitude = point.LatToDecimal(),
                Longitude = point.LonToDecimal()
            };
            foreach (var placesDbEntry in _entries)
            {
                foreach (var flightLogPoint in placesDbEntry.Points)
                {
                    Location knownLocation = new Location()
                    {
                        Latitude = flightLogPoint.LatToDecimal(),
                        Longitude = flightLogPoint.LonToDecimal()
                    };

                    if (TrackingHelper.Distance(unknownLocation.Latitude,unknownLocation.Longitude, knownLocation.Latitude,knownLocation.Longitude) <= 2)
                    {
                        return placesDbEntry;
                    }
                }
            }
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PlacesDbEntry : JsonSerializable, INotifyPropertyChanged
    {
        private string _name;
        private ObservableCollection<FlightLogPoint> _points = new ObservableCollection<FlightLogPoint>();

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public ObservableCollection<FlightLogPoint> Points
        {
            get { return _points; }
            set
            {
                _points = value; 
                OnPropertyChanged("Points");
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