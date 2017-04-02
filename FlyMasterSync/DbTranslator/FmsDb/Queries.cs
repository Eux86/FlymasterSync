using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FmsDb.Entities;

namespace FmsDb.Query
{
    public class Query
    {
        private SQLiteConnection _connection;

        protected SQLiteConnection Open()
        {
            string filename = "fmsync.db";
            _connection = new SQLiteConnection("Data Source='" + filename + "';Version=3;");
            _connection.Open();
            return _connection;
        }

        protected void Close()
        {
            _connection.Close();
        }
    }

    public class GetAllFlights : Query
    {
        public List<Flight> Execute()
        {
            var connection = Open();
            string qry = "SELECT * FROM Flight LEFT JOIN Place ON Flight.TakeOffPlaceId = Place.Id";
            var cmd = new SQLiteCommand(qry, connection);
            var res = cmd.ExecuteReader();
            var flights = new List<Flight>();
            while (res.Read())
            {
                try
                {
                    var flight = new Flight();
                    flight.Id = res["Id"].ToString();
                    flight.Date = SQLUtils.UnixTimeStampToDateTime((long)res["Date"]);
                    flight.Duration = new TimeSpan(0, 0, 0, int.Parse(res["DurationSeconds"].ToString()));
                    flight.Takeoff = new GetPlaceWithId().Execute((long)res["TakeOffPlaceId"]);
                    flights.Add(flight);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    throw ex;
                }
            }
            Close();
            return flights;
        }
    }

    public class GetAllPlaces : Query
    {
        public List<Place> Execute()
        {
            var connection = Open();
            string qry = "SELECT * FROM Place";
            var cmd = new SQLiteCommand(qry, connection);
            var res = cmd.ExecuteReader();
            var places = new List<Place>();
            while (res.Read())
            {
                try
                {
                    var place = new Place();
                    place.Id = long.Parse(res["Id"].ToString());
                    place.Name = res["Name"].ToString();
                    place.Latitude = res["Latitude"].ToString();
                    place.Longitude = res["Longitude"].ToString();
                    place.AltitudeMeters = (long) res["AltitudeMeters"];
                    places.Add(place);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    throw ex;
                }
            }
            Close();
            return places;
        }
    }

    public class GetPlaceWithId : Query
    {
        public Place Execute(long id)
        {
            var connection = Open();
            string qry = string.Format("SELECT * FROM Place WHERE Id = {0}",id);
            var cmd = new SQLiteCommand(qry, connection);
            var res = cmd.ExecuteReader();
            var place = new Place();
            res.Read();
            place.Id = (long)res["Id"];
            place.Name = (string) res["Name"];
            place.Latitude = (string) res["Latitude"];
            place.Longitude = (string) res["Longitude"];
            place.AltitudeMeters = (long) res["AltitudeMeters"];
            Close();
            return place;
        }
    }

    public class AddPlace : Query
    {
        public class AddPlaceParameters
        {
            public string Name { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public long Altitude { get; set; }
        }
        public SQLiteDataReader Execute(AddPlaceParameters parameters)
        {
            var connection = Open();
            string qry = string.Format(@"INSERT INTO Place ('Name','Latitude','Longitude','AltitudeMeters') values ('{0}','{1}','{2}',{3})",
                                                parameters.Name, parameters.Latitude, parameters.Longitude, parameters.Altitude);
            var cmd = new SQLiteCommand(qry, connection);
            var res = cmd.ExecuteReader();
            Close();
            return res;
        }
    }

    public class AddFlight : Query
    {
        public SQLiteDataReader Execute(Flight flight)
        {
            var connection = Open();
            string qry =
                string.Format(
                    @"INSERT INTO Flight ('Id','Date','DurationSeconds','Comments','TakeOffPlaceId') values ('{0}',{1},'{2}','{3}',{4})",
                    flight.Id, SQLUtils.DateTimeToUnixTimeStamp(flight.Date), flight.Duration.TotalSeconds, flight.Comments, flight.Takeoff.Id);
            var cmd = new SQLiteCommand(qry, connection);
            var res = cmd.ExecuteReader();
            Close();
            return res;
        }
    }


    internal class SQLUtils
    {

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static long DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimestamp = (long)dateTime.Subtract(dtDateTime).TotalSeconds;
            return unixTimestamp;
        }

        public static string DateTimeSqLite(DateTime datetime)
        {
            string dateTimeFormat = "{0}-{1}-{2} {3}:{4}:{5}.{6}";
            return string.Format(dateTimeFormat, datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second, datetime.Millisecond);
        }
    }
}
