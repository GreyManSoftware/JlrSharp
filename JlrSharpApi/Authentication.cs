using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using GreyMan.JlrSharp;
using GreyMan.JlrSharp.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JlrSharpApi
{
    public static class Authentication
    {
        public static Dictionary<string, AuthenticatedUsers> UserInfoByEmail =
            new Dictionary<string, AuthenticatedUsers>();

        public static Dictionary<string, AuthenticatedUsers> UserInfoByAuthToken =
            new Dictionary<string, AuthenticatedUsers>();

        public static Dictionary<string, AuthenticatedUsers> UserInfoByAccessToken =
            new Dictionary<string, AuthenticatedUsers>();

        [FunctionName("Login")]
        public static async Task<IActionResult> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Processing login request");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            LoginDetails loginDetails = JsonConvert.DeserializeObject<LoginDetails>(requestBody);

            try
            {
                JlrSharpConnection jlrSharp = new JlrSharpConnection(loginDetails.EmailAddress, loginDetails.Password);

                // Grab successfully authenticated user details and store it (this should be SQL'd up)
                UserDetails userDetails = jlrSharp.GetAuthenticatedUserDetails();
                userDetails.Pin = loginDetails.PinCode;
                TokenStore tokenStore = jlrSharp.GetAuthenticationTokens();
                UserInfoByEmail[userDetails.Email] = new AuthenticatedUsers(userDetails, tokenStore);
                UserInfoByAuthToken[tokenStore.authorization_token] = UserInfoByEmail[userDetails.Email];
                UserInfoByAccessToken[tokenStore.access_token] = UserInfoByEmail[userDetails.Email];

                return new OkObjectResult(new Dictionary<string, string>
                { ["authorization_token"] = HttpUtility.UrlEncode(tokenStore.authorization_token) });
            }
            catch (AuthenticationException e)
            {
                return new BadRequestObjectResult("Email address or password not recognised");
            }
        }

        [FunctionName("RequestAccessToken")]
        public static async Task<IActionResult> RequestAccessToken(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Processing access token request");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Dictionary<string, string> queryParams = ParseQueryToDictionary(requestBody);

            // Determine if this is a new key request
            if (queryParams.ContainsKey("grant_type") && queryParams["grant_type"] == "authorization_code")
            {
                if (queryParams.ContainsKey("code"))
                {
                    string authCode = HttpUtility.UrlDecode(queryParams["code"]);
                    AuthenticatedUsers accessDetails = UserInfoByAuthToken[authCode];

                    // Create bespoke Alexa response (not sure if this is actually required)
                    Dictionary<string, string> alexaResponse = new Dictionary<string, string>
                    {
                        ["access_token"] = accessDetails.TokenStore.access_token,
                        ["token_type"] = accessDetails.TokenStore.token_type,
                        ["expires_in"] = accessDetails.TokenStore.expires_in,
                        ["refresh_token"] = accessDetails.TokenStore.refresh_token,
                    };

                    return new OkObjectResult(alexaResponse);
                }
            }

            try
            {
                return new OkObjectResult("data");
            }
            catch (AuthenticationException e)
            {
                return new BadRequestObjectResult("Email address or password not recognised");
            }
        }

        /// <summary>
        /// Converts an HTTP url encoded query string into a Dictionary
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        private static Dictionary<string, string> ParseQueryToDictionary(string queryString)
        {
            string[] queryParams = queryString.Split('&');
            Dictionary<string, string> queryParamDictionary = new Dictionary<string, string>();

            if (queryParams.Length % 2 != 0)
            {
                throw new ArgumentException("Query parameter string is invalid");
            }

            for (int x = 0; x < queryParams.Length; x++)
            {
                string[] keyValues = queryParams[x].Split('=');
                
                if (keyValues.Length != 2)
                {
                    throw new ArgumentException("Query parameter values are invalid");
                }

                queryParamDictionary.Add(keyValues[0], keyValues[1]);
            }

            return queryParamDictionary;
        }
    }

    [Serializable]
    public class LoginDetails
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string PinCode { get; set; }
    }

    [Serializable]
    public class AuthenticatedUsers
    {
        public UserDetails UserDetails { get; set; }
        public TokenStore TokenStore { get; set; }

        public AuthenticatedUsers(UserDetails userDetails, TokenStore tokenStore)
        {
            UserDetails = userDetails;
            TokenStore = tokenStore;
        }
    }
}
