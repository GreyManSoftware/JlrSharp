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
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = "",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json",
            };

            IRestResponse stopResponse = PostRequest($"vehicles/{vin}/engineOn", httpHeaders, GenerateAuthenticationToken("REON", pin));

            if (!stopResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error starting engine");
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

            IRestResponse stopResponse = PostRequest($"vehicles/{vin}/engineOff", httpHeaders, GenerateAuthenticationToken("REOFF", pin));

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
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Content-Type"] = "application/json",
            };

            IRestResponse climateTempResponse = GetRequest($"vehicles/{vin}/settings/ClimateControlRccTargetTemp", httpHeaders);

            if (!climateTempResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error retrieving climate target temperature");
            }
        }

        // TODO: This currently doesn't work
        public void SetClimateTemperature(string targetTemperature = "25")
        {
            RestRequest climateTempSetRequest = new RestRequest($"vehicles/{vin}/settings/", Method.POST);

            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Content-Type"] = "application/json",
            };

            IRestResponse climateTempResponse = PostRequest($"vehicles/{vin}/settings", httpHeaders, new ClimateControlSettings());

            if (!climateTempResponse.IsSuccessful || climateTempResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
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
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = @"application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v5+json",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.PhevService-v1+json; charset=utf",
            };

            ApiResponse climateToken = GenerateAuthenticationToken("ECC", pin);
            IRestResponse climateResponse = PostRequest($"vehicles/{vin}/preconditioning", httpHeaders,
                new EvClimatePreconditioningSettings(climateToken["token"], startStop, targetTemperature));

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
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = @"application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v4+json",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json; charset=utf-8",
            };

            IRestResponse honkBlinkResponse = PostRequest($"vehicles/{vin}/honkBlink", httpHeaders, GenerateAuthenticationToken("HBLF", GetVinProtectedPin()));

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
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = "",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json"
            };

            IRestResponse unlockResponse = PostRequest($"vehicles/{vin}/lock", httpHeaders,
                GenerateAuthenticationToken("RDL", pin));

            if (!unlockResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error locking vehicle");
            }
        }

        /// <summary>
        /// Unlocks the vehicle
        /// </summary>
        public void Unlock(string pin)
        {
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = "",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json"
            };

            IRestResponse unlockResponse = PostRequest($"vehicles/{vin}/unlock", httpHeaders,
                GenerateAuthenticationToken("RDU", pin));
            
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
            HttpHeaders httpHeaders = new HttpHeaders
            {
                ["Accept"] = @"application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v4+json",
                ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json; charset=utf-8"
            };

            IRestResponse healthResponse = PostRequest($"vehicles/{vin}/healthstatus", httpHeaders, GenerateAuthenticationToken("VHS"));

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
            IRestResponse subscriptionResponse = GetRequest($"vehicles/{vin}/subscriptionpackages", new HttpHeaders());

            if (!subscriptionResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error getting vehicle subscriptions");
            }
        }

        /// <summary>
        /// Retrieves the next service due in miles
        /// </summary>
        public void RefreshVehicleStatusReport()
        {
            HttpHeaders httpHeaders = new HttpHeaders {["Accept"] = @"application/vnd.ngtp.org.if9.healthstatus-v2+json"};
            IRestResponse<VehicleStatusReport> vehicleStatusResponse = GetRequest<VehicleStatusReport>($"vehicles/{vin}/status", httpHeaders);

            if (!vehicleStatusResponse.IsSuccessful)
            {
                throw new InvalidOperationException("Error retrieving vehicle status report");
            }

            VehicleStatus = vehicleStatusResponse.Data;
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
            HttpHeaders httpHeaders = new HttpHeaders {["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.AuthenticateRequest-v2+json; charset=utf-8"};
            IRestResponse tokenResponse = PostRequest($"vehicles/{vin}/users/{userId}/authenticate", httpHeaders, tokenData);

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
        private string GetVinProtectedPin()
        {
            return vin.Substring(vin.Length - 4, 4);
        }

        /// <summary>
        /// Used to make Rest GET requests and re-validate the token if required
        /// </summary>
        /// <param name="httpHeaders"></param>
        /// <returns>Completed rest request</returns>
        private IRestResponse GetRequest(string url, HttpHeaders httpHeaders)
        {
            RestRequest restRequest = new RestRequest(url, Method.GET, DataFormat.Json);
            UpdateRestRequestHeaders(restRequest, httpHeaders);
            return VehicleRequestClient.Execute(restRequest);
        }

        /// <summary>
        /// Used to make Rest GET requests and re-validate the token if required
        /// </summary>
        /// <param name="httpHeaders"></param>
        /// <returns>Completed rest request</returns>
        private IRestResponse<T> GetRequest<T>(string url, HttpHeaders httpHeaders) where T : new()
        {
            RestRequest restRequest = new RestRequest(url, Method.GET, DataFormat.Json);
            UpdateRestRequestHeaders(restRequest, httpHeaders);
            return VehicleRequestClient.Execute<T>(restRequest);
        }

        /// <summary>
        /// Used to make Rest POST requests and re-validate the token if required
        /// </summary>
        /// <param name="httpHeaders"></param>
        /// <returns>Completed rest request</returns>
        private IRestResponse PostRequest(string url, HttpHeaders httpHeaders, object payloadData)
        {
            RestRequest restRequest = new RestRequest(url, Method.POST);
            UpdateRestRequestHeaders(restRequest, httpHeaders);
            restRequest.AddJsonBody(payloadData);
            return VehicleRequestClient.Execute(restRequest);
        }

        /// <summary>
        /// Used to make Rest POST requests and re-validate the token if required
        /// </summary>
        /// <param name="httpHeaders"></param>
        /// <returns>Completed rest request</returns>
        private IRestResponse<T> PostRequest<T>(string url, HttpHeaders httpHeaders, object payloadData) where T : new()
        {
            RestRequest restRequest = new RestRequest(url, Method.POST);
            UpdateRestRequestHeaders(restRequest, httpHeaders);
            restRequest.AddJsonBody(payloadData);
            return VehicleRequestClient.Execute<T>(restRequest);
        }

        /// <summary>
        /// Adds all custom headers to the RestRequest being constructed
        /// </summary>
        /// <param name="restRequest"></param>
        /// <param name="httpHeaders"></param>
        private void UpdateRestRequestHeaders(RestRequest restRequest, HttpHeaders httpHeaders)
        {
            //Add all the custom headers to the request
            foreach (string httpHeaderKey in httpHeaders.Keys)
            {
                restRequest.AddHeader(httpHeaderKey, httpHeaders[httpHeaderKey]);
            }
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