using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FmsDb.Entities
{
    public class Place
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public long AltitudeMeters { get; set; }

        public override string ToString()
        {
            return Id + ", " + Name + ", " + Latitude + ", " + Longitude + ", " + AltitudeMeters;
        }
    }
}
