using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
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
                // Log in to the API
                JlrSharpConnection jlrSharp = new JlrSharpConnection(loginDetails.EmailAddress, loginDetails.Password);

                // Grab successfully authenticated user details and store it (this should be SQL'd up)
                AuthorisedUser authorisedUser = jlrSharp.GetAuthorisedUser();
                UserDetails userDetails = authorisedUser.UserInfo;
                userDetails.Pin = loginDetails.PinCode;
                TokenStore tokenStore = authorisedUser.TokenData; ;

                // Determine if user already exists - they shouldn't, as you should only get here when linking for the first time
                if (SqlQueries.GetAuthorisedUserByEmail(userDetails.Email) != null)
                {
                    log.LogWarning($"Deleting old user \"{userDetails.Email}\"");
                    // Delete old user
                    SqlQueries.DeleteUserByEmail(authorisedUser.UserInfo.Email);
                }

                // Add our new user to the database
                if (SqlQueries.InsertNewUser(userDetails, tokenStore))
                {
                    log.LogInformation($"Successfully added user into database for \"{userDetails.Email}\"");
                }

                // Return the authorization_token for the first part of the OAuth2 set-up
                return new OkObjectResult(new Dictionary<string, string>
                    {["authorization_token"] = HttpUtility.UrlEncode(tokenStore.authorization_token)});
            }
            catch (AuthenticationException e)
            {
                return new BadRequestObjectResult("Incorrect email address and/or password");
            }
        }

        /// <summary>
        /// Deals with returning or refreshing access token
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("RequestAccessToken")]
        public static async Task<IActionResult> RequestAccessToken(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Dictionary<string, string> queryParams = ParseQueryToDictionary(requestBody);

            // Determine if this is a new key request
            if (queryParams.ContainsKey("grant_type") && queryParams["grant_type"] == "authorization_code")
            {
                log.LogInformation("Processing access token request");

                string authorisationToken = HttpUtility.UrlDecode(queryParams["code"]); // The auth code is Base64 and so is UrlEncoded
                AuthorisedUser authorisedUser = SqlQueries.GetAuthorisedUserByAuthorisationToken(authorisationToken);

                if (authorisedUser == null)
                {
                    return new BadRequestObjectResult("User could not be identified by authorisation code");
                }

                // Create bespoke Alexa response (not sure if we could just return everything)
                Dictionary<string, string> alexaResponse = new Dictionary<string, string>
                {
                    ["access_token"] = authorisedUser.TokenData.access_token,
                    ["token_type"] = authorisedUser.TokenData.token_type,
                    ["expires_in"] = authorisedUser.TokenData.expires_in,
                    ["refresh_token"] = authorisedUser.TokenData.refresh_token
                };

                return new OkObjectResult(alexaResponse);
            }

            // Deal with re-generating the access token
            if (queryParams.ContainsKey("refresh_token"))
            {
                string refreshToken = queryParams["refresh_token"];
                AuthorisedUser authorisedUser = SqlQueries.GetAuthorisedUserByRefreshToken(refreshToken);

                if (authorisedUser == null)
                {
                    return new BadRequestObjectResult("User could not be identified by refresh token");
                }

                // This c'tor causes the tokens to be refreshed
                JlrSharpConnection jlrSharpConnection = new JlrSharpConnection(authorisedUser.UserInfo.Email,
                    refreshToken, authorisedUser.UserInfo.DeviceId.ToString());

                // Delete old user
                SqlQueries.DeleteUserByEmail(authorisedUser.UserInfo.Email);

                // Insert user with updated details
                authorisedUser = jlrSharpConnection.GetAuthorisedUser();
                SqlQueries.InsertNewUser(authorisedUser.UserInfo, authorisedUser.TokenData);

                // Create bespoke Alexa response (not sure if we could just return everything)
                Dictionary<string, string> alexaResponse = new Dictionary<string, string>
                {
                    ["access_token"] = authorisedUser.TokenData.access_token,
                    ["token_type"] = authorisedUser.TokenData.token_type,
                    ["expires_in"] = authorisedUser.TokenData.expires_in,
                    ["refresh_token"] = authorisedUser.TokenData.refresh_token
                };

                return new OkObjectResult(alexaResponse);
            }
            
            return new BadRequestObjectResult("A fatal error occured generating the access token");
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
}
