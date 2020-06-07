using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace JlrSharp.Responses
{
    public enum VehicleFuelType
    {
        Gasoline = 1,
        Ev = 2,
    }

    public interface IVehicle
    {
        string vin { get; set; }
        string userId { get; set; }
        string role { get; set; }
    }

    public interface IVehicleBaseFunctionality
    {
        VehicleFuelType FuelType { get; set; }
        int GetServiceDueInMiles();
        int GetMileage();
        int GetDistanceUntilEmpty();
        WindowStatus GetWindowStatus();
        TyrePressures GetTyrePressures();
        void Lock(string pin);
        void Unlock(string pin);
    }
}
