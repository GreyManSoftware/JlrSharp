using System;
using System.Text;

namespace GreyMan.JlrSharp
{
    /// <summary>
    /// The interface for the JLR InControl connectivity
    /// </summary>
    public class JlrSharpConnection
    {
        private Uri IfasBaseUrl = new Uri("https://ifas.prod-row.jlrmotor.com/ifas/jlr"); // Used for generating tokens
        private Uri IfopBaseUrl = new Uri("https://ifop.prod-row.jlrmotor.com/ifop/jlr"); // Used for registering devices
        private Uri If9BaseUrl = new Uri("https://if9.prod-row.jlrmotor.com/if9/jlr"); // Used for vehicle requests

        private string _username;
        private string _password;
        private string _pin;
        private string _deviceId;
        private string _refreshToken;

        public JlrSharpConnection(string username, string password)
        {
            _username = username;
            _password = Convert.ToBase64String(Encoding.ASCII.GetBytes(password));
        }

        public void Connect()
        {

        }
    }
}