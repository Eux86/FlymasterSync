using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexData;
using RestSharp;

namespace DoaramaSharp
{
    class DoaramaResponse : JsonSerializable
    {
        private string _id;
        private string _key;
        private string _infoUrl;
        private string _authorUrl;

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string InfoUrl
        {
            get { return _infoUrl; }
            set { _infoUrl = value; }
        }

        public string AuthorUrl
        {
            get { return _authorUrl; }
            set { _authorUrl = value; }
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }
    }

    static class DoaramaApiCall
    {
        static string _apiName = "flymastersync";
        static string _apiKey = "a7454143-340f-448b-80bb-4c09039dad15";

        public async static Task<DoaramaResponse> UploadTrackAsync(string igcPath, string userKey, string trackName = "UploadedWithFlymasterSync.igc")
        {
            byte[] filedata = Encoding.ASCII.GetBytes(File.ReadAllText(igcPath));
            
            var client = new RestClient("https://api.doarama.com/api/0.2/activity");
            var request = new RestRequest(Method.POST);
            request.AddHeader("api-name", _apiName);
            request.AddHeader("api-key", _apiKey);
            request.AddHeader("user-key", userKey);
            request.AddHeader("Accept", "application/json");
            request.AddFile("gps_track", filedata, trackName);
            var response = await Task.Run(() => (RestResponse) client.Execute(request));
            var content = response.Content;
#if DEBUG
            Console.WriteLine(content);
#endif   
            DoaramaResponse r = JsonSerializable.DeserializeFromJson<DoaramaResponse>(content);

            return r;
        }

        public static async Task<bool> SetActivityToParagliding(string activityId, string userKey)
        {
            var client = new RestClient("https://api.doarama.com/api/0.2/activity/" + activityId);
            var request = new RestRequest(Method.POST);
            request.AddHeader("api-name", _apiName);
            request.AddHeader("api-key", _apiKey);
            request.AddHeader("user-key", userKey);
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json; charset=utf-8", "{\"activityTypeId\":29}", ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            var result = await Task.Run(() =>
            {
                try
                {
                    client.Execute(request);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return false;
                }
            });
#if DEBUG
            Console.WriteLine("Response = "+result);
#endif
            return result;
        }

        public static async Task<DoaramaResponse> CreateVisualization(string activityId, string userKey)
        {
            var client = new RestClient("https://api.doarama.com/api/0.2/visualisation");
            var request = new RestRequest(Method.POST);
            request.AddHeader("api-name", _apiName);
            request.AddHeader("api-key", _apiKey);
            request.AddHeader("user-key", userKey);
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json; charset=utf-8", "{\"activityIds\":[" + activityId + "]}", ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            var response = await Task.Run(() => (RestResponse) client.Execute(request));
            Console.WriteLine(response.Content);
            var r = JsonSerializable.DeserializeFromJson<DoaramaResponse>(response.Content);
#if DEBUG
            var content = response.Content;
            Console.WriteLine(content);
#endif
            return r;
        }
    }
}
