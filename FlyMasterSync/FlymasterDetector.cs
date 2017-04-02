using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyMasterSerial
{
    public static class FlymasterDetector
    {
        public static async Task<string> Detect()
        {
            string comName = null;

            string[] ports = SerialPort.GetPortNames();
            SerialReader reader = new SerialReader();
            bool result = false;
            foreach (string port in ports)
            {
#if DEBUG
                Console.WriteLine("Checking port: " + port);
#endif
                comName = port;
                result = await reader.Connect(port);
                if (result) break;
            }
            reader.Dispose();
            return comName;
        }

    }
}
