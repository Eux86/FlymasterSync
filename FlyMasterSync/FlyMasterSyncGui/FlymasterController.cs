using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyMasterSerial;
using FlyMasterSerial.Data;

namespace FlyMasterSyncGui
{
    class FlymasterController : IDisposable
    {
        public delegate void DisconnectedEventHandler();
        public event DisconnectedEventHandler DisconnectedEvent;

        SerialReader _serial = new FlyMasterSerial.SerialReader();
        bool _isConnected = false;
        private List<FlightInfo> _flightList;
        private bool _busy;


        public async Task<bool> Connect()
        {
            await IsFree();

            _serial.Dispose();

            string portName = await FlymasterDetector.Check();
            if (portName != null)
            {
                if (await _serial.Connect(portName))
                {
                    _isConnected = true;
                    CheckConnection();
                    return true;
                }
            }
            _isConnected = false;

            
            return false;
        }

        private async void CheckConnection()
        {
            await IsFree();
            try
            {
                await GetDeviceInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _isConnected = false;
                if (DisconnectedEvent != null) DisconnectedEvent();
                return;
            }
            await Task.Delay(5000);
            CheckConnection();
        }

        public async Task<ObservableCollection<FlightInfo>> GetFlightList()
        {
            await IsFree();

            if (_isConnected)
            {
                _flightList = await _serial.GetFlightsList();
                return new ObservableCollection<FlightInfo>(_flightList);
            }
            else
            {
                return null;
            }
        }
        public async Task<DeviceInfo> GetDeviceInfo()
        {
            await IsFree();

            if (_isConnected)
            {
                return await _serial.GetDeviceInfo();
            }
            else
            {
                return null;
            }
        }
        
        public async Task<List<FlightLogPoint>> GetFlightTrack(string flightID)
        {
            await IsFree();
            _busy = true;
            if (_isConnected)
            {
                
                var ret = await _serial.GetFlightLog(flightID);
                _busy = false;
                return ret;
            }
            _busy = false;
            return null;
        }

        public void Dispose()
        {
            _serial.Dispose();
        }

        // Awaits that the serial is free. After 10 seconds it returns anyway. //TODO: It should raise an exception in this case.
        private async Task<bool> IsFree()
        {
            int count = 0;
            while ((_serial.Busy || _busy) && count<100)
            {
                await Task.Delay(100);
            }
            if (count >= 100) throw new Exception("asddas");
            return true;
        }
    }
}
