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

            // Re-create UserDetails and TokenStore minimums
            UserDetails userDetails = new UserDetails
            {
                DeviceId = Guid.Parse(""),
                DeviceIdExpiry = DateTime.Parse(""),
                Email = "",
                UserId = "",
            };

            TokenStore tokenStore = new TokenStore
            {
                access_token = "",
                CreatedDate = DateTime.Parse("")
            };

            // Set the pin
            string myPin = "1234";

            // Connect to the JLR API with token data
            JlrSharpConnection jlrSharp = new JlrSharpConnection(userDetails, tokenStore);

            // Disable automatic refreshing of the tokens
            jlrSharp.AutoRefreshTokens = false;

            Trace.TraceInformation("Grabbing default vehicle");
            
            // Grab the generic vehicle
            Vehicle defaultVehicle = jlrSharp.GetPrimaryVehicle();

            Console.WriteLine($"Mileage: {defaultVehicle.GetMileage()}");
            Console.WriteLine($"Distance until empty: {defaultVehicle.GetDistanceUntilEmpty()}");
            defaultVehicle.Unlock(myPin);
            // wait before issuing these back to back
            defaultVehicle.Lock(myPin);

            // Determine the vehicle type
            if (defaultVehicle is GasVehicle gasVehicle)
            {
                Console.WriteLine($"The fuel level is at {gasVehicle.GetFuelLevelPercentage()}%");
                gasVehicle.StartEngine(myPin);
                // wait before issuing these back to back
                gasVehicle.StopEngine(myPin);
            }
            else if (defaultVehicle is ElectricVehicle electricVehicle)
            {
                Console.WriteLine($"Vehicle is charging: {electricVehicle.IsCharging()}");
                Console.WriteLine($"Time until charged: {electricVehicle.GetTimeUntilCharged()}");
                Console.WriteLine($"Charge level: {electricVehicle.GetChargeLevel()}");
                electricVehicle.StopCharging(myPin);
            }
        }
    }
}