using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace JlrSharp.Responses
{
    public enum VehicleFuelType
    {
        Unknown = 0,
        Gasoline = 1,
        Bev = 2,
        Phev = 3,
        Mhev = 4,
    }

    public interface IVehicle
    {
        string vin { get; set; }
        string userId { get; set; }
        string role { get; set; }
    }

    public interface IVehicleBaseFunctionality
    {
        VehicleFuelType FuelType { get; }
        int GetServiceDueInMiles();
        int GetMileage();
        int GetDistanceUntilEmpty();
        WindowStatus GetWindowStatus();
        TyrePressures GetTyrePressures();
        void Lock(string pin, bool updateStatus = false);
        void Unlock(string pin, bool updateStatus = false);
    }
}
