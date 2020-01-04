using System;

namespace JlrSharp
{
    [Serializable]
    public class UserDetails
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserId { get; set; }
        public Guid DeviceId { get; set; }
        public DateTime DeviceIdExpiry { get; set; }
    }
}
