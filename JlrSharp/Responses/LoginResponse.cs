using System;
using System.Collections.Generic;
using System.Text;
using RestSharp.Deserializers;

namespace GreyMan.JlrSharp.Responses
{
    public class LoginResponse
    {
        [DeserializeAs(Name = "userId")]
        public string UserId { get; set; }
        [DeserializeAs(Name = "loginName")]
        public string LoginName { get; set; }
        [DeserializeAs(Name = "userType")]
        public object UserType { get; set; }
        [DeserializeAs(Name = "contact")]
        public Contact Contact { get; set; }
        [DeserializeAs(Name = "homeAddress")]
        public HomeAddress HomeAddress { get; set; }
    }

    public class Contact
    {
        [DeserializeAs(Name = "firstName")]
        public string FirstName { get; set; }
        [DeserializeAs(Name = "middleName")]
        public object MiddleName { get; set; }
        [DeserializeAs(Name = "lastName")]
        public string LastName { get; set; }
        [DeserializeAs(Name = "emailAddress")]
        public string EmailAddress { get; set; }
        [DeserializeAs(Name = "phoneNumbers")]
        public List<PhoneNumber> PhoneNumbers { get; set; }
    }

    public class PhoneNumber
    {
        [DeserializeAs(Name = "number")]
        public string Number { get; set; }
        [DeserializeAs(Name = "type")]
        public string Type { get; set; }
        [DeserializeAs(Name = "primary")]
        public bool Primary { get; set; }
    }

    public class HomeAddress
    {
        [DeserializeAs(Name = "addressLine1")]
        public string AddressLine1 { get; set; }
        [DeserializeAs(Name = "addressLine2")]
        public string AddressLine2 { get; set; }
        [DeserializeAs(Name = "street")]
        public object Street { get; set; }
        [DeserializeAs(Name = "city")]
        public string City { get; set; }
        [DeserializeAs(Name = "stateProvince")]
        public string StateProvince { get; set; }
        [DeserializeAs(Name = "country")]
        public string Country { get; set; }
        [DeserializeAs(Name = "zipCode")]
        public string ZipCode { get; set; }
    }
}
