using System;
using System.Collections.Generic;
using System.Text;

namespace JlrSharp.Utils
{
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
    }
}
