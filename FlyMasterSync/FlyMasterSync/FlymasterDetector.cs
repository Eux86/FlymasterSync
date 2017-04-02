using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyMasterSerial
{
    static public class FlymasterDetector
    {
        static public async Task<string> Check()
        {
            string comName = null;

            string[] ports = SerialPort.GetPortNames();
            SerialReader reader = new SerialReader();
            foreach (var port in ports)
            {
#if DEBUG
                Console.WriteLine("Checking port: " + port);
#endif
                bool result = false;
                try
                {
                    result = await reader.Connect(port);
                }
                catch (Exception)
                {}

                if (result)
                {
                    comName = port;
                    break;
                }
            }
            reader.Dispose();
            return comName;
        }

    }
}
