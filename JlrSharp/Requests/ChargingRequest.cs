using System;
using System.Collections.Generic;
using System.Text;

namespace JlrSharp.Requests
{
    public class ChargingRequestSettings
    {
        public string token { get; set; }
        public List<ServiceParameter> serviceParameters { get; set; }

        public ChargingRequestSettings(string cpToken, bool startStop)
        {
            string command = startStop ? "FORCE_ON" : "FORCE_OFF";
            token = cpToken;

            serviceParameters = new List<ServiceParameter>
            {
                new ServiceParameter{key = "CHARGE_NOW_SETTINGS", value = command}
            };
        }
    }
}
