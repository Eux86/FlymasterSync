using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GeoUtilities.Data;

namespace GeoUtilities.Serializers
{
    public class IgcTrackSerializer : ITrackSerializer
    {
        public void Serialize(Track track, StreamWriter writer)
        {
            writer.WriteLine("HFDTE" + track.Points[0].Time.ToString("ddMMyy"));
            writer.WriteLine("HODTM100GPSDATUM: WGS-84");
            foreach (TrackPoint point in track.Points)
            {
                writer.WriteLine("B" + point.Time.ToString("HHmmss") + point.Latitude + point.Longitude + "A" + point.BaroAltitude.ToString("00000", CultureInfo.InvariantCulture) + point.GPSAltitude.ToString("00000", CultureInfo.InvariantCulture));
            }
        }

        public Track Deserialize(StreamReader reader)
        {
            List<TrackPoint> points = new List<TrackPoint>();
            DateTime baseDate = new DateTime();
            reader.BaseStream.Position = 0;
            var line = reader.ReadLine();
            while (line != null)
            {
                Regex regexDate = new Regex(@"HFDTE(\d\d)(\d\d)(\d\d)");
                Match matchDate = regexDate.Match(line);
                if (matchDate.Success)
                {
                    baseDate = new DateTime(int.Parse(matchDate.Groups[3].Value), int.Parse(matchDate.Groups[2].Value), int.Parse(matchDate.Groups[1].Value));
                }
                Regex regexLine = new Regex(@"B(\d\d)(\d\d)(\d\d)(\d*.)(\d*.)A(\d{5})(\d{5})");
                Match matchLine = regexLine.Match(line);
                if (matchLine.Success)
                {
                    TrackPoint point = new TrackPoint();
                    point.Time = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day,
                        int.Parse(matchLine.Groups[1].Value), int.Parse(matchLine.Groups[2].Value), int.Parse(matchLine.Groups[3].Value));
                    point.Latitude = matchLine.Groups[4].Value;
                    point.Longitude = matchLine.Groups[5].Value;
                    point.BaroAltitude = int.Parse(matchLine.Groups[6].Value);
                    point.GPSAltitude = int.Parse(matchLine.Groups[7].Value);
                    points.Add(point);
                }
                line = reader.ReadLine();
            }
            return new Track() {Points = points};
        }
    }
}
