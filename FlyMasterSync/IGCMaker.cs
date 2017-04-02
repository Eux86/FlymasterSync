using FlyMasterSerial.Data;
using System;
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
        
        public static void Make(List<FlightLogPoint> points, string path = "output.igc")
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
                file.WriteLine("HFDTE"+points[0].Time.ToString("ddMMyy"));
                file.WriteLine("HODTM100GPSDATUM: WGS-84");
                foreach (FlightLogPoint point in points)
                {
                    string baroAltFormat = point.BaroAltitude < 0 ? "0000" : "00000";       // The characters for the altitude number are always 5. If the number has a "-" sign, then we must use 1 less character for the number.
                    string gpsAltFormat = point.GPSAltitude < 0 ? "0000" : "00000";
                    file.WriteLine("B" + point.Time.ToString("HHmmss") + point.Latitude + point.Longitude + "A" + point.BaroAltitude.ToString(baroAltFormat, CultureInfo.InvariantCulture) + point.GPSAltitude.ToString(gpsAltFormat, CultureInfo.InvariantCulture));
                }
            }  
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
