using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoaramaSharp
{
    public static class DoaramaVisualizationMaker
    {
        const string ApiName = "flymastersync";
        const string ApiKey = "a7454143-340f-448b-80bb-4c09039dad15";


        public static async Task<string> CreateVisualizationAsync(string igcFilePath, string userApiKey, string trackName)
        {
            var r = await DoaramaApiCall.UploadTrackAsync(igcFilePath, userApiKey, trackName);
            if (r != null)
            {
                if (await DoaramaApiCall.SetActivityToParagliding(r.Id, userApiKey))
                {
                    r = await DoaramaApiCall.CreateVisualization(r.Id, userApiKey);
                }
                else
                {
                    Console.WriteLine("Problems setting track info");
                    return null;
                }
                return r.Key;
            }
            else return null;
        }

        public static Uri GetVisualizationUrl(string visualizationId)
        {
            return new Uri("http://api.doarama.com/api/0.2/visualisation?k="+visualizationId);
        }
    }
}
