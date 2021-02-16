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
        // TODO: This currently doesn't work
        /// <summary>
        /// Retrieves the current climate control setting
        /// </summary>
        public void GetCurrentClimateSettings()
        {
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Content-Type"] = "application/json",
            };

            IRestResponse restResponse = GetRequest($"vehicles/{vin}/settings/ClimateControlRccTargetTemp", httpHeaders);

            if (!restResponse.IsSuccessful)
            {
                RequestException.GenerateRequestException("Get Climate Settings", restResponse.Content, restResponse.ErrorException);
            }
        }

        // TODO: This likely isn't working 100%
        // Range of 31-57. 31 == cold, 57 == hot
        public void SetClimateTemperature(int targetTemperature = 41)
        {
            RestRequest climateTempSetRequest = new RestRequest($"vehicles/{vin}/settings/", Method.POST);

            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Content-Type"] = "application/json",
            };

            IRestResponse restResponse = PostRequest($"vehicles/{vin}/settings", httpHeaders, new ClimateControlSettings(42));

            if (!restResponse.IsSuccessful || restResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                RequestException.GenerateRequestException("Set Climate Settings", restResponse.Content, restResponse.ErrorException);
            }
        }

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
                RequestException.GenerateRequestException("Start Engine", restResponse.Content, restResponse.ErrorException);
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
                RequestException.GenerateRequestException("Stop Engine", restResponse.Content, restResponse.ErrorException);
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
