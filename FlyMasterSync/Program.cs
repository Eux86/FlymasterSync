using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Windows.Forms.VisualStyles;
using FlyMasterSerial.Data;

namespace FlyMasterSerial
{
    class Program
    {
        private static bool _waiting;

        static bool alive = true;
        static void Main(string[] args)
        {
            Test();
            while (alive) {}
        }

        static async void Test()
        {
            string portName = await FlymasterDetector.Detect();
            if (portName != null)
            {
                SerialReader s = new SerialReader();
                await s.Connect(portName);

                DeviceInfo devInfo = await s.GetDeviceInfo();
                Console.WriteLine(string.Format(@"Connected to Flymaster {0} (FW version: {1})", devInfo.Name, devInfo.Version));

                List<FlightInfo> fl = await s.GetFlightsList();
                Console.WriteLine("Registered flights (" + fl.Count + "):");
                int i = 1;
                foreach (FlightInfo fi in fl)
                {
                    Console.WriteLine(string.Format(@"Flight: {0} - Date: {1} - Duration: {2}", i++, fi.Date, fi.Duration));
                }
                Console.WriteLine("\nWhich flight?");
                int sel = -1;
#if !DEBUG
                string input = Console.ReadLine();
                int.TryParse(input, out sel);
                var points = await s.GetFlightLog(fl[sel - 1].ID);
                IGCMaker.Make(points, @"C:\Users\Eugenio\Desktop\" + sel + ".igc");
                Console.WriteLine("IGC Exported");
#else
                var points = await s.GetFlightLog(fl[0].ID);
                IGCMaker.Make(points, @"C:\Users\Eugenio\Desktop\1.igc");
                Console.WriteLine("IGC Exported: " + points.Count + " points");

                

#endif



            }
            else
            {
                Console.WriteLine("No flymaster found");
            }

            Console.ReadLine();
            alive = false;
        }

        static private async Task<bool> awaitResponse()
        {
            _waiting = true;
            while (_waiting)
            {
                await Task.Delay(100);
            }
            Console.WriteLine("Done");
            return true;
        }

    }
}
