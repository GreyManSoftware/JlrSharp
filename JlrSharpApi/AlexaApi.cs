using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
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
    public static class AlexaApi
    {
        [FunctionName("Alexa")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            string json = await req.ReadAsStringAsync();
            SkillRequest skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);
            Type requestType = skillRequest.GetRequestType();
            SkillResponse response = null;

            if (requestType == typeof(LaunchRequest))
            {
                response = ResponseBuilder.Tell("Welcome to Jaguar Remote!");
                response.Response.ShouldEndSession = false;
            }
            else if (requestType == typeof(IntentRequest))
            {
                // Retrieve user account info
                AuthenticatedUsers authenticatedUser = Authentication.UserInfoByAuthToken[skillRequest.Session.User.AccessToken];

                IntentRequest intentRequest = skillRequest.Request as IntentRequest;

                if (intentRequest.Intent.Name == "Unlock")
                {
                    
                }
            }

            return new OkObjectResult(response);
        }
    }
}
