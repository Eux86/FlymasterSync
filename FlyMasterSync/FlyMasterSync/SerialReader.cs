using FlyMasterSerial.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlyMasterSerial.Exceptions;

namespace FlyMasterSerial
{
    public class SerialReader
    {
        const int READING_TIMEOUT = 500;
        const int READING_INTERVAL = 100;

        const string ASK_FLYMASTER_INFO = "$PFMSNP,*3A";
        const string ASK_FLIGHTS_LIST = "$PFMDNL,LST,*56";

        const string ERROR_READING_TIMEOUT = "Didn't receive a reply from the device";
        const string ERROR_NODATA = "No data received";

        public string PortName
        {
            get
            {
                if (comport != null && comport.IsOpen)
                    return comport.PortName;
                else
                    return null;
            }
        }

        SerialPort comport = new SerialPort();

        public bool Busy { get; set; }

        public SerialReader()
        {
            
        }

        public void Dispose()
        {
            comport.Dispose();
        }

        public async Task<bool> Connect(string portName)
        {
            Dispose();
            comport=new SerialPort();

            bool error = false;
            // Set the port's settings
            comport.BaudRate = 9600;
            comport.DataBits = 8;
            comport.StopBits = StopBits.One;
            comport.Parity = Parity.None;
            comport.PortName = portName;
            //comport.WriteTimeout = 100;
            comport.Handshake = Handshake.None;
            try
            {// Open the port
                try
                {
                    comport.Open();
                }
                catch (System.IO.IOException e)
                {
                    error = true;
                }
                if (comport.IsOpen)
                {
                    DeviceInfo devInfo = await GetDeviceInfo();
                    if (devInfo != null)
                    {
                        //Console.WriteLine(string.Format(@"Connected to Flymaster {0} (FW version: {1})",devInfo.Name,devInfo.Version));
                        return true;
                    }
                    //SendData("$PFMDNL,LST,*56");
                }
            }
            catch (UnauthorizedAccessException) { error = true; }
            catch (IOException) { error = true; }
            catch (ArgumentException) { error = true; }
            if (error) Console.WriteLine("Could not open the COM port.  Most likely it is already in use, has been removed, or is unavailable.");

            return false;
        }

        

        #region ASK FOR DATA

        public async Task<DeviceInfo> GetDeviceInfo()
        {
            if (comport.IsOpen)
            {
                List<string> data = await AskData(ASK_FLYMASTER_INFO);
                return ParseDeviceInfo(data[0]);
            }
            else
            {
                throw new NotConnectedException(NotConnectedException.ERROR_NOT_CONNECTED);
            }
        }
        public async Task<List<FlightInfo>> GetFlightsList()
        {
            if (comport.IsOpen)
            {
                List<string> data = await AskData(ASK_FLIGHTS_LIST);
                List<FlightInfo> flightList = new List<FlightInfo>();
                foreach (string f in data)
                {
                    flightList.Add(ParseFlightInfo(f));
                }
                return flightList;
            }
            else
            {
                throw new NotConnectedException(NotConnectedException.ERROR_NOT_CONNECTED);
            }
        }

        public async Task<List<FlightLogPoint>> GetFlightLog(string flightID)
        {
            return await GPSLogStreamReader.AskAsync(comport, flightID);
        }

        #endregion

        #region PARSING
        private FlightInfo ParseFlightInfo(string dataString)
        {
            //$PFMLST,013,001,28.09.14,17:52:08,00:46:11*32
            dataString = dataString.Substring(0, dataString.Length - 4);
            string[] parts = dataString.Split(',');
            string[] dateParts = parts[3].Split('.');
            string[] timeParts = parts[4].Split(':');
            string[] durationParts = parts[5].Split(':');
            int y = int.Parse(dateParts[2]);
            int M = int.Parse(dateParts[1]);
            int d = int.Parse(dateParts[0]);
            int h = int.Parse(timeParts[0]);
            int m = int.Parse(timeParts[1]);
            int s = int.Parse(timeParts[2]);
            DateTime date = new DateTime(y,M,d,h,m,s);
            h = int.Parse(durationParts[0]);
            m = int.Parse(durationParts[1]);
            s = int.Parse(durationParts[2]);
            TimeSpan duration = new TimeSpan(h,m,s);
            string id = string.Format(@"{0}{1}{2}{3}{4}{5}", date.Year, TwoDigitNumber(date.Month), TwoDigitNumber(date.Day), TwoDigitNumber(date.Hour), TwoDigitNumber(date.Minute), TwoDigitNumber(date.Second));
            FlightInfo fi = new FlightInfo()
            {
                ID = id,
                Date = date,
                Duration = duration
            };
            return fi;
        }
        
        private DeviceInfo ParseDeviceInfo(string deviceInfoString)
        {
            //$PFMSNP,NavSD,,00374,1.03x,1009.26,*2E
            string[] parts = deviceInfoString.Split(',');
            DeviceInfo di = new DeviceInfo()
            {
                Name = parts[1],
                Version = parts[4]
            };
            return di;
        }

        #endregion 

        #region IO

            // ############################## MAKE THIS THREAD SAFE! #############################
        private async Task<List<string>> AskData(string query)
        {
            while (Busy)
            {
                await Task.Delay(100);
            }
            Busy = true;

#if DEBUG
            Console.WriteLine("Query >> " + query);
#endif
            try
            {
                comport.WriteLine(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
 

            DateTime startTime = DateTime.Now;
            List<string> data = new List<string>();
            bool endReading = false;
            double timeout = 0;
            while ( (timeout < READING_TIMEOUT || !endReading) && comport.IsOpen)
            {
                await Task.Delay(READING_INTERVAL);
                try
                {
                    if (comport.BytesToRead > 0)
                    {

                        string temp = comport.ReadLine();
                        data.Add(temp);

                    }
                    else
                        endReading = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                timeout = DateTime.Now.Subtract(startTime).TotalMilliseconds;
            }
            if (data.Count > 0)
            {
                Busy = false;
                return data;
            }
            else
            {
                Busy = false;
                if (timeout > READING_TIMEOUT)
                    throw new Exception(ERROR_READING_TIMEOUT);
                else
                    throw new Exception(ERROR_NODATA);
            }
        }

        #endregion



        #region helpers


        // Output a number with minimum 2 digits. If number is 1 the output will be 01
        private string TwoDigitNumber(int number)
        {
            return number < 10 ? "0" + number : number.ToString();
        }

        #endregion

    }




}
