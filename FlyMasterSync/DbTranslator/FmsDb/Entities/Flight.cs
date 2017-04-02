using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyMasterSyncGui.Forms;

namespace FmsDb.Entities
{
    public class Flight
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
        public string Comments { get; set; }
        public Place Takeoff { get; set; }

        public override string ToString()
        {
            return Id + ", " + Date + ", " + Duration + ", " + Takeoff;
        }
    }

    


}
