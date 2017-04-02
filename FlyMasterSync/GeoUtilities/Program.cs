using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoUtilities.Data;
using GeoUtilities.Serializers;

namespace GeoUtilities
{
    class Program
    {
        static void Main(string[] args)
        {
            ITrackSerializer igcSerializer = new IgcTrackSerializer();
            Track track = null;
            using (StreamReader reader = new StreamReader(@"E:\FlymasterSync\Tracks\150322132758.igc"))
            {
                track = igcSerializer.Deserialize(reader);
            }
            Console.WriteLine(track.GetMaxSpeed()+"km/h");
            Console.ReadLine();
        }
    }
}
