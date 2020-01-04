using System;
using System.Collections.Generic;
using System.Text;

namespace GreyMan.JlrSharp
{
    public class AuthorisedUser
    {
        public UserDetails UserInfo { get; set; }
        public TokenStore TokenData { get; set; }
    }

    [Serializable]
    public class UserDetails
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Pin { get; set; }
        public string UserId { get; set; }
        public Guid DeviceId { get; set; }
        public DateTime DeviceIdExpiry { get; set; }
    }

    [Serializable]
    public sealed class TokenStore
    {
        public string access_token { get; set; }
        public string authorization_token { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }

        public DateTime CreatedDate = DateTime.Now;
        public DateTime ExpirationTime => CreatedDate.AddSeconds(Convert.ToInt32(expires_in));
    }
}
