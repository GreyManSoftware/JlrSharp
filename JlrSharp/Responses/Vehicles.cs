using System;
using System.Collections.Generic;
using System.Linq;
using JlrSharp.Requests;
using JlrSharp.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace JlrSharp.Responses
{
    public class VehicleCollection
    {
        public List<Vehicle> Vehicles { get; set; }
    }

    [Serializable]
    public sealed class Vehicle
    {
        private RestClient VehicleRequestClient { get; set; }
        private JlrSharpConnection JlrSharpConnector { get; set; } // I don't like this, but I am being lazy
        public string userId { get; set; }
        public string vin { get; set; }
        public string role { get; set; }
        public VehicleStatusReport VehicleStatus { get; private set; }
        public bool AutoRefreshTokens { get; set; }

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
                throw new RequestException("Get Climate Settings", restResponse.Content, restResponse.ErrorException);
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

            IRestResponse restResponse = PostRequest($"vehicles/{vin}/settings", httpHeaders, new ClimateControlSettings());

            if (!restResponse.IsSuccessful || restResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                throw new RequestException("Set Climate Settings", restResponse.Content, restResponse.ErrorException);
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
            IRestResponse restResponse = PostRequest($"vehicles/{vin}/preconditioning", httpHeaders,
                new EvClimatePreconditioningSettings(climateToken["token"], startStop, targetTemperature));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Ev pre-condition", restResponse.Content, restResponse.ErrorException);
            }
        }

        /// <summary>
        /// Retrieves the next service due in miles
        /// </summary>
        public int GetServiceDueInMiles()
        {
            VehicleStatusReport.VehicleStatus odometerReading = VehicleStatus.vehicleStatus.First(status => status.key == "EXT_KILOMETERS_TO_SERVICE");
            return Convert.ToInt32(Convert.ToDouble(odometerReading.value) / 1.609);
        }

        /// <summary>
        /// Gets the current mileage of the vehicle
        /// </summary>
        public int GetMileage()
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
        /// Retrieves remaining miles until empty
        /// </summary>
        /// <returns></returns>
        public int GetDistanceUntilEmpty()
        {
            VehicleStatusReport.VehicleStatus remainingFuel = VehicleStatus.vehicleStatus.First(status => status.key == "DISTANCE_TO_EMPTY_FUEL");
            return Convert.ToInt32(Convert.ToDouble(remainingFuel.value) / 1.609);
        }

        /// <summary>
        /// Returns the remaining run time left for remote climate
        /// </summary>
        /// <returns></returns>
        public int GetRemainingClimateRunTime()
        {
            VehicleStatusReport.VehicleStatus remainingRunTime = VehicleStatus.vehicleStatus.First(status => status.key == "CLIMATE_STATUS_REMAINING_RUNTIME");
            return Convert.ToInt32(remainingRunTime.value);
        }

        /// <summary>
        /// Returns the current state of the vehicle
        /// </summary>
        /// <returns></returns>
        public string GetVehicleStateType()
        {
            //TODO: Need to translate these into human readable messages
            VehicleStatusReport.VehicleStatus vehicleStateType = VehicleStatus.vehicleStatus.First(status => status.key == "VEHICLE_STATE_TYPE");
            return vehicleStateType.ToString();
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

            IRestResponse restResponse = PostRequest($"vehicles/{vin}/honkBlink", httpHeaders, GenerateAuthenticationToken("HBLF", GetVinProtectedPin()));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Honk and blink", restResponse.Content, restResponse.ErrorException);
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

            IRestResponse restResponse = PostRequest($"vehicles/{vin}/lock", httpHeaders,
                GenerateAuthenticationToken("RDL", pin));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Lock vehicle", restResponse.Content, restResponse.ErrorException);
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

            IRestResponse restResponse = PostRequest($"vehicles/{vin}/unlock", httpHeaders,
                GenerateAuthenticationToken("RDU", pin));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Unlock vehicle", restResponse.Content, restResponse.ErrorException);
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

            IRestResponse restResponse = PostRequest($"vehicles/{vin}/healthstatus", httpHeaders, GenerateAuthenticationToken("VHS"));

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Vehicle health status", restResponse.Content, restResponse.ErrorException);
            }

            return JsonConvert.DeserializeObject<VehicleHealthReport>(restResponse.Content);
        }

        /// <summary>
        /// Retrieves the subscriptions the vehicle is enrolled in
        /// </summary>
        public void GetSubscriptions()
        {
            IRestResponse restResponse = GetRequest($"vehicles/{vin}/subscriptionpackages", new HttpHeaders());

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Get subscriptions", restResponse.Content, restResponse.ErrorException);
            }
        }

        /// <summary>
        /// Populates the vehicle status report
        /// </summary>
        public void RefreshVehicleStatusReport()
        {
            HttpHeaders httpHeaders = new HttpHeaders {["Accept"] = @"application/vnd.ngtp.org.if9.healthstatus-v2+json"};
            IRestResponse<VehicleStatusReport> restResponse = GetRequest<VehicleStatusReport>($"vehicles/{vin}/status", httpHeaders);

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Get vehicle status", restResponse.Content, restResponse.ErrorException);
            }

            VehicleStatus = restResponse.Data;
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
            IRestResponse restResponse = PostRequest($"vehicles/{vin}/users/{userId}/authenticate", httpHeaders, tokenData);

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException($"Error generating { serviceName } token", restResponse.Content, restResponse.ErrorException);
            }

            return JsonConvert.DeserializeObject<ApiResponse>(restResponse.Content);
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
            JlrSharpConnector.UpdateIfRequired(AutoRefreshTokens);
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
            JlrSharpConnector.UpdateIfRequired(AutoRefreshTokens);
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
            JlrSharpConnector.UpdateIfRequired(AutoRefreshTokens);
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
            JlrSharpConnector.UpdateIfRequired(AutoRefreshTokens);
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
        internal void SetVehicleRequestClient(RestClient vehicleRequestClient, JlrSharpConnection jlrSharpConnection)
        {
            VehicleRequestClient = vehicleRequestClient;
            JlrSharpConnector = jlrSharpConnection;
            RefreshVehicleStatusReport();
        }
    }
}