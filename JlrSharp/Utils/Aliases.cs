using System;
using System.Collections.Generic;
using System.Text;

namespace GreyMan.JlrSharp.Utils
{
    /// <summary>
    /// A wrapper for the OAuth token
    /// </summary>
    public class Oauth : Dictionary<string, string>
    {
    }

    /// <summary>
    /// This is used to wrap most of the responses from the API
    /// </summary>
    public class ApiResponse : Dictionary<string, string>
    {
    }
}
