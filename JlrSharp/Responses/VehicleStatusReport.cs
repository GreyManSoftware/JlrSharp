using System;
using System.Collections.Generic;
using System.Text;

namespace GreyMan.JlrSharp.Responses
{
    public class VehicleStatusReport
    {
        public List<VehicleStatus> vehicleStatus { get; set; }
        public List<VehicleAlert> vehicleAlerts { get; set; }
        public DateTime lastUpdatedTime { get; set; }

        public class VehicleStatus
        {
            public string key { get; set; }
            public object value { get; set; }
            public override string ToString()
            {
                return key;
            }
        }

        public class VehicleAlert
        {
            public string key { get; set; }
            public string value { get; set; }
            public bool active { get; set; }
            public DateTime lastUpdatedTime { get; set; }
        }
    }
}