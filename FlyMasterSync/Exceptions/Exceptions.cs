using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyMasterSerial.Exceptions
{

    public class NotConnectedException : Exception
    {
        public const string ERROR_NOT_CONNECTED = "Not connected to a Flymaster device";

        public NotConnectedException()
        {
        }

        public NotConnectedException(string message)
            : base(message)
        {
        }

        public NotConnectedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
