using System;
using System.Collections.Generic;
using System.Text;

namespace GreyMan.JlrSharp
{
    [Serializable]
    public class TokenStore
    {
        public string access_token { get; set; }
        public string authorization_token { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
    }
}
