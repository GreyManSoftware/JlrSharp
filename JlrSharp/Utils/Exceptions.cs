using System;
using System.Collections.Generic;
using System.Text;

namespace JlrSharp.Utils
{
    public class AuthenticationNetworkError : Exception
    {
        public AuthenticationNetworkError(string message) : base(message)
        {
        }
    }
    
    public class InvalidPinException : RequestException
    { 
        public InvalidPinException(string apiCommand, string errorMessage, Exception restRequestException) : base(apiCommand, errorMessage, restRequestException)
        {
        }
    }

    public class ServiceAlreadyStartedException : RequestException
    {
        public ServiceAlreadyStartedException(string apiCommand, string errorMessage, Exception restRequestException) : base(apiCommand, errorMessage, restRequestException)
        {
        }
    }

    public class RequestException : Exception
    {
        public string ApiCommand { get; set; }
        public string ErrorMessage { get; set; }
        public Exception RestRequestException { get; set; }

        public RequestException(string apiCommand, string errorMessage, Exception restRequestException)
        {
            ApiCommand = apiCommand;
            ErrorMessage = errorMessage;
            RestRequestException = restRequestException;
        }

        public static void GenerateRequestException(string apiCommand, string errorMessage, Exception restRequestException)
        {
            // Service already started
            if (errorMessage == @"{""errorLabel"":""ServiceAlreadyStarted"",""errorDescription"":""Service is already started""}")
            {
                throw new ServiceAlreadyStartedException(apiCommand, errorMessage, restRequestException);
            }

            // Invalid pin
            if (errorMessage.StartsWith(@"{""errorLabel"":""InvalidCredentials"""))
            {
                throw new InvalidPinException(apiCommand, errorMessage, restRequestException);
            }

            RequestException.GenerateRequestException(apiCommand, errorMessage, restRequestException);
        }
    }
}
