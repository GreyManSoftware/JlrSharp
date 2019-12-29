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
            string myPin = "1234";
            Trace.Listeners.Add(new ConsoleTraceListener());

            JlrSharpConnection jlrSharp = new JlrSharpConnection("your@email_address.com", "your_password");

            Trace.TraceInformation("Grabbing default vehicle");
            Vehicle defaultVehicle = jlrSharp.GetPrimaryVehicle();

            // Basic functionality
            VehicleHealthReport healthReport = defaultVehicle.GetVehicleHealth();
            defaultVehicle.GetNextServiceDue();
            defaultVehicle.HonkAndBlink();
            defaultVehicle.Unlock(myPin);
            defaultVehicle.Lock(myPin);
            defaultVehicle.StartEngine(myPin);
            defaultVehicle.StopEngine(myPin);
        }
    }
}