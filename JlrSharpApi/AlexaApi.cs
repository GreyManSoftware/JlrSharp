using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            else if (skillRequest.Request is IntentRequest intentRequest)
            {
                // Grab the authenticated user from the database
                AuthorisedUser authorisedUser = SqlQueries.GetAuthorisedUserByAccessToken(skillRequest.Session.User.AccessToken);
                log.LogInformation($"Grabbed logged in user \"{authorisedUser.UserInfo.Email}\"");

                log.LogInformation("Connecting to Jaguar Remote");
                JlrSharpConnection jlrSharpConnection = new JlrSharpConnection(authorisedUser.UserInfo, authorisedUser.TokenData);

                Vehicle vehicle = jlrSharpConnection.GetPrimaryVehicle();
                log.LogInformation($"Using default vehicle with vin \"{vehicle.vin}\"");
                
                switch (intentRequest.Intent.Name)
                {
                    case "Unlock":
                        vehicle.Unlock(authorisedUser.UserInfo.Pin);
                        response = ResponseBuilder.Tell("The doors have been unlocked for 30 seconds");
                        break;
                    case "Lock":
                        vehicle.Lock(authorisedUser.UserInfo.Pin);
                        response = ResponseBuilder.Tell("The doors have been locked");
                        break;
                    case "StartEngine":
                        vehicle.StartEngine(authorisedUser.UserInfo.Pin);
                        response = ResponseBuilder.Tell("The engine has been started");
                        break;
                    case "StopEngine":
                        vehicle.StopEngine(authorisedUser.UserInfo.Pin);
                        response = ResponseBuilder.Tell("The engine has stopped");
                        break;
                    case "ServiceDue":
                        int milesUntilService = vehicle.GetServiceDueInMiles();
                        response = ResponseBuilder.Tell($"There are {milesUntilService} miles until the next service is due");
                        break;
                    case "Mileage":
                        int mileage = vehicle.GetMileage();
                        response = ResponseBuilder.Tell($"The current mileage is {mileage}");
                        break;
                    case "FuelRange":
                        int distanceRemaining = vehicle.GetDistanceUntilEmpty();
                        response = ResponseBuilder.Tell($"There is {distanceRemaining} miles remaining until the fuel tank is empty");
                        break;
                    case "FuelPerc":
                        int fuelPerc = vehicle.GetFuelLevelPercentage();
                        response = ResponseBuilder.Tell($"The fuel tank is at {fuelPerc} percent full");
                        break;
                }
            }

            return new OkObjectResult(response);
        }
    }
}
