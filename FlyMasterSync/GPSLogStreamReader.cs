using FlyMasterSerial.Data;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace FlyMasterSerial
{
    public class GPSLogStreamReader
    {
        static string ASK_FLIGHT_POINTS = "$PFMDNL,<param1>,*1D";
        static private readonly byte[] bytesSEND = new byte[1] { 0xB1 };

        static StringBuilder _receiveBuffer = new StringBuilder();
        static string _flightdata;
        static int _errorsCount = 0;

        static private string _id = "";
        static private List<FlightLogPoint> _points;

        static bool reading = false;
        static bool Reading
        {
            get { return reading; }
            set
            {
                reading = value;
                Console.WriteLine("Reading = "+value);
            }
        }
        static private System.Timers.Timer _timeoutTimer = new System.Timers.Timer(2000);
        static private int trackcount = 0;

        public static async Task<List<FlightLogPoint>> AskAsync(SerialPort comport, string id)
        {
            if (Reading) throw new Exception("Already reading _points!");

            _receiveBuffer = new StringBuilder();
            _flightdata = "";
            _errorsCount = 0;
            _id = "";
            _points = new List<FlightLogPoint>();

            string query = ASK_FLIGHT_POINTS.Replace("<param1>", id);
            comport.DataReceived -= comport_DataReceived;
            comport.DataReceived += comport_DataReceived;
            comport.WriteLine(query);

            Reading = true;
            while (Reading)
            {
                await Task.Delay(100);
            }

            return _points;
        }

        static void comport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Console.WriteLine("Datareceived "+e.EventType+" - "+DateTime.Now.TimeOfDay);
            SerialPort sp = (SerialPort)sender;
            while (sp.BytesToRead > 0)
            {
                int bytes = sp.BytesToRead;
                byte[] buffer = new byte[bytes];
                sp.Read(buffer, 0, bytes);
                sp.Write(bytesSEND, 0, 1);

                Read(buffer);

                _timeoutTimer.Enabled = false;
                _timeoutTimer.Stop();
                _timeoutTimer.Enabled = true;
                _timeoutTimer.Start();
                _timeoutTimer.Elapsed -= _timeoutTimer_Elapsed;
                _timeoutTimer.Elapsed += _timeoutTimer_Elapsed;
            }
        }

        static void _timeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Reading = false;
            _timeoutTimer.Enabled = false;
#if DEBUG
            Console.WriteLine("Receiving track log timeout.. ending.");
#endif
        }

        static void Read(byte[] buffer)
        {
            //_points.Clear();
            string buff = ByteArrayToHexString(buffer).ToString();
            
            //Console.WriteLine("Buffer: "+buff);
            
            _receiveBuffer.Append(buff);
            _flightdata = _receiveBuffer.ToString();
            if (_flightdata.IndexOf("A3A3") != -1 && _flightdata.IndexOf("A2A2") != -1 && _flightdata.IndexOf("A1A1") != -1)
            {
#if DEBUG
                //Console.WriteLine("Started");
#endif
                int Latitude = 0;
                int Longitude = 0;
                int Altitude = 0;
                int Presure = 0;
                int Gpstime = 0;
                int PresureAlt = 0;
                string strLatitude = "";
                string strLongitude = "";

                _flightdata = _flightdata.Remove(_flightdata.Length - (_flightdata.Length - _flightdata.IndexOf("A3A3")), (_flightdata.Length - _flightdata.IndexOf("A3A3")));
                Regex regex = new Regex(@"A1A1");
                string[] substrings = regex.Split(_flightdata);
                for (int ctr0 = 1; ctr0 < substrings.Length; ctr0++)
                {
#if DEBUG
                    //Console.WriteLine("livello 1: " + ctr0 + "/" + substrings.Length);
#endif


                    Latitude = Convert.ToInt32((substrings[ctr0].Substring(10, 2) + substrings[ctr0].Substring(8, 2) + substrings[ctr0].Substring(6, 2) + substrings[ctr0].Substring(4, 2)), 16);
                    Longitude = Convert.ToInt32((substrings[ctr0].Substring(18, 2) + substrings[ctr0].Substring(16, 2) + substrings[ctr0].Substring(14, 2) + substrings[ctr0].Substring(12, 2)), 16);
                    Altitude = Convert.ToInt32((substrings[ctr0].Substring(22, 2) + substrings[ctr0].Substring(20, 2)), 16);
                    Presure = Convert.ToInt32((substrings[ctr0].Substring(26, 2) + substrings[ctr0].Substring(24, 2)), 16);
                    Gpstime = Convert.ToInt32((substrings[ctr0].Substring(34, 2) + substrings[ctr0].Substring(32, 2) + substrings[ctr0].Substring(30, 2) + substrings[ctr0].Substring(28, 2)), 16);
                    PresureAlt = Convert.ToInt32((double)(1 - (double)Math.Pow(Math.Abs((Presure / 10.0) / 1013.25), 0.190284)) * 44307.69);
                    strLatitude = ConvertDecimalToDegMinSecLat(Latitude);
                    strLongitude = ConvertDecimalToDegMinSecLong(Longitude);

                    AddPoint(strLatitude, strLongitude, Altitude, PresureAlt, Gpstime, Presure);
                    //Console.WriteLine("Received point = "+p);


                    trackcount++;
#if DEBUG
                    //Console.WriteLine(trackcount + ">> " + flightview);
#endif

                    Regex regexlog = new Regex(@"A2A2");
                    string[] substringslog = regexlog.Split(substrings[ctr0]);

                    Helper.Location lastLocation = null;
                    for (int ctr1 = 1; ctr1 < substringslog.Length; ctr1++)
                    {
                        
#if DEBUG
                        //Console.WriteLine("        livello 2: " + ctr1 + "/" + substringslog.Length);
#endif
                        int shift = (substringslog[ctr1].StartsWith("A2") ? 2 : 0);
                        int loglat = 4 + shift;
                        int loglong = 6 + shift;
                        int logalt = 8 + shift;
                        int presure = 10 + shift;
                        int logtime = 12 + shift;
                        int i = 1;
                        for (i = 1; i <= (substringslog[ctr1].Length - 4) / 12; i++)
                        {
                            int tmpLatitude = Latitude + HexToInt(substringslog[ctr1].Substring(loglat, 2));
                            int tmpLongitude = Longitude + HexToInt(substringslog[ctr1].Substring(loglong, 2));
                            int tmpAltitude = Altitude + HexToInt(substringslog[ctr1].Substring(logalt, 2));
                            int tmpPresure = Presure + HexToInt(substringslog[ctr1].Substring(presure, 2));
                            int tmpGpstime = Gpstime + HexToInt(substringslog[ctr1].Substring(logtime, 2));
                            PresureAlt = Convert.ToInt32((double)(1 - (double)Math.Pow(Math.Abs((tmpPresure / 10.0) / 1013.25), 0.190284)) * 44307.69);
                            strLatitude = ConvertDecimalToDegMinSecLat(tmpLatitude);
                            strLongitude = ConvertDecimalToDegMinSecLong(tmpLongitude);

                            
                            

                            if (lastLocation == null) lastLocation = new Helper.Location { Longitude = (double)tmpLongitude / 60000, Latitude = (double)tmpLatitude / 60000 };
                            Helper.Location thisLocation = new Helper.Location { Longitude = (double)tmpLongitude / 60000, Latitude = (double)tmpLatitude / 60000 };
                            var distance = Helper.TrackingHelper.CalculateDistance(lastLocation, thisLocation) * 1000;

                            if (distance > 100)
                                throw new Exception("Speed > 100m/s  - ignoring point");
                            else
                            {
#if DEBUG
                                //Console.WriteLine("             livello 3: " + i + "/" + (substringslog[ctr1].Length - 4) / 12 + "   - " + point.Time.ToString("HH:mm:ss") + "   - " + distance);
#endif
                                AddPoint(strLatitude, strLongitude, Altitude, PresureAlt, Gpstime, Presure);
                                //Console.WriteLine("Received point = " + p);
                                lastLocation = thisLocation;

                                Latitude = tmpLatitude;
                                Longitude = tmpLongitude;
                                Altitude = tmpAltitude;
                                Presure = tmpPresure;
                                Gpstime = tmpGpstime;
                            }


                            loglat = loglat + 12;
                            loglong = loglong + 12;
                            logalt = logalt + 12;
                            presure = presure + 12;
                            logtime = logtime + 12;
                            trackcount++;

#if DEBUG
                            //Console.WriteLine(trackcount + ">> " + flightview);
#endif
                        }
                        //Console.ReadLine();

                    }
                }
                
            }
        }

        private static void AddPoint(string strLatitude, string strLongitude, int Altitude, int PresureAlt, int Gpstime,
            int Presure)
        {
            var p = new FlightLogPoint()
            {
                Latitude = strLatitude,
                Longitude = strLongitude,
                GPSAltitude = Altitude,
                BaroAltitude = PresureAlt,
                Time = get_gps_date(Gpstime),
                Presure = Presure,
            };
            if (_points.Any() && p.Time.Ticks<=_points.Last().Time.Ticks) return;   // Ignore repeated points (there's a bug that causes the points to be transmitted twice)
            _points.Add(p);
        }

        static private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
            {
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
                //sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));      
            }
            return sb.ToString().ToUpper();
        }


        static private DateTime get_gps_date(double Gpstime)
        {
            TimeSpan TS = TimeSpan.FromSeconds(Gpstime);
            DateTime date = new DateTime(2000, 1, 1).Add(TS);
            return date;
        }

        static private string get_gps_time(double Gpstime)
        {
            TimeSpan TS = TimeSpan.FromSeconds(Gpstime);
            return TS.Hours.ToString("00", CultureInfo.InvariantCulture) +
                   TS.Minutes.ToString("00", CultureInfo.InvariantCulture) +
                   TS.Seconds.ToString("00", CultureInfo.InvariantCulture);
        }

        static private string ConvertDecimalToDegMinSecLong(double value)
        {
            double dLat = (double)value / (double)60000;
            int deg = (int)dLat;
            dLat = Math.Abs(dLat - deg);
            double min = (double)(dLat * 60);
            if (value < 0)
            {
                return (-1 * deg).ToString("000", CultureInfo.InvariantCulture) +
                    min.ToString("00.000E", CultureInfo.InvariantCulture).Substring(0, 2) +
                    min.ToString("00.000E", CultureInfo.InvariantCulture).Substring(3, 4);
            }
            else
            {
                return (deg).ToString("000", CultureInfo.InvariantCulture) +
                    min.ToString("00.000W", CultureInfo.InvariantCulture).Substring(0, 2) +
                    min.ToString("00.000W", CultureInfo.InvariantCulture).Substring(3, 4);
            }
        }
        static private string ConvertDecimalToDegMinSecLat(double value)
        {
            double dLat = (double)value / (double)60000;
            int deg = (int)dLat;
            dLat = Math.Abs(dLat - deg);
            double min = (double)(dLat * 60);
            if (value < 0)
            {
                return (-1 * deg).ToString("00", CultureInfo.InvariantCulture) +
                    min.ToString("00.000S", CultureInfo.InvariantCulture).Substring(0, 2) +
                    min.ToString("00.000S", CultureInfo.InvariantCulture).Substring(3, 4);
            }
            else
            {
                return deg.ToString("00", CultureInfo.InvariantCulture) +
                    min.ToString("00.000N", CultureInfo.InvariantCulture).Substring(0, 2) +
                    min.ToString("00.000N", CultureInfo.InvariantCulture).Substring(3, 4);
            }

        }

        static private int HexToInt(string hex)
        {
            int myInt = Convert.ToInt32(hex, 16);
            if (hex.StartsWith("F") || hex.StartsWith("E") || hex.StartsWith("D") || hex.StartsWith("C") || hex.StartsWith("B") || hex.StartsWith("A"))
                return myInt - 256;
            else
                return myInt;

        }

        


    }
}
