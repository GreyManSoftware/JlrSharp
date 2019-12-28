using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
        private RestClient _vehicleClient = new RestClient(If9BaseUrl);

        // User details
        private string _email;
        private string _password;
        private string _pin;

        // Authentication details
        private Guid _deviceId;
        private TokenStore _tokens;
        private Oauth _oauth;

        /// <summary>
        /// Constructs a JLRSharp object using the specified credentials
        /// </summary>
        /// <param name="email">Your email address</param>
        /// <param name="password">Your plaintext password</param>
        /// <param name="deviceId">A device ID. If null, one will be generated for you</param>
        public JlrSharpConnection(string email, string password, string deviceId = null)
        {
            _email = email;
            _password = Convert.ToBase64String(Encoding.ASCII.GetBytes(password));
            _deviceId = Validators.GenerateDeviceId(deviceId);

            // Construct a oauth token using the provided credentials
            _oauth = new Oauth
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password
            };
        }

        /// <summary>
        /// Connects and retrieve the auth token, which is required for future operations
        /// </summary>
        public void Connect()
        {
            Trace.TraceInformation("Connecting...");

            RestClient authClient = new RestClient(IfasBaseUrl);
            Authenticate(authClient);

            // Modify _vehicleRequest to take new headers
            _vehicleClient.AddDefaultHeader("Authorization", $"");
        }

        /// <summary>
        /// Authenticate using hardcoded credentials
        /// </summary>
        /// <param name="authClient"></param>
        /// <returns></returns>
        private void Authenticate(RestClient authClient)
        {
            RestRequest loginRequest = new RestRequest("tokens", Method.POST, DataFormat.Json);
            loginRequest.AddHeader("Authorization", "Basic YXM6YXNwYXNz");
            loginRequest.AddHeader("X-Device-Id", _deviceId.ToString());
            loginRequest.AddHeader("Connection", "close");
            loginRequest.AddJsonBody(_oauth);
            IRestResponse<TokenStore> response = authClient.Execute<TokenStore>(loginRequest);
            TokenStore apiResponse = response.Data;

            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException("Error authenticating");
            }
        }

        // This should override the oauth token using the refresh token?
        public void RefreshToken()
        {
            throw new NotImplementedException();
        }
    }
}