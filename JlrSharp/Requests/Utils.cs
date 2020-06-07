using System;
using System.Collections.Generic;
using System.Text;

namespace JlrSharp.Requests
{
    /// <summary>
    /// This is used by multiple request objects
    /// </summary>
    public class ServiceParameter
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}
