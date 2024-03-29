﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using JlrSharp.Responses;
using JlrSharp.Responses.Vehicles;
using JlrSharp.Utils;
using Newtonsoft.Json;
using RestSharp;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace JlrSharp
{
    /// <summary>
    /// The interface for the JLR InControl connectivity
    /// </summary>
    public sealed class JlrSharpConnection
    {
        // Base Urls
        private static readonly Uri IfasBaseUrl = new Uri("https://ifas.prod-row.jlrmotor.com/ifas/jlr"); // Used for generating tokens
        private static readonly Uri IfopBaseUrl = new Uri("https://ifop.prod-row.jlrmotor.com/ifop/jlr"); // Used for registering devices
        private static readonly Uri If9BaseUrl = new Uri("https://if9.prod-row.jlrmotor.com/if9/jlr"); // Used for vehicle requests

        // Rest Clients
        private readonly RestClient _authClient = new RestClient(IfasBaseUrl);
        private readonly RestClient _deviceClient = new RestClient(IfopBaseUrl);
        private readonly RestClient _vehicleClient = new RestClient(If9BaseUrl);

        // User details
        private readonly UserDetails _userInfo;

        // Authentication details
        private TokenStore _tokens;
        private OAuth _oAuth; // This can't be readonly if we build reconnect()

        // Associated vehicles
        private VehicleCollection _vehicles;

        /// <summary>
        /// Allows you to specify if the API should handle refreshing tokens
        /// </summary>
        public bool AutoRefreshTokens { get; set; }

        /// <summary>
        /// Constructs a JlrSharp object using the specified credentials
        /// </summary>
        /// <param name="email">Your email address</param>
        /// <param name="password">Your plaintext password</param>
        public JlrSharpConnection(string email, string password)
        {
            _userInfo = new UserDetails
            {
                Email = email,
                Password = Convert.ToBase64String(Encoding.ASCII.GetBytes(password)),
                DeviceId = Validators.GenerateDeviceId(null),
            };

            // Construct a oauth token using the provided credentials
            _oAuth = new OAuth
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password
            };

            Connect(_oAuth);
            // TODO: Have a thread that refreshes the tokens?
        }

        /// <summary>
        /// Constructs a JlrSharp object using an existing refresh token and email combo
        /// </summary>
        /// <param name="email"></param>
        /// <param name="refreshToken"></param>
        /// <param name="deviceId"></param>
        public JlrSharpConnection(string email, string refreshToken, string deviceId)
        {
            _userInfo = new UserDetails
            {
                Email = email,
                DeviceId = Validators.GenerateDeviceId(deviceId),
            };

            _oAuth = new OAuth
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };

            Connect(_oAuth);
        }

        public JlrSharpConnection(UserDetails userDetails, TokenStore tokenStore)
        {
            _userInfo = userDetails;
            _tokens = tokenStore;

            // We will need this token later on for re-generating the tokens
            _oAuth = new OAuth
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = tokenStore.refresh_token
            };

            ConnectWithExistingCreds();
        }

        /// <summary>
        /// Sets up a connection using already generated credentials
        /// </summary>
        private void ConnectWithExistingCreds()
        {
            Trace.TraceInformation($"Connecting device ID \"{_userInfo.DeviceId}\"");

            // Add some default headers
            _authClient.AddDefaultHeader("X-Device-Id", _userInfo.DeviceId.ToString());
            _authClient.AddDefaultHeader("Connection", "close");
            _deviceClient.AddDefaultHeader("X-Device-Id", _userInfo.DeviceId.ToString());
            _deviceClient.AddDefaultHeader("Connection", "close");
            _vehicleClient.AddDefaultHeader("X-Device-Id", _userInfo.DeviceId.ToString());
            _vehicleClient.AddDefaultHeader("Connection", "close");

            // Configure the access tokens for connections
            _deviceClient.AddDefaultHeader("Authorization", $"Bearer {_tokens.access_token}");
            _vehicleClient.AddDefaultHeader("Authorization", $"Bearer {_tokens.access_token}");

            // Grab all associated vehicles
            GetVehicles();
        }

        /// <summary>
        /// Connects and retrieve the auth token, which is required for future operations
        /// </summary>
        private void Connect(OAuth oAuthToken)
        {
            Trace.TraceInformation($"Connecting device ID \"{_userInfo.DeviceId}\"");

            // Add some default headers
            _authClient.AddDefaultHeader("X-Device-Id", _userInfo.DeviceId.ToString());
            _authClient.AddDefaultHeader("Connection", "close");
            _deviceClient.AddDefaultHeader("X-Device-Id", _userInfo.DeviceId.ToString());
            _deviceClient.AddDefaultHeader("Connection", "close");
            _vehicleClient.AddDefaultHeader("X-Device-Id", _userInfo.DeviceId.ToString());
            _vehicleClient.AddDefaultHeader("Connection", "close");

            // Authenticate
            RestRequest loginRequest = new RestRequest("tokens", Method.POST, DataFormat.Json);
            loginRequest.AddHeader("Authorization", "Basic YXM6YXNwYXNz");
            loginRequest.AddJsonBody(oAuthToken);
            IRestResponse<TokenStore> response = _authClient.Execute<TokenStore>(loginRequest);

            if (!response.IsSuccessful)
            {
                // Check for indications of email/password issues
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new AuthenticationException("Error authenticating with OAuth Token");

                }

                // Network failure of some kind
                throw new AuthenticationNetworkErrorException($"{response.StatusDescription} - {response.StatusCode}");
            }

            _tokens = response.Data;

            Trace.TraceInformation("Authentication complete");

            // Configure the access tokens for connections
            _deviceClient.AddDefaultHeader("Authorization", $"Bearer {_tokens.access_token}");
            _vehicleClient.AddDefaultHeader("Authorization", $"Bearer {_tokens.access_token}");

            // Register device
            RegisterDevice();

            // Log in the user
            LoginUser();

            // Grab all associated vehicles
            GetVehicles();

            //#if DEBUG
            //            DumpData();
            //#endif
        }

        /// <summary>
        /// Only exists in debug builds to dump user data for testing
        /// </summary>
        private void DumpData()
        {
            // Sort out temp output dir
            string dataDir = Path.Combine(Path.GetTempPath(), "JlrSharp", _userInfo.Email);

            if (Directory.Exists(dataDir))
            {
                Directory.Delete(dataDir, true);
            }

            Directory.CreateDirectory(dataDir);

            // Pretty print, as these are for human reading
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // Dump objects as JSON
            File.WriteAllText(Path.Combine(dataDir, "userInfo.json"), JsonSerializer.Serialize(_userInfo, options));
            File.WriteAllText(Path.Combine(dataDir, "tokens.json"), JsonSerializer.Serialize(_tokens, options));
            File.WriteAllText(Path.Combine(dataDir, "vehicles.json"), JsonSerializer.Serialize(_vehicles, options));
        }

        /// <summary>
        /// Returns the credentials of the currently authorised user
        /// </summary>
        /// <returns></returns>
        public AuthorisedUser GetAuthorisedUser()
        {
            return new AuthorisedUser { UserInfo = _userInfo, TokenData = _tokens };
        }

        /// <summary>
        /// Registers the device id with the service
        /// </summary>
        private void RegisterDevice()
        {
            RestRequest deviceRegistrationRequest = new RestRequest($"users/{_userInfo.Email}/clients", Method.POST, DataFormat.Json);

            Dictionary<string, string> deviceRegistrationData = new Dictionary<string, string>
            {
                ["access_token"] = _tokens.access_token,
                ["authorization_token"] = _tokens.authorization_token,
                ["expires_in"] = "86400",
                ["deviceID"] = _userInfo.DeviceId.ToString(),
            };

            deviceRegistrationRequest.AddJsonBody(deviceRegistrationData);
            _userInfo.DeviceIdExpiry = DateTime.UtcNow.AddSeconds(86400);
            IRestResponse registrationResponse = _deviceClient.Execute(deviceRegistrationRequest);

            // HTTP Response code 204 indicates success
            if (!registrationResponse.IsSuccessful || registrationResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException("Error registering device");
            }
        }

        /// <summary>
        /// Logs in with the specified email address and saves the user_id
        /// </summary>
        private void LoginUser()
        {
            RestRequest loginRequest = new RestRequest($"users/?loginName={_userInfo.Email}", Method.GET, DataFormat.Json);
            IRestResponse<LoginResponse> response = _vehicleClient.Execute<LoginResponse>(loginRequest);

            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException("Error logging in user");
            }

            LoginResponse loginResponse = response.Data;
            _userInfo.UserId = loginResponse.userId;
        }

        /// <summary>
        /// Retrieves the default/primary vehicle associated to the user
        /// </summary>
        public Vehicle GetPrimaryVehicle()
        {
            return (Vehicle)_vehicles.Vehicles.First(v => v.role == "Primary");
        }

        /// <summary>
        /// Retrieves all vehicles associated to the user
        /// </summary>
        ///  TODO: C'Tor needs to take a VIN/Identifier of some type to allow which car to pick from the list. For now we will take the primary
        private void GetVehicles()
        {
            RestRequest getVehiclesRequest = new RestRequest($"users/{_userInfo.UserId}/vehicles?primaryOnly=false", Method.GET, DataFormat.Json);
            IRestResponse response = _vehicleClient.Execute(getVehiclesRequest);

            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException("Error retrieving associated vehicles");
            }

            DummyVehicleCollection vehicleData = JsonConvert.DeserializeObject<DummyVehicleCollection>(response.Content);

            if (vehicleData.vehicles == null)
            {
                throw new NoVehiclesOnAccountException();
            }

            _vehicles = new VehicleCollection();

            // Create specific vehicle classes depending on the fuel type
            foreach (DummyVehicle vehicle in vehicleData.vehicles)
            {
                vehicle.SetVehicleRequestClient(_vehicleClient, this);
                vehicle.AutoRefreshTokens = AutoRefreshTokens;

                // Convert the dummy vehicle into the correct type
                Vehicle specificVehicle = Vehicle.CreateVehicle(vehicle);

                Trace.TraceInformation($"Processing vehicle model \"{vehicle.Model} - {vehicle.FuelType.ToString().ToUpperInvariant()}\"");
                specificVehicle.SetVehicleRequestClient(_vehicleClient, this);
                _vehicles.Vehicles.Add(specificVehicle);
            }
        }

        /// <summary>
        /// Updates the tokens if required
        /// </summary>
        /// <returns>Returns true if the tokens were refreshed</returns>
        public bool UpdateIfRequired(bool performRefresh)
        {
            // If the token has more than 15 mins remaining, it's fine
            if (_tokens.ExpirationTime >= DateTime.UtcNow.AddMinutes(15))
            {
                return false;
            }

            if (!performRefresh)
            {
                return false;
            }

            // Try to refresh the token using refresh token or username and password
            try
            {
                Connect(_oAuth); // The last _oAuth token should be a refresh token
            }
            catch (AuthenticationException)
            {
                Trace.TraceWarning("Re-authentication failed. Attempting username and password authentication");
                try
                {
                    OAuth userNamePassword = new OAuth
                    {
                        ["grant_type"] = "password",
                        ["username"] = _userInfo.Email,
                        ["password"] = _userInfo.Password,
                    };

                    Connect(userNamePassword);
                }
                catch
                {
                    Trace.TraceError("Fatal re-authentication error with username password");
                    throw;
                }
            }

            return true;
        }
    }
}

/// <summary>
/// Used to deserialise the data back from Jaguar so we can determine the vehicle type
/// </summary>
internal sealed class DummyVehicleCollection
{
    public List<DummyVehicle> vehicles { get; set; }
}