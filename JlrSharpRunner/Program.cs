using System;
using System.Diagnostics;
using System.Threading.Tasks;
using JlrSharp;
using JlrSharp.Responses;

namespace JlrSharpRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            string myPin = "1234";
            Trace.Listeners.Add(new ConsoleTraceListener());

            JlrSharpConnection jlrSharp = new JlrSharpConnection();
            jlrSharp.AutoRefreshTokens = true;
            Trace.TraceInformation("Grabbing default vehicle");
            Vehicle defaultVehicle = jlrSharp.GetPrimaryVehicle();

            // Basic functionality
            VehicleHealthReport healthReport = defaultVehicle.GetVehicleHealth();
            int runTime = defaultVehicle.GetRemainingClimateRunTime();
            //int milesRemainingUntilService = defaultVehicle.GetDistanceUntilEmpty();
            //defaultVehicle.HonkAndBlink();
            //Task.Delay(5000).Wait();
            //defaultVehicle.Unlock(myPin);
            //Task.Delay(5000).Wait();
            //defaultVehicle.Lock(myPin);
            //Task.Delay(5000).Wait();
            //defaultVehicle.StartEngine(myPin);
            //Task.Delay(5000).Wait();
            //defaultVehicle.StopEngine(myPin);
        }
    }
}