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
using Alexa.NET.Security.Functions;
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

            // Validate skill request
            bool isValid = await skillRequest.ValidateRequestAsync(req, log);
            if (!isValid)
            {
                return new BadRequestResult();
            }

            Type requestType = skillRequest.GetRequestType();
            SkillResponse response = null;

            // Handle open requests
            if (requestType == typeof(LaunchRequest))
            {
                response = ResponseBuilder.Tell("Welcome to Jaguar Remote!");
                response.Response.ShouldEndSession = false;
            }

            // Handle exit requests
            else if (requestType == typeof(SessionEndedRequest))
            {
                response = ResponseBuilder.Tell("Good bye");
                response.Response.ShouldEndSession = true;
            }

            // Handle intent requests
            else if (skillRequest.Request is IntentRequest intentRequest)
            {
                AuthorisedUser authorisedUser = null;
                try
                {
                    // Grab the authenticated user from the database
                    authorisedUser = SqlQueries.GetAuthorisedUserByAccessToken(skillRequest.Session.User.AccessToken);
                    log.LogInformation($"Grabbed logged in user \"{authorisedUser.UserInfo.Email}\"");

                    log.LogInformation("Connecting to Jaguar Remote");
                    JlrSharpConnection jlrSharpConnection = new JlrSharpConnection(authorisedUser.UserInfo, authorisedUser.TokenData);

                    Vehicle vehicle = jlrSharpConnection.GetPrimaryVehicle();
                    log.LogInformation($"Using default vehicle with vin \"{vehicle.vin}\"");

                    switch (intentRequest.Intent.Name)
                    {
                        // Amazon intents
                        case "AMAZON.CancelIntent":
                        case "AMAZON.StopIntent":
                            response = ResponseBuilder.Tell("Your action has been cancelled");
                            response.Response.ShouldEndSession = true;
                            break;
                        case "AMAZON.HelpIntent":
                            response = ResponseBuilder.Tell("Try asking questions like, how much fuel remains?");
                            response.Response.ShouldEndSession = false;
                            break;
                        // Custom intents
                        case "Unlock":
                            vehicle.Unlock(authorisedUser.UserInfo.Pin);
                            response = ResponseBuilder.Tell("Unlocking the doors for 30 seconds");
                            break;
                        case "Lock":
                            vehicle.Lock(authorisedUser.UserInfo.Pin);
                            response = ResponseBuilder.Tell("Locking the doors");
                            break;
                        case "StartEngine":
                            int climateRemainingRunTime = vehicle.GetRemainingClimateRunTime();
                            string responseMessage = "Starting the engine";

                            if (climateRemainingRunTime > 0)
                            {
                                if (climateRemainingRunTime > 0 && climateRemainingRunTime <= 5)
                                {
                                    responseMessage =
                                        $"Starting the engine, but note you only have {climateRemainingRunTime} minutes remaining until the engine automatically stops";
                                }

                                vehicle.StartEngine(authorisedUser.UserInfo.Pin);
                            }
                            else
                            {
                                responseMessage =
                                    $"Remote climate cannot start as you've exceeded the maximum allowed time. Please drive the car normally to reset this";
                            }

                            response = ResponseBuilder.Tell(responseMessage);
                            break;
                        case "StopEngine":
                            vehicle.StopEngine(authorisedUser.UserInfo.Pin);
                            response = ResponseBuilder.Tell("Stopping the engine");
                            break;
                        case "ServiceDue":
                            int milesUntilService = vehicle.GetServiceDueInMiles();
                            response = ResponseBuilder.Tell(
                                $"There are {milesUntilService} miles until the next service is due");
                            break;
                        case "Mileage":
                            int mileage = vehicle.GetMileage();
                            response = ResponseBuilder.Tell($"The current mileage is {mileage}");
                            break;
                        case "FuelRange":
                            int distanceRemaining = vehicle.GetDistanceUntilEmpty();
                            response = ResponseBuilder.Tell(
                                $"There is {distanceRemaining} miles remaining until the fuel tank is empty");
                            break;
                        case "FuelPerc":
                            int fuelPerc = vehicle.GetFuelLevelPercentage();
                            response = ResponseBuilder.Tell($"The fuel tank has {fuelPerc} percent remaining");
                            break;
                        case "HonkBeep":
                            vehicle.HonkAndBlink();
                            response = ResponseBuilder.Tell("Beeping and flashing the lights");
                            break;
                        default:
                            response = ResponseBuilder.Tell("Try asking how much fuel remains by saying, how much fuel remains?");
                            response.Response.ShouldEndSession = false;
                            break;
                    }

                    SqlQueries.LogIntentUsage(authorisedUser.UserInfo.Email, intentRequest.Intent.Name, true);
                }
                catch
                {
                    if (authorisedUser != null)
                    {
                        SqlQueries.LogIntentUsage(authorisedUser.UserInfo.Email, intentRequest.Intent.Name, false);
                    }

                    response = ResponseBuilder.Tell("Please report this beta test error to Chris Davies");
                    //response = ResponseBuilder.Tell("There was an error with your request, please try again later");
                }
            }
            
            // If response is null, it means we haven't correctly handled it
            if (response == null)
            {
                return new BadRequestResult();
            }

            // This is the valid response
            return new OkObjectResult(response);
        }
    }
}
