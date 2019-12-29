using System;
using System.Diagnostics;
using GreyMan.JlrSharp;
using GreyMan.JlrSharp.Responses;

namespace JlrSharpRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            JlrSharpConnection jlrSharp = new JlrSharpConnection();

            Trace.TraceInformation("Grabbing default vehicle");
            Vehicle defaultVehicle = jlrSharp.GetPrimaryVehicle();

            //VehicleHealthReport healthReport = defaultVehicle.GetVehicleHealth();
            defaultVehicle.GetNextServiceDue();
            //defaultVehicle.HonkAndBlink();
            //defaultVehicle.Unlock("1253");
            //defaultVehicle.Lock("1253");
            //defaultVehicle.GetCurrentClimateSettings();
            //defaultVehicle.StartEngine("1253");
            //defaultVehicle.SetClimateTemperature("28");
            //defaultVehicle.GetCurrentClimateSettings();
            //defaultVehicle.StopEngine("1253");
        }
    }
}