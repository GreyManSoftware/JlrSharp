using System;
using System.Collections.Generic;
using System.Text;
using RestSharp.Deserializers;

namespace GreyMan.JlrSharp.Responses
{
    [Serializable]
    public sealed class TokenStore
    {
        public string access_token { get; set; }
        public string authorization_token { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        
        private DateTime _timeOfCreation = DateTime.Now;
        public DateTime ExpirationTime => _timeOfCreation.AddSeconds(Convert.ToInt32(expires_in));
    }
}
