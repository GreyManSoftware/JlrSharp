using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using RestSharp;

using JlrSharpFrontend.Utils;

namespace JlrSharpFrontend.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public ActionResult OnPost()
        {
            // Grab user credentials
            string emailAddress = Request.Form["inputEmail"];
            string password = Request.Form["inputPassword"];
            string pinCode = Request.Form["inputPin"];

            // Grab Alexa information
            string redirect_uri;
            string client_id;
            string response_type;
            string scope;
            string state;

            Request.Query.TryGetParamAsString("redirect_uri", out redirect_uri);
            Request.Query.TryGetParamAsString("client_id", out client_id);
            Request.Query.TryGetParamAsString("response_type", out response_type);
            Request.Query.TryGetParamAsString("scope", out scope);
            Request.Query.TryGetParamAsString("state", out state);

            // Construct authentication object
            Dictionary<string, string> loginDetails = new Dictionary<string, string>
            {
                [nameof(emailAddress)] = emailAddress,
                [nameof(password)] = password,
                [nameof(pinCode)] = pinCode,
            };

            // Post these to the API
            //RestClient restClient = new RestClient("http://localhost:7071");
            RestClient restClient = new RestClient("https://aa1b1d41.ngrok.io");
      
            RestRequest loginRequest = new RestRequest("api/Login", Method.POST, DataFormat.Json);
            loginRequest.AddJsonBody(loginDetails);
            IRestResponse response = restClient.Execute(loginRequest);

            if (!response.IsSuccessful)
            {
                return new RedirectToPageResult("Error");
            }

            Dictionary<string, string> authToken = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Content);

            // Request came from Alexa, so lets form the correct response
            if (!String.IsNullOrEmpty(redirect_uri) && !String.IsNullOrEmpty(client_id))
            {
                return new RedirectResult($"{redirect_uri}?state={state}&code={authToken["authorization_token"]}");
            }

            // Request came from somewhere else, lets deal with that
            return new RedirectToPageResult("Index"); // TODO: Change this to be a success page or something
        }
    }
}
