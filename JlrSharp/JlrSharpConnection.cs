using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GreyMan.JlrSharp.Responses;
using GreyMan.JlrSharp.Utils;
using RestSharp;

namespace GreyMan.JlrSharp
{
    /// <summary>
    /// The interface for the JLR InControl connectivity
    /// </summary>
    public class JlrSharpConnection
    {
        // Base Urls
        private static readonly Uri IfasBaseUrl = new Uri("https://ifas.prod-row.jlrmotor.com/ifas/jlr"); // Used for generating tokens
        private static readonly Uri IfopBaseUrl = new Uri("https://ifop.prod-row.jlrmotor.com/ifop/jlr"); // Used for registering devices
        private static readonly Uri If9BaseUrl = new Uri("https://if9.prod-row.jlrmotor.com/if9/jlr"); // Used for vehicle requests

        // Rest Clients
        private RestClient _authClient = new RestClient(IfasBaseUrl);
        private RestClient _deviceClient = new RestClient(IfopBaseUrl);
        private RestClient _vehicleClient = new RestClient(If9BaseUrl);

        // User details
        private string _email;
        private string _password;
        private string _pin;
        private string _userId;

        // Authentication details
        private Guid _deviceId;
        private TokenStore _tokens;
        private Oauth _oauth;

        // TODO: Add Vehicles here

        /// <summary>
        /// Constructs a JLRSharp object using the specified credentials
        /// </summary>
        /// <param name="email">Your email address</param>
        /// <param name="password">Your plaintext password</param>
        public JlrSharpConnection(string email, string password)
        {
            _email = email;
            _password = Convert.ToBase64String(Encoding.ASCII.GetBytes(password));
            _deviceId = Validators.GenerateDeviceId(null);

            // Construct a oauth token using the provided credentials
            _oauth = new Oauth
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password
            };
        }

        public JlrSharpConnection(string email, string refreshToken, string deviceId = null)
        {
            _email = email;
            _deviceId = Validators.GenerateDeviceId(deviceId);
            _oauth = new Oauth
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };
        }

        /// <summary>
        /// Connects and retrieve the auth token, which is required for future operations
        /// </summary>
        public void Connect()
        {
            Trace.TraceInformation("Connecting...");

            // Add some default headers
            _authClient.AddDefaultHeader("X-Device-Id", _deviceId.ToString());
            _authClient.AddDefaultHeader("Connection", "close");
            _deviceClient.AddDefaultHeader("X-Device-Id", _deviceId.ToString());
            _deviceClient.AddDefaultHeader("Connection", "close");
            _vehicleClient.AddDefaultHeader("X-Device-Id", _deviceId.ToString());
            _vehicleClient.AddDefaultHeader("Connection", "close");

            Authenticate();

            Trace.TraceInformation("Authentication complete");

            // Configure the access tokens for connections
            _deviceClient.AddDefaultHeader("Authorization", $"Bearer {_tokens.AccessToken}");
            _vehicleClient.AddDefaultHeader("Authorization", $"Bearer {_tokens.AccessToken}");

            // Register device
            RegisterDevice();

            // Log in the user
            LoginUser();
        }

        /// <summary>
        /// Authenticate using hardcoded credentials
        /// </summary>
        private void Authenticate()
        {
            RestRequest loginRequest = new RestRequest("tokens", Method.POST, DataFormat.Json);
            loginRequest.AddHeader("Authorization", "Basic YXM6YXNwYXNz");
            loginRequest.AddJsonBody(_oauth);
            IRestResponse<TokenStore> response = _authClient.Execute<TokenStore>(loginRequest);
            _tokens = response.Data;

            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException("Error authenticating");
            }
        }

        /// <summary>
        /// Registers the device id with the service
        /// </summary>
        private void RegisterDevice()
        {
            RestRequest deviceRegistrationRequest = new RestRequest($"users/{_email}/clients", Method.POST, DataFormat.Json);
            Dictionary<string, string> deviceRegistrationData = new Dictionary<string, string>
            {
                ["access_token"] = _tokens.AccessToken,
                ["authorization_token"] = _tokens.AuthorizationToken,
                ["expires_in"] = "86400",
                ["deviceID"] = _deviceId.ToString(),
            };
            deviceRegistrationRequest.AddJsonBody(deviceRegistrationData);
            IRestResponse registrationResponse =_deviceClient.Execute(deviceRegistrationRequest);

            // HTTP Response code 204 indicates success
            if (!registrationResponse.IsSuccessful && registrationResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException("Error registering device");
            }
        }

        /// <summary>
        /// Logs in with the specified email address and saves the user_id
        /// </summary>
        private void LoginUser()
        {
            RestRequest loginRequest = new RestRequest($"users/?loginName={_email}", Method.GET, DataFormat.Json);
            IRestResponse<LoginResponse> response = _vehicleClient.Execute<LoginResponse>(loginRequest);
            LoginResponse loginResponse = response.Data;

            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException("Error logging in user");
            }
        }

        // This should override the oauth token using the refresh token?
        public void RefreshToken()
        {
            throw new NotImplementedException();
        }
    }
}