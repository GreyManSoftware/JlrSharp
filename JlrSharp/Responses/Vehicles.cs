using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
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
            // Grab the vehicle health status using the token from above
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
        /// Sets the RequestClient for vehicle based API queries
        /// </summary>
        /// <param name="vehicleRequestClient"></param>
        internal void SetVehicleRequestClient(RestClient vehicleRequestClient)
        {
            VehicleRequestClient = vehicleRequestClient;
        }
    }
}