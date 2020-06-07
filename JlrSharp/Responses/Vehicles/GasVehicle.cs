using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using JlrSharp.Requests;
using JlrSharp.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace JlrSharp.Responses
{
    [Serializable]
    public sealed class GasVehicle : Vehicle
    {
        public override int GetDistanceUntilEmpty()
        {
            VehicleStatusReport.VehicleStatus remainingFuel = VehicleStatusRaw.vehicleStatus.First(status => status.key == "DISTANCE_TO_EMPTY_FUEL");
            return Convert.ToInt32(Convert.ToDouble(remainingFuel.value) / 1.609);
        }

        /// <summary>
        /// Starts the engine
        /// </summary>
        public void StartEngine(string pin)
        {
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = "",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json",
            };

            IRestResponse restResponse = PostRequest($"vehicles/{vin}/engineOn", httpHeaders, GenerateAuthenticationToken("REON", pin));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Start Engine", restResponse.Content, restResponse.ErrorException);
            }
        }

        /// <summary>
        /// Stops the engine
        /// </summary>
        public void StopEngine(string pin)
        {
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = "",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json",
            };

            IRestResponse restResponse = PostRequest($"vehicles/{vin}/engineOff", httpHeaders, GenerateAuthenticationToken("REOFF", pin));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Stop Engine", restResponse.Content, restResponse.ErrorException);
            }
        }

        /// <summary>
        /// Retrieves the fuel level as a percentage
        /// </summary>
        /// <returns></returns>
        public int GetFuelLevelPercentage()
        {
            VehicleStatusReport.VehicleStatus odometerReading = VehicleStatusRaw.vehicleStatus.First(status => status.key == "FUEL_LEVEL_PERC");
            return Convert.ToInt32(odometerReading.value);
        }

        /// <summary>
        /// Returns whether the engine is running
        /// </summary>
        public bool IsEngineRunning()
        {
            return Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus.First(door => door.key == "VEHICLE_STATE_TYPE").value != "KEY_REMOVED");
        }

        /// <summary>
        /// Creates a fossil fuel specific vehicle
        /// </summary>
        /// <param name="vehicle"></param>
        public GasVehicle(Vehicle vehicle)
        {
            vin = vehicle.vin;
            userId = vehicle.userId;
            role = vehicle.role;
        }
    }
}
