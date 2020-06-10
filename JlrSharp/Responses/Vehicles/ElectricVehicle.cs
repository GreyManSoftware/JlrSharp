using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using JlrSharp.Requests;
using JlrSharp.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace JlrSharp.Responses.Vehicles
{
    public sealed class ElectricVehicle : Vehicle
    {
        public ElectricVehicle(Vehicle vehicle)
        {
            vin = vehicle.vin;
            userId = vehicle.userId;
            role = vehicle.role;
        }

        /// <summary>
        /// Starts the preconditioning for electric vehicles
        /// </summary>
        /// <param name="pin">The users PIN</param>
        /// <param name="targetTemperature">Temperature is expressed without decimal point. 210 = 21.0</param>
        public void StartClimatePreconditioning(string pin, string targetTemperature = "210")
        {
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = @"application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v5+json",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.PhevService-v1+json; charset=utf",
            };

            ApiResponse climateToken = GenerateAuthenticationToken("ECC", pin);
            IRestResponse restResponse = PostRequest($"vehicles/{vin}/preconditioning", httpHeaders,
                new EvClimatePreconditioningSettings(climateToken["token"], true, targetTemperature));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Ev pre-condition", restResponse.Content, restResponse.ErrorException);
            }
        }

        /// <summary>
        /// Stops the preconditioning for electric vehicles
        /// </summary>
        /// <param name="pin">The users PIN</param>
        public void StopClimatePreconditioning(string pin)
        {
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = @"application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v5+json",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.PhevService-v1+json; charset=utf",
            };

            ApiResponse climateToken = GenerateAuthenticationToken("ECC", pin);
            IRestResponse restResponse = PostRequest($"vehicles/{vin}/preconditioning", httpHeaders,
                new EvClimatePreconditioningSettings(climateToken["token"], false, "210"));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Ev pre-condition", restResponse.Content, restResponse.ErrorException);
            }
        }

        /// <summary>
        /// Determines if the vehicle is currently charging
        /// </summary>
        /// <returns></returns>
        public bool IsCharging()
        {
            return VehicleStatusRaw.vehicleStatus.Any(status => status.key == "EV_CHARGING_STATUS" && (string)status.value == "CHARGING");
        }

        /// <summary>
        /// Determines if the vehicle is plugged in
        /// </summary>
        /// <returns></returns>
        public bool IsPluggedIn()
        {
            string chargingStatus = (string)VehicleStatusRaw.vehicleStatus.First(status => status.key == "EV_CHARGING_METHOD").value;

            if (chargingStatus != "NOTCONNECTED")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the time until battery charging is complete
        /// </summary>
        /// <returns></returns>
        public int GetTimeUntilCharged()
        {
            return Convert.ToInt32(VehicleStatusRaw.vehicleStatus
                .First(status => status.key == "EV_MINUTES_TO_FULLY_CHARGED").value);
        }

        /// <summary>
        /// Gets the current charge level
        /// </summary>
        /// <returns></returns>
        public int GetChargeLevel()
        {
            return Convert.ToInt32(VehicleStatusRaw.vehicleStatus
                .First(status => status.key == "EV_STATE_OF_CHARGE").value);
        }

        /// <summary>
        /// Starts charging the vehicle to the pre-determined charge level
        /// </summary>
        /// <param name="pin"></param>
        public void StartCharging(string pin)
        {
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = "application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v5+json",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.PhevService-v1+json; charset=utf",
            };

            ApiResponse chargeToken = GenerateAuthenticationToken("CP", pin);
            IRestResponse restResponse = PostRequest($"vehicles/{vin}/chargeProfile", httpHeaders,
                new ChargingRequestSettings(chargeToken["token"], true));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Start charging", restResponse.Content, restResponse.ErrorException);
            }
        }

        /// <summary>
        /// Starts charging the vehicle to the pre-determined charge level
        /// </summary>
        /// <param name="pin"></param>
        public void StopCharging(string pin)
        {
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = "application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v5+json",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.PhevService-v1+json; charset=utf",
            };

            ApiResponse chargeToken = GenerateAuthenticationToken("CP", pin);
            IRestResponse restResponse = PostRequest($"vehicles/{vin}/chargeProfile", httpHeaders,
                new ChargingRequestSettings(chargeToken["token"], false));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Stop charging", restResponse.Content, restResponse.ErrorException);
            }
        }

        public override int GetDistanceUntilEmpty()
        {
            return Convert.ToInt32(VehicleStatusRaw.vehicleStatus
                .First(status => status.key == "EV_RANGE_ON_BATTERY_MILES").value);
        }
    }
}
