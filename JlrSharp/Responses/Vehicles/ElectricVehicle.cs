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
    public class ElectricVehicle : Vehicle
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
        /// <param name="updateStatus">Determines if the vehicle status should be updated after the command is executed</param>
        public void StartClimatePreconditioning(string pin, string targetTemperature = "210", bool updateStatus = false)
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
                RequestException.GenerateRequestException("Ev pre-condition", restResponse.Content, restResponse.ErrorException);
            }

            if (updateStatus)
            {
                UpdateVehicleStatus();
            }
        }

        /// <summary>
        /// Stops the preconditioning for electric vehicles
        /// </summary>
        /// <param name="pin">The users PIN</param>
        /// <param name="updateStatus">Determines if the vehicle status should be updated after the command is executed</param>
        public void StopClimatePreconditioning(string pin, bool updateStatus = false)
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
                RequestException.GenerateRequestException("Ev pre-condition", restResponse.Content, restResponse.ErrorException);
            }

            if (updateStatus)
            {
                UpdateVehicleStatus();
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
        /// <param name="updateStatus">Determines if the vehicle status should be updated after the command is executed</param>
        public void StartCharging(string pin, bool updateStatus = false)
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
                RequestException.GenerateRequestException("Start charging", restResponse.Content, restResponse.ErrorException);
            }

            if (updateStatus)
            {
                UpdateVehicleStatus();
            }
        }

        /// <summary>
        /// Starts charging the vehicle to the pre-determined charge level
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="updateStatus">Determines if the vehicle status should be updated after the command is executed</param>
        public void StopCharging(string pin, bool updateStatus = false)
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
                RequestException.GenerateRequestException("Stop charging", restResponse.Content, restResponse.ErrorException);
            }

            if (updateStatus)
            {
                UpdateVehicleStatus();
            }
        }

        public override int GetDistanceUntilEmpty()
        {
            return Convert.ToInt32(VehicleStatusRaw.vehicleStatus
                .First(status => status.key == "EV_RANGE_ON_BATTERY_MILES").value);
        }
    }
}
