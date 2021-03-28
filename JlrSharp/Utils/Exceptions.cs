using System;
using System.Collections.Generic;
using System.Text;

namespace JlrSharp.Utils
{
    /// <summary>
    /// Thrown during login if the JLR Endpoint is unhealthy
    /// </summary>
    public class AuthenticationNetworkErrorException : Exception
    {
        public AuthenticationNetworkErrorException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown if an account has no vehicles associated with it
    /// </summary>
    public class NoVehiclesOnAccountException : Exception
    {
    }

    /// <summary>
    /// Thrown if a vehicle has an unidentified fuel type
    /// </summary>
    public class UnknownFuelTypeException : Exception
    {
    }

    /// <summary>
    /// Thrown if an incorrect pin is provided
    /// </summary>
    public class InvalidPinException : RequestException
    { 
        public InvalidPinException(string apiCommand, string errorMessage, Exception restRequestException) : base(apiCommand, errorMessage, restRequestException)
        {
        }
    }

    /// <summary>
    /// Thrown if a duplicate command is executed
    /// </summary>
    public class ServiceAlreadyStartedException : RequestException
    {
        public ServiceAlreadyStartedException(string apiCommand, string errorMessage, Exception restRequestException) : base(apiCommand, errorMessage, restRequestException)
        {
        }
    }

    /// <summary>
    /// Thrown when the JLR Endpoint returns an error
    /// </summary>
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

        /// <summary>
        /// Used to generate the appropriate <see cref="RequestException"/>
        /// </summary>
        /// <param name="apiCommand">The API path being called</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="restRequestException">The request exception</param>
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
