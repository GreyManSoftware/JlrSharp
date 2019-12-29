using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using GreyMan.JlrSharp.Requests;
using GreyMan.JlrSharp.Utils;
using RestSharp;
using RestSharp.Deserializers;

namespace GreyMan.JlrSharp.Responses
{


    public class VehicleCollection
    {
        public List<Vehicle> Vehicles { get; set; }
    }

    [Serializable]
    public sealed class Vehicle
    {
        private RestClient VehicleRequestClient { get; set; }
        public string userId { get; set; }
        public string vin { get; set; }
        public string role { get; set; }
        public VehicleStatusReport VehicleStatus { get; private set; }

        /// <summary>
        /// Starts the engine
        /// </summary>
        public void StartEngine(string pin)
        {
            RestRequest startEngineRequest = new RestRequest($"vehicles/{vin}/engineOn", Method.POST);
            ApiResponse result = GenerateAuthenticationToken("REON", pin);
            startEngineRequest.AddHeader("Accept", "");
            startEngineRequest.AddHeader("Content-Type", @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json");
            startEngineRequest.AddJsonBody(result);
            IRestResponse startResponse = VehicleRequestClient.Execute(startEngineRequest);

            if (!startResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error starting engine");
            }
        }

        /// <summary>
        /// Stops the engine
        /// </summary>
        public void StopEngine(string pin)
        {
            RestRequest stopEngineRequest = new RestRequest($"vehicles/{vin}/engineOff", Method.POST);
            ApiResponse result = GenerateAuthenticationToken("REOFF", pin);
            stopEngineRequest.AddHeader("Accept", "");
            stopEngineRequest.AddHeader("Content-Type", @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json");
            stopEngineRequest.AddJsonBody(result);
            IRestResponse stopResponse = VehicleRequestClient.Execute(stopEngineRequest);

            if (!stopResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error stopping engine");
            }
        }

        // TODO: This currently doesn't work
        /// <summary>
        /// Retrieves the current climate control setting
        /// </summary>
        public void GetCurrentClimateSettings()
        {
            RestRequest climateTempRequest = new RestRequest($"vehicles/{vin}/settings/ClimateControlRccTargetTemp", Method.GET, DataFormat.Json);
            climateTempRequest.AddHeader("Content-Type", "application/json");
            IRestResponse climateTempResponse = VehicleRequestClient.Execute(climateTempRequest);

            if (!climateTempResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error retrieving climate target temperature");
            }
        }

        // TODO: This currently doesn't work
        public void SetClimateTemperature(string targetTemperature = "25")
        {
            RestRequest climateTempSetRequest = new RestRequest($"vehicles/{vin}/settings/", Method.POST, DataFormat.Json);
            climateTempSetRequest.AddHeader("Content-Type", "application/json");
            climateTempSetRequest.AddJsonBody(new ClimateControlSettings()); 
            IRestResponse climateTempResponse = VehicleRequestClient.Execute(climateTempSetRequest);

            if (!climateTempResponse.IsSuccessful ||
                climateTempResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException("Error setting climate target setting");
            }
        }

        /// <summary>
        /// Sets the preconditioning for electric vehicles
        /// </summary>
        /// <param name="pin">The users PIN</param>
        /// <param name="startStop">True starts the engine, false stops</param>
        /// <param name="targetTemperature">Temperature is expressed without decimal point. 210 = 21.0</param>
        public void EvClimatePreconditioning(string pin, bool startStop, string targetTemperature = "210")
        {
            RestRequest climateRequest = new RestRequest($"vehicles/{vin}/preconditioning", Method.POST);
            climateRequest.AddHeader("Content-Type", @"application/vnd.wirelesscar.ngtp.if9.PhevService-v1+json; charset=utf");
            climateRequest.AddHeader("Accept", @"application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v5+json");
            ApiResponse climateToken = GenerateAuthenticationToken("ECC", pin);
            climateRequest.AddJsonBody(new EvClimatePreconditioningSettings(climateToken["token"], startStop, targetTemperature));
            string debug =
                JsonSerializer.Serialize(
                    new EvClimatePreconditioningSettings(climateToken["token"], startStop, targetTemperature));
            
            IRestResponse climateResponse = VehicleRequestClient.Execute(climateRequest);

            if (!climateResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error starting engine");
            }
        }

        /// <summary>
        /// Retrieves the next service due in miles
        /// </summary>
        public int GetNextServiceDue()
        {
            VehicleStatusReport.VehicleStatus odometerReading = VehicleStatus.vehicleStatus.First(status => status.key == "ODOMETER_MILES");
            return Convert.ToInt32(odometerReading.value);
        }

        /// <summary>
        /// Retrieves the fuel level as a percentage
        /// </summary>
        /// <returns></returns>
        public int GetFuelLevelPercentage()
        {
            VehicleStatusReport.VehicleStatus odometerReading = VehicleStatus.vehicleStatus.First(status => status.key == "FUEL_LEVEL_PERC");
            return Convert.ToInt32(odometerReading.value);
        }

        /// <summary>
        /// Honks the horn and flashes the lights
        /// </summary>
        public void HonkAndBlink()
        {
            RestRequest honkBlinkRequest = new RestRequest($"vehicles/{vin}/honkBlink", Method.POST);
            honkBlinkRequest.AddHeader("Content-Type", @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json; charset=utf-8");
            honkBlinkRequest.AddHeader("Accept", @"application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v4+json");
            honkBlinkRequest.AddJsonBody(GenerateAuthenticationToken("HBLF", GetVinProtectedPin()));

            IRestResponse honkBlinkResponse = VehicleRequestClient.Execute(honkBlinkRequest);

            if (!honkBlinkResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error honk and blinking");
            }
        }

        /// <summary>
        /// Locks the vehicle
        /// </summary>
        public void Lock(string pin)
        {
            RestRequest lockRequest = new RestRequest($"vehicles/{vin}/lock", Method.POST);
            lockRequest.AddHeader("Accept", "");
            lockRequest.AddHeader("Content-Type", @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json");
            lockRequest.AddJsonBody(GenerateAuthenticationToken("RDL", pin));

            IRestResponse lockResponse = VehicleRequestClient.Execute(lockRequest);

            if (!lockResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error locking vehicle");
            }
        }

        /// <summary>
        /// Unlocks the vehicle
        /// </summary>
        public void Unlock(string pin)
        {
            RestRequest unlockRequest = new RestRequest($"vehicles/{vin}/unlock", Method.POST);
            unlockRequest.AddHeader("Accept", "");
            unlockRequest.AddHeader("Content-Type", @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json");
            unlockRequest.AddJsonBody(GenerateAuthenticationToken("RDU", pin));

            IRestResponse unlockResponse = VehicleRequestClient.Execute(unlockRequest);
            
            if (!unlockResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error unlocking vehicle");
            }
        }

        /// <summary>
        /// Returns the Vehicle Health Report
        /// </summary>
        /// <returns></returns>
        public VehicleHealthReport GetVehicleHealth()
        {
            RestRequest healthRequest = new RestRequest($"vehicles/{vin}/healthstatus", Method.POST);
            healthRequest.AddHeader("Accept", @"application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v4+json");
            healthRequest.AddHeader("Content-Type", @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json; charset=utf-8");
            healthRequest.AddJsonBody(GenerateAuthenticationToken("VHS"));

            IRestResponse healthResponse = VehicleRequestClient.Execute(healthRequest);

            if (!healthResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error retrieving vehicle health status");
            }

            return JsonSerializer.Deserialize<VehicleHealthReport>(healthResponse.Content);
        }

        /// <summary>
        /// Retrieves the subscriptions the vehicle is enrolled in
        /// </summary>
        public void GetSubscriptions()
        {
            RestRequest subscriptionRequest = new RestRequest($"vehicles/{vin}/subscriptionpackages", Method.GET, DataFormat.Json);
            IRestResponse subscriptionResponse = VehicleRequestClient.Execute(subscriptionRequest);
            System.IO.File.WriteAllText(@"c:\users\Chris\appdata\local\temp\content.txt", subscriptionResponse.Content);
        }

        /// <summary>
        /// Generate the appropriate token for the given service
        /// </summary>
        /// <param name="serviceName">The service requested</param>
        /// <param name="pin">The pin to use</param>
        /// <returns></returns>
        private ApiResponse GenerateAuthenticationToken(string serviceName, string pin = "")
        {
            TokenData tokenData = GenerateTokenData(serviceName, pin);
            RestRequest tokenRequest = new RestRequest($"vehicles/{vin}/users/{userId}/authenticate", Method.POST);
            tokenRequest.AddHeader("Content-Type", @"application/vnd.wirelesscar.ngtp.if9.AuthenticateRequest-v2+json; charset=utf-8");
            tokenRequest.AddJsonBody(tokenData);
            IRestResponse tokenResponse = VehicleRequestClient.Execute(tokenRequest);

            if (!tokenResponse.IsSuccessful)
            {
                throw new InvalidOperationException($"Error generating {serviceName} token");
            }

            return JsonSerializer.Deserialize<ApiResponse>(tokenResponse.Content);
        }

        /// <summary>
        /// Creates generic token data with given service name
        /// </summary>
        /// <param name="serviceName">The name of the token service being requested</param>
        /// <param name="pin">The pin to use</param>
        private TokenData GenerateTokenData(string serviceName, string pin = "")
        {
            // Generate VHS token for request below - this uses an empty pin
            TokenData tokenData = new TokenData
            {
                ["serviceName"] = serviceName,
                ["pin"] = pin
            };

            return tokenData;
        }

        /// <summary>
        /// Generates a pin based on the last 4 digits of the VIN
        /// </summary>
        /// <returns></returns>
        private string GetVinProtectedPin()
        {
            return vin.Substring(vin.Length - 4, 4);
        }

        /// <summary>
        /// Retrieves the next service due in miles
        /// </summary>
        public void RefreshVehicleStatusReport()
        {
            RestRequest vehicleStatusRequest = new RestRequest($"vehicles/{vin}/status", Method.GET, DataFormat.Json);
            vehicleStatusRequest.AddHeader("Accept", @"application/vnd.ngtp.org.if9.healthstatus-v2+json");
            IRestResponse<VehicleStatusReport> vehicleStatusResponse = VehicleRequestClient.Execute<VehicleStatusReport>(vehicleStatusRequest);
            
            if (!vehicleStatusResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error retrieving vehicle status report");
            }

            VehicleStatus = vehicleStatusResponse.Data;
        }

        /// <summary>
        /// Sets the RequestClient for vehicle based API queries
        /// </summary>
        /// <param name="vehicleRequestClient"></param>
        internal void SetVehicleRequestClient(RestClient vehicleRequestClient)
        {
            VehicleRequestClient = vehicleRequestClient;
            RefreshVehicleStatusReport();
        }
    }
}