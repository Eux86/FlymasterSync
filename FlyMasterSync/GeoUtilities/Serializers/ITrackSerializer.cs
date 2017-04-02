using GeoUtilities.Data;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace GeoUtilities.Serializers
{
    public interface ITrackSerializer
    {
        void Serialize(Track track, StreamWriter writer);
        Track Deserialize(StreamReader reader);
    }
}