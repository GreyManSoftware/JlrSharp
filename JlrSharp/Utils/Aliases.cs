using System.Collections.Generic;

namespace JlrSharp.Utils
{
    /// <summary>
    /// A wrapper for the OAuth token
    /// </summary>
    public class OAuth : Dictionary<string, string>
    {
    }

    /// <summary>
    /// This is used to wrap most of the responses from the API
    /// </summary>
    public class ApiResponse : Dictionary<string, string>
    {
    }

    /// <summary>
    /// This is used to wrap the data for additional token requests such as VHS, ECC, CP, RDL etc...
    /// </summary>
    public class TokenData : Dictionary<string, string>
    {
    }

    /// <summary>
    /// This is used to wrap HTTP headers
    /// </summary>
    public class HttpHeaders : Dictionary<string, string>
    {
    }
}
