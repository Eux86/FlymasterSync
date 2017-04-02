using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexData;
using RestSharp;

namespace DoaramaSharp
{
    class Program
    {
        private class DoaramaResponse : JsonSerializable
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

        static void Main(string[] args)
        {
            Test();

            Console.ReadLine();

            //byte[] filedata = Encoding.ASCII.GetBytes(File.ReadAllText(filePath));

            // Uploading track
            //var client = new RestClient("https://api.doarama.com/api/0.2/activity");
            //RestResponse response;
            //string content;
            //DoaramaResponse r;

            //var request = new RestRequest(Method.POST);
            //request.AddHeader("api-name", apiName);
            //request.AddHeader("api-key", apiKey);
            //request.AddHeader("user-key", "0e1c3fe9-8fcf-49bd-86cf-04605a750d32");
            ////request.AddHeader("user-id", "1");
            //request.AddHeader("Accept", "application/json");
            //request.AddFile("gps_track", filedata, "@myFile.gpx");
            //response = (RestResponse)client.Execute(request);
            //content = response.Content;
            //Console.WriteLine(content);
            //r = JsonSerializable.DeserializeFromJson<DoaramaResponse>(content);

            //string activityId = r.Id;

            //// Setting activity info to paragliding
            //client = new RestClient("https://api.doarama.com/api/0.2/activity/" + activityId);
            //request = new RestRequest(Method.POST);
            //request.AddHeader("api-name", apiName);
            //request.AddHeader("api-key", apiKey);
            //request.AddHeader("user-key", "0e1c3fe9-8fcf-49bd-86cf-04605a750d32");
            ////request.AddHeader("user-id", "1");
            //request.AddHeader("Accept","application/json");
            //request.AddParameter("application/json; charset=utf-8", "{\"activityTypeId\":29}", ParameterType.RequestBody);
            //request.RequestFormat = DataFormat.Json;
            //request.AddHeader("Content-Type", "application/json");
            //response = (RestResponse) client.Execute(request);
            //content = response.Content;
            //Console.WriteLine(content);

            //// Create visualization
            //client = new RestClient("https://api.doarama.com/api/0.2/visualisation");
            //request = new RestRequest(Method.POST);
            //request.AddHeader("api-name", apiName);
            //request.AddHeader("api-key", apiKey);
            //request.AddHeader("user-key", "0e1c3fe9-8fcf-49bd-86cf-04605a750d32");
            ////request.AddHeader("user-id", "1");
            //request.AddHeader("Accept", "application/json");
            //request.AddParameter("application/json; charset=utf-8", "{\"activityIds\":["+activityId+"]}", ParameterType.RequestBody);
            //request.RequestFormat = DataFormat.Json;
            //request.AddHeader("Content-Type", "application/json");
            //response = (RestResponse)client.Execute(request);
            //Console.WriteLine(response.Content);
            //r = JsonSerializable.DeserializeFromJson<DoaramaResponse>(response.Content);
            //content = response.Content;
            //Console.WriteLine(content);



        }

        static async void Test()
        {
            string apiName = "flymastersync";
            string apiKey = "a7454143-340f-448b-80bb-4c09039dad15";
            string userKey = "0e1c3fe9-8fcf-49bd-86cf-04605a750d32";

            string filePath = @"E:\EuxSoft\FlymasterSync\Tracks\150328115459.igc";

            var r = await DoaramaApiCall.UploadTrackAsync(filePath, userKey);
            if (await DoaramaApiCall.SetActivityToParagliding(r.Id, userKey))
            {
                r = await DoaramaApiCall.CreateVisualization(r.Id, userKey);
            }
            else
            {
                Console.WriteLine("Problems setting track info");
            }

            Process.Start(new ProcessStartInfo(@"http://api.doarama.com/api/0.2/visualisation?k=" + r.Key));
            
        }
    }

    class Test
    {
        public int ActivityTypeId { get; set; }
    }
}
