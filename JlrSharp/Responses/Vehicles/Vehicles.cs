using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using JlrSharp.Requests;
using JlrSharp.Responses.Vehicles;
using JlrSharp.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace JlrSharp.Responses
{
    public class VehicleCollection
    {
        public List<IVehicle> Vehicles { get; set; } = new List<IVehicle>();
    }

    [Serializable]
    public abstract class Vehicle : IVehicle, IVehicleBaseFunctionality
    {
        private RestClient VehicleRequestClient { get; set; }
        private JlrSharpConnection JlrSharpConnector { get; set; } // I don't like this, but I am being lazy
        public string Model => Attributes.vehicleType;
        public string userId { get; set; }
        public string vin { get; set; }
        public string role { get; set; }
        protected VehicleAttributes Attributes { get; set; }
        protected VehicleStatusReport VehicleStatusRaw { get; set; }
        public VehicleStatus Status => new VehicleStatus(this);
        public bool AutoRefreshTokens { get; set; }
        public VehicleFuelType FuelType { get; set; }

        /// <summary>
        /// Retrieves the next service due in miles
        /// </summary>
        public int GetServiceDueInMiles()
        {
            VehicleStatusReport.VehicleStatus odometerReading = VehicleStatusRaw.vehicleStatus.First(status => status.key == "EXT_KILOMETERS_TO_SERVICE");
            return Convert.ToInt32(Convert.ToDouble(odometerReading.value) / 1.609);
        }

        /// <summary>
        /// Gets the current mileage of the vehicle
        /// </summary>
        public int GetMileage()
        {
            VehicleStatusReport.VehicleStatus odometerReading = VehicleStatusRaw.vehicleStatus.First(status => status.key == "ODOMETER_MILES");
            return Convert.ToInt32(odometerReading.value);
        }

        /// <summary>
        /// Retrieves remaining miles until empty
        /// </summary>
        /// <returns></returns>
        public abstract int GetDistanceUntilEmpty();

        /// <summary>
        /// Returns the remaining run time left for remote climate
        /// </summary>
        /// <returns></returns>
        public int GetRemainingClimateRunTime()
        {
            VehicleStatusReport.VehicleStatus remainingRunTime = VehicleStatusRaw.vehicleStatus.First(status => status.key == "CLIMATE_STATUS_REMAINING_RUNTIME");
            return Convert.ToInt32(remainingRunTime.value);
        }

        /// <summary>
        /// Returns the current state of the vehicle
        /// </summary>
        /// <returns></returns>
        public string GetVehicleStateType()
        {
            // TODO: Convert the value to meaningful data
            return (string)VehicleStatusRaw.vehicleStatus.First(status => status.key == "VEHICLE_STATE_TYPE").value;
        }

        /// <summary>
        /// Returns the door lock status
        /// </summary>
        /// <returns></returns>
        public DoorStatus GetDoorLockStatus()
        {
            // Jaguar store the pressure in Kilopascal
            DoorStatus doors = new DoorStatus
            {
                IsBonnetLocked = Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus.First(door => door.key == "DOOR_ENGINE_HOOD_LOCK_STATUS").value == "LOCKED"),
                IsFrontLeftDoorLocked = Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus.First(door => door.key == "DOOR_FRONT_LEFT_LOCK_STATUS").value == "LOCKED"),
                IsFrontRightDoorLocked = Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus.First(door => door.key == "DOOR_FRONT_RIGHT_LOCK_STATUS").value == "LOCKED"),
                IsRearLeftDoorLocked = Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus.First(door => door.key == "DOOR_REAR_LEFT_LOCK_STATUS").value == "LOCKED"),
                IsRearRightDoorLocked = Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus.First(door => door.key == "DOOR_REAR_RIGHT_LOCK_STATUS").value == "LOCKED"),
                IsBootLocked = !Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus.First(door => door.key == "DOOR_IS_BOOT_LOCKED").value == "LOCKED"),
            };

            return doors;
        }

        /// <summary>
        /// Returns the window position status
        /// </summary>
        /// <returns></returns>
        public WindowStatus GetWindowStatus()
        {
            WindowStatus windows = new WindowStatus
            {
                IsFrontLeftWindowClosed =
                    Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus
                        .First(door => door.key == "WINDOW_FRONT_LEFT_STATUS").value == "CLOSED"),
                IsFrontRightWindowClosed =
                    Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus
                        .First(door => door.key == "WINDOW_FRONT_RIGHT_STATUS").value == "CLOSED"),
                IsRearLeftWindowClosed =
                    Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus
                        .First(door => door.key == "WINDOW_REAR_LEFT_STATUS").value == "CLOSED"),
                IsRearRightWindowClosed =
                    Convert.ToBoolean((string)VehicleStatusRaw.vehicleStatus
                        .First(door => door.key == "WINDOW_REAR_RIGHT_STATUS").value == "CLOSED"),
            };

            return windows;
        }

        /// <summary>
        /// Returns the tyre pressures
        /// </summary>
        /// <returns></returns>
        public TyrePressures GetTyrePressures()
        {
            // Jaguar store the pressure in Kilopascal
            TyrePressures tyrePressures = new TyrePressures
            {
                FrontLeft = (int)(Convert.ToInt32(VehicleStatusRaw.vehicleStatus
                    .First(tyre => tyre.key == "TYRE_PRESSURE_FRONT_LEFT").value) / 6.895),
                FrontRight = (int)(Convert.ToInt32(VehicleStatusRaw.vehicleStatus
                    .First(tyre => tyre.key == "TYRE_PRESSURE_FRONT_RIGHT").value) / 6.895),
                RearLeft = (int)(Convert.ToInt32(VehicleStatusRaw.vehicleStatus
                    .First(tyre => tyre.key == "TYRE_PRESSURE_REAR_LEFT").value) / 6.895),
                RearRight = (int)(Convert.ToInt32(VehicleStatusRaw.vehicleStatus
                    .First(tyre => tyre.key == "TYRE_PRESSURE_REAR_RIGHT").value) / 6.895)
            };

            return tyrePressures;
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
        /// Determines if the vehicle is locked
        /// </summary>
        /// <returns></returns>
        public bool IsLocked()
        {
            return Convert.ToBoolean(VehicleStatusRaw.vehicleStatus.First(lockedDoors => lockedDoors.key == "DOOR_IS_ALL_DOORS_LOCKED").value);
        }

        public void GetVehicleAttributes()
        {
            HttpHeaders httpHeaders = new HttpHeaders { ["Accept"] = @"application/vnd.ngtp.org.VehicleAttributes-v4+json" };
            IRestResponse<VehicleAttributes> restResponse = GetRequest<VehicleAttributes>($"vehicles/{vin}/attributes", httpHeaders);

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Get attributes", restResponse.Content, restResponse.ErrorException);
            }

            Attributes = restResponse.Data;

            if (Attributes.fuelType.Equals("electric", StringComparison.OrdinalIgnoreCase))
            {
                FuelType = VehicleFuelType.Ev;
            }
            else
            {
                FuelType = VehicleFuelType.Gasoline;
            }
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
        public void GetVehicleStatusReport()
        {
            HttpHeaders httpHeaders = new HttpHeaders { ["Accept"] = @"application/vnd.ngtp.org.if9.healthstatus-v2+json" };
            IRestResponse<VehicleStatusReport> restResponse = GetRequest<VehicleStatusReport>($"vehicles/{vin}/status", httpHeaders);

            if (!restResponse.IsSuccessful)
            {
                throw new RequestException("Get vehicle status", restResponse.Content, restResponse.ErrorException);
            }

            VehicleStatusRaw = restResponse.Data;
        }

        /// <summary>
        /// Generate the appropriate token for the given service
        /// </summary>
        /// <param name="serviceName">The service requested</param>
        /// <param name="pin">The pin to use</param>
        /// <returns></returns>
        protected ApiResponse GenerateAuthenticationToken(string serviceName, string pin = "")
        {
            TokenData tokenData = GenerateTokenData(serviceName, pin);
            HttpHeaders httpHeaders = new HttpHeaders { ["Content-Type"] = @"application/vnd.wirelesscar.ngtp.if9.AuthenticateRequest-v2+json; charset=utf-8" };
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
        protected IRestResponse GetRequest(string url, HttpHeaders httpHeaders)
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
        protected IRestResponse<T> GetRequest<T>(string url, HttpHeaders httpHeaders) where T : new()
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
        protected IRestResponse PostRequest(string url, HttpHeaders httpHeaders, object payloadData)
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
        protected IRestResponse<T> PostRequest<T>(string url, HttpHeaders httpHeaders, object payloadData) where T : new()
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
        protected void UpdateRestRequestHeaders(RestRequest restRequest, HttpHeaders httpHeaders)
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
        }
    }
}