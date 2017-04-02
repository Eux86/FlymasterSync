using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FlyMasterSerial.Data;
using FlyMasterSyncGui.Database;
using FmsDb;
using FmsDb.Entities;
using FmsDb.Query;
using FlyMasterSyncGui.Database;

namespace DbTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            //var con = new SQLiteConnection(@"Data Source='C:\Users\Eugenio\Desktop\Databases voli\Logfly.db';Version=3;");
            //con.Open();
            //string sql = "SELECT * FROM Vol";
            //SQLiteCommand command = new SQLiteCommand(sql, con);
            //SQLiteDataReader reader = command.ExecuteReader();
            //while (reader.Read())
            //{
                
            //    FlightInfo fi = new FlightInfo();
            //    fi.Date = DateTime.Parse(reader["V_Date"].ToString());
            //    fi.Duration = new TimeSpan(0,0,0,(int)int.Parse(reader["V_Duree"].ToString()));
            //    Console.WriteLine(fi);
            //}
            //con.Close();


            Create();

            FlyMasterSyncGui.Database.PlacesDB places = PlacesDB.GetInstance();
            foreach (var place in places.Entries)
            {
                new FmsDb.Query.AddPlace().Execute(new AddPlace.AddPlaceParameters()
                {
                    Name = place.Name,
                    Longitude = place.Points[0].Longitude,
                    Latitude = place.Points[0].Latitude,
                    Altitude = place.Points[0].BaroAltitude
                });
            }

            var places2 = new FmsDb.Query.GetAllPlaces().Execute();

            FlyMasterSyncGui.Database.TracksDb tracks = TracksDb.GetInstance();
            int i = 0;
            foreach (var entry in tracks.Entries)
            {
                long takeOffIndex = 0;
                foreach (var place in places.Entries)
                {
                    if (place.Name == entry.TakeOffName)
                    {
                        break;
                    }
                    takeOffIndex++;
                }

                new FmsDb.Query.AddFlight().Execute(new Flight()
                {
                    Id = entry.FlightInfo.ID,
                    Comments = entry.Comments,
                    Date = entry.FlightInfo.Date,
                    Duration = entry.FlightInfo.Duration,
                    Takeoff = new Place() { Id = takeOffIndex }
                });
            }

            var res = new FmsDb.Query.GetAllFlights().Execute();
            foreach (var flight in res)
            {
                Console.WriteLine(flight);
            }
            Console.ReadLine();

            
        }

        static void Create()
        {
            string filename = "fmsync.db";
            if (!File.Exists(filename))
                SQLiteConnection.CreateFile(filename);
            var connection = new SQLiteConnection("Data Source='"+filename+"';Version=3;");
            connection.Open();
            string sql = File.ReadAllText("CreateDB.sqlite");
            var cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();
            connection.Close();

            new FmsDb.Query.AddPlace().Execute(new AddPlace.AddPlaceParameters() { Name = "Fonte Da Teglia", Altitude = 12 });
            new FmsDb.Query.AddFlight().Execute(new Flight(){Id = "321sda", Comments = "asdasd", Date = DateTime.Now, Duration = new TimeSpan(0,0,351), Takeoff = new Place() {Id = 1}});
        }

    }
}
