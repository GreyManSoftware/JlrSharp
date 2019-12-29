using System;
using System.Collections.Generic;
using System.Text;

namespace GreyMan.JlrSharp.Requests
{
    // This currently doesn't work
    [Serializable]
    public class ClimateControlSettings
    {
        public string key = "ClimateControlRccTargetTemp";
        public string value = (25 * 2).ToString();
        public bool applied = true;
    }

    /// <summary>
    /// Helper for creating engine start/stop requests
    /// </summary>
    public class ClimatePreconditioningSettings
    {
        public string token { get; set; }
        public List<ServiceParameter> serviceParameters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token">The ECC token</param>
        /// <param name="startStop">True starts the engine, false stops</param>
        /// <param name="targetTemperature">Temperature is expressed without decimal point. 210 = 21.0</param>
        public ClimatePreconditioningSettings(string reonToken)
        {
            token = reonToken;
            // FUEL_FIRED_HEATER_SETTING has a value of FFHSettingState = Unknown, Enabled, Disabled

            serviceParameters = new List<ServiceParameter>
            {
                //new ServiceParameter{key = "FUEL_FIRED_HEATER_SETTING", value = targetTemperature},
            };
        }
    }

    /// <summary>
    /// Helper for creating EV pre-condition requests
    /// </summary>
    public class EvClimatePreconditioningSettings
    {
        public string token { get; set; }
        public List<ServiceParameter> serviceParameters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token">The ECC token</param>
        /// <param name="startStop">True starts the engine, false stops</param>
        /// <param name="targetTemperature">Temperature is expressed without decimal point. 210 = 21.0</param>
        public EvClimatePreconditioningSettings(string eccToken, bool startStop, string targetTemperature = "210")
        {
            string command = startStop ? "START" : "STOP";
            int temp = Convert.ToInt32(targetTemperature);
            token = eccToken;
            
            // Ensure that target temperature is within limits
            if (temp < 155 && temp > 285)
            {
                targetTemperature = "210";
            }

            serviceParameters = new List<ServiceParameter>
            {
                new ServiceParameter{key = "PRECONDITIONING", value = command},
                new ServiceParameter{key = "TARGET_TEMPERATURE_CELSIUS", value = targetTemperature},
            };
        }
    }

    public class ServiceParameter
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}