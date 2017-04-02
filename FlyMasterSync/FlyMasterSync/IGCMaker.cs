using FlyMasterSerial.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlyMasterSerial
{
    public class IGCMaker
    {
        //igc = igc + "B" + get_gps_time(Gpstime) + strLatitude + strLongitude + "A" +
        //PresureAlt.ToString("00000", CultureInfo.InvariantCulture) +
        //Altitude.ToString("00000", CultureInfo.InvariantCulture) + "\r\n";
        public static void Make(List<FlightLogPoint> points, string path = "output.igc")
        {
            List<int> lastBaroAltValues = new List<int>();
            List<int> lastGPSAltValues = new List<int>();

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
                file.WriteLine("HFDTE"+points[0].Time.ToString("ddMMyy"));
                file.WriteLine("HODTM100GPSDATUM: WGS-84");
                foreach (FlightLogPoint point in points)
                {
                    string maskGPSAlt = "";
                    if (point.GPSAltitude < 0)
                        maskGPSAlt = "0000";
                    else
                        maskGPSAlt = "00000";
                    string maskBaroAlt = "";
                    if (point.BaroAltitude < 0)
                        maskBaroAlt = "0000";
                    else
                        maskBaroAlt = "00000";

                    int baroAlt = FilterOutlier(lastBaroAltValues, point.BaroAltitude,3);
                    int gpsAlt = FilterOutlier(lastGPSAltValues, point.GPSAltitude,3);

                    file.WriteLine("B" + point.Time.ToString("HHmmss") + point.Latitude + point.Longitude + "A" + baroAlt.ToString(maskBaroAlt, CultureInfo.InvariantCulture) + gpsAlt.ToString(maskGPSAlt, CultureInfo.InvariantCulture));

                    lastBaroAltValues.Insert(0,point.BaroAltitude);
                    lastGPSAltValues.Insert(0,point.GPSAltitude);
                }
            }  
        }

        private static int FilterOutlier(IEnumerable<int> values, int value, int lastValuesToConsiderNumber)
        {
            if (values.Count() < lastValuesToConsiderNumber) return value;

            double av = 0;
            for (int i = 0; i < lastValuesToConsiderNumber; i++)
            {
                av += values.ToList()[i];
            }
            av = av/lastValuesToConsiderNumber;

            double sum = 0;
            for (int i = 0; i < lastValuesToConsiderNumber; i++)
            {
                var val = values.ToList()[i];
                sum += Math.Pow((av - val), 2);
            }
            double sd = Math.Sqrt(sum/values.Count());

            if (value > av + sd || value < av - sd)
                return (int)av;
            else
                return value;
        }

        public static List<FlightLogPoint> Load(string path)
        {
            List<FlightLogPoint> points = new List<FlightLogPoint>();
            DateTime baseDate = new DateTime();
            using (System.IO.StreamReader file = new StreamReader(path))
            {
                var line = file.ReadLine();
                while (line != null)
                {
                    Regex regexDate = new Regex(@"HFDTE(\d\d)(\d\d)(\d\d)");
                    Match matchDate = regexDate.Match(line);
                    if (matchDate.Success)
                    {
                        baseDate = new DateTime(int.Parse(matchDate.Groups[3].Value),int.Parse(matchDate.Groups[2].Value), int.Parse(matchDate.Groups[1].Value));
                    }
                    Regex regexLine = new Regex(@"B(\d\d)(\d\d)(\d\d)(\d*.)(\d*.)A(\d{5})(\d{5})");
                    Match matchLine = regexLine.Match(line);
                    if (matchLine.Success)
                    {
                        FlightLogPoint point = new FlightLogPoint();
                        point.Time = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, 
                            int.Parse(matchLine.Groups[1].Value),int.Parse(matchLine.Groups[2].Value),int.Parse(matchLine.Groups[3].Value));
                        point.Latitude = matchLine.Groups[4].Value;
                        point.Longitude = matchLine.Groups[5].Value;
                        point.BaroAltitude = int.Parse(matchLine.Groups[6].Value);
                        point.GPSAltitude = int.Parse(matchLine.Groups[7].Value);
                        points.Add(point);
                    }
                    line = file.ReadLine();
                }
            }
            return points;
        }



    }
}
