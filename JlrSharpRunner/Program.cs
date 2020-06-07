using System;
using System.Diagnostics;
using System.Threading.Tasks;
using JlrSharp;
using JlrSharp.Responses;
using JlrSharp.Responses.Vehicles;

namespace JlrSharpRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            // Set the pin
            string myPin = "1234";

            // Connect to the JLR API
            JlrSharpConnection jlrSharp = new JlrSharpConnection();

            // Disable automatic refreshing of the tokens
            jlrSharp.AutoRefreshTokens = false;

            Trace.TraceInformation("Grabbing default vehicle");
            
            // Grab the generic vehicle
            Vehicle defaultVehicle = jlrSharp.GetPrimaryVehicle();

            // Determine the vehicle type
            if (defaultVehicle is GasVehicle gasVehicle)
            {
                Console.WriteLine("We got a gas vehicle");
                gasVehicle.GetCurrentClimateSettings();
            }
            else if (defaultVehicle is ElectricVehicle electricVehicle)
            {
                Console.WriteLine("We got an EV");
                Console.Write($"Time until charged {electricVehicle.GetTimeUntilCharged()}");
                Console.WriteLine($"Distance until empty{electricVehicle.GetDistanceUntilEmpty()}");
                Console.WriteLine($"Charge level: {electricVehicle.GetChargeLevel()}");
            }
        }
    }
}