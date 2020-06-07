using System;
using System.Collections.Generic;
using System.Text;

namespace JlrSharp.Responses
{
    [Serializable]
    public class VehicleAttributes
    {
        public string engineCode { get; set; }
        public int seatsQuantity { get; set; }
        public string exteriorColorName { get; set; }
        public string exteriorCode { get; set; }
        public object interiorColorName { get; set; }
        public object interiorCode { get; set; }
        public object tyreDimensionCode { get; set; }
        public object tyreInflationPressureLightCode { get; set; }
        public object tyreInflationPressureHeavyCode { get; set; }
        public string fuelType { get; set; }
        public object fuelTankVolume { get; set; }
        public int grossWeight { get; set; }
        public int modelYear { get; set; }
        public object constructionDate { get; set; }
        public object deliveryDate { get; set; }
        public int numberOfDoors { get; set; }
        public string country { get; set; }
        public string registrationNumber { get; set; }
        public object carLocatorMapDistance { get; set; }
        public string vehicleBrand { get; set; }
        public string vehicleType { get; set; }
        public string vehicleTypeCode { get; set; }
        public string bodyType { get; set; }
        public string gearboxCode { get; set; }
        public List<Availableservice> availableServices { get; set; }
        public object timeFullyAccessible { get; set; }
        public object timePartiallyAccessible { get; set; }
        public object subscriptionType { get; set; }
        public object subscriptionStartDate { get; set; }
        public object subscriptionStopDate { get; set; }
        public List<Capability> capabilities { get; set; }
        public string nickname { get; set; }
        public Telematicsdevice telematicsDevice { get; set; }
    }

    public class Telematicsdevice
    {
        public string serialNumber { get; set; }
        public object imei { get; set; }
    }

    public class Availableservice
    {
        public string serviceType { get; set; }
        public bool vehicleCapable { get; set; }
        public bool serviceEnabled { get; set; }
    }

    public class Capability
    {
        public string capability { get; set; }
        public string capabilityClass { get; set; }
    }

}
