using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoUtilities.Data
{
    public class TrackPoint
    {
        private string latitude;
        public string Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                latitude = value;
            }
        }
        private string longitude;
        public string Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                longitude = value;
            }
        }
        private int gpsAltitude;
        public int GPSAltitude
        {
            get
            {
                return gpsAltitude;
            }
            set
            {
                gpsAltitude = value;
            }
        }
        private DateTime time;
        public DateTime Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
            }
        }
        private int baroAltitude;
        public int BaroAltitude
        {
            get
            {
                return baroAltitude;
            }
            set
            {
                baroAltitude = value;
            }
        }
        private int presure;
        public int Presure
        {
            get
            {
                return presure;
            }
            set
            {
                presure = value;
            }
        }

        public float LatToDecimal()
        {
            var degree = latitude.Substring(0, 2);
            var minutes = latitude.Substring(2, 2) + "." + latitude.Substring(4, 3);
            var sign = latitude.Substring(7, 1) == "S" ? -1 : 1;
            return sign * (int.Parse(degree) + float.Parse(minutes, CultureInfo.InvariantCulture) / 60);
        }

        public float LonToDecimal()
        {
            var degree = longitude.Substring(0, 3);
            var minutes = longitude.Substring(3, 2) + "." + longitude.Substring(5, 3);
            var sign = longitude.Substring(8, 1) == "W" ? -1 : 1;
            return sign * (int.Parse(degree) + float.Parse(minutes, CultureInfo.InvariantCulture) / 60);
        }

        public override string ToString()
        {
            return string.Format(@"{0} Lat {1} Long {2} Alt {3} GPSAlt {4}", time.ToShortTimeString(), latitude, longitude, baroAltitude, gpsAltitude);
        }


    }
}
