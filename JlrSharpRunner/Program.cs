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

            // Create the credential objects
            UserDetails userDetails = new UserDetails
            {
                DeviceId = Guid.Parse("EBE7F85C-023C-4880-A94E-74F25510CCC2"),
                DeviceIdExpiry = DateTime.Now.AddHours(24),
                Email = "benjie@still-waters.co.uk",
                UserId = "16DE9E3D9CA",
            };

            TokenStore tokenStore = new TokenStore
            {
                access_token = "a5de7436-4c3b-4021-9b0a-bcb1ff9e2b5a",
                CreatedDate = DateTime.Now,
            };

            // Set the pin
            string myPin = "2611";

            // Connect to the JLR API
            JlrSharpConnection jlrSharp = new JlrSharpConnection(userDetails, tokenStore);

            // Disable automatic refreshing of the tokens
            jlrSharp.AutoRefreshTokens = false;

            Trace.TraceInformation("Grabbing default vehicle");
            
            // Grab the generic vehicle
            Vehicle defaultVehicle = jlrSharp.GetPrimaryVehicle();

            // Determine the vehicle type
            if (defaultVehicle is GasVehicle gasVehicle)
            {
                Console.WriteLine("We got a gas vehicle");
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