using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GreyMan.JlrSharp;
using GreyMan.JlrSharp.Responses;

namespace JlrSharpRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            string myPin = "1253";
            Trace.Listeners.Add(new ConsoleTraceListener());

            JlrSharpConnection jlrSharp = new JlrSharpConnection("cjdavies18@gmail.com", "ece77092-ac08-43f4-9551-8ddb1ad07b43", "88751ecd-81d2-4063-97ad-8becb34c8df0");

            Trace.TraceInformation("Grabbing default vehicle");
            Vehicle defaultVehicle = jlrSharp.GetPrimaryVehicle();

            // Basic functionality
            //VehicleHealthReport healthReport = defaultVehicle.GetVehicleHealth();
            //int milesRemainingUntilService = defaultVehicle.GetNextServiceDue();
            //defaultVehicle.HonkAndBlink();
            //Task.Delay(5000).Wait();
            defaultVehicle.Unlock(myPin);
            Task.Delay(5000).Wait();
            defaultVehicle.Lock(myPin);
            Task.Delay(5000).Wait();
            //defaultVehicle.StartEngine(myPin);
            //Task.Delay(5000).Wait();
            //defaultVehicle.StopEngine(myPin);
        }
    }
}