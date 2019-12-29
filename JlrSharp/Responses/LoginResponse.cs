using System;
using System.Collections.Generic;
using System.Text;
using RestSharp.Deserializers;

namespace GreyMan.JlrSharp.Responses
{
    [Serializable]
    public class LoginResponse
    {
        public string userId { get; set; }
        public string loginName { get; set; }
        public object userType { get; set; }
        public Contact contact { get; set; }
        public HomeAddress homeAddress { get; set; }
    }

    [Serializable]
    public class Contact
    {
        public string firstName { get; set; }
        public object middleName { get; set; }
        public string lastName { get; set; }
        public string emailAddress { get; set; }
        public List<PhoneNumber> phoneNumbers { get; set; }
    }

    [Serializable]
    public class PhoneNumber
    {
        public string number { get; set; }
        public string type { get; set; }
        public bool primary { get; set; }
    }

    [Serializable]
    public class HomeAddress
    {
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public object street { get; set; }
        public string city { get; set; }
        public string stateProvince { get; set; }
        public string country { get; set; }
        public string zipCode { get; set; }
    }
}
