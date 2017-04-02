using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoUtilities.Helpers;

namespace GeoUtilities.Data
{
    public class Track
    {
        private List<TrackPoint> _points;

        public List<TrackPoint> Points
        {
            get { return _points; }
            set { _points = value; }
        }

        /// <summary>
        /// Returns the max speed obtained in the track
        /// </summary>
        /// <returns>The speed in km/h</returns>
        public double GetMaxSpeed()
        {
            double speed = -1;

            TrackPoint prevPoint = null;
            foreach (var trackPoint in Points)
            {
                if (prevPoint != null)
                {
                    var deltaTime = trackPoint.Time.Subtract(prevPoint.Time).TotalSeconds;
                    if (deltaTime==0) continue;
                    var dist = MathGeo.Distance(trackPoint.LatToDecimal(), trackPoint.LonToDecimal(), prevPoint.LatToDecimal(), prevPoint.LonToDecimal());
                    var sp = (dist*3600)/deltaTime;
                    if (sp > speed) speed = sp;
                    Console.WriteLine("Point speed = "+sp+"km/h");
                }
                prevPoint = trackPoint;
            }

            return speed;
        }
    }
}
