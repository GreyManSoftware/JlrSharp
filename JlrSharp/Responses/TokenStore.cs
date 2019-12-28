using System;
using System.Collections.Generic;
using System.Text;
using RestSharp.Deserializers;

namespace GreyMan.JlrSharp.Responses
{
    [Serializable]
    public class TokenStore
    {
        [DeserializeAs(Name = "access_token")]
        public string AccessToken { get; set; }
        [DeserializeAs(Name = "authorization_token")]
        public string AuthorizationToken { get; set; }
        [DeserializeAs(Name = "expires_in")]
        public string ExpiresIn { get; set; }
        [DeserializeAs(Name = "refresh_token")]
        public string RefreshToken { get; set; }
        [DeserializeAs(Name = "token_type")]
        public string TokenType { get; set; }
        
        private DateTime _timeOfCreation = DateTime.Now;
        public DateTime ExpirationTime => _timeOfCreation.AddSeconds(Convert.ToInt32(ExpiresIn));
    }
}
