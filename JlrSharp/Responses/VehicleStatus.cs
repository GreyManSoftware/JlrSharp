using System;
using System.Collections.Generic;
using System.Text;

namespace JlrSharp.Responses
{
    /// <summary>
    /// Provides a breakdown of the vehicles status
    /// </summary>
    [Serializable]
    public class VehicleStatus
    {
        public string Vin { get; set; }
        public int Mileage { get; set; }
        public int FuelPerc { get; set; }
        public int FuelRange { get; set; }
        public int DistanceUntilService { get; set; }
        public bool IsLocked { get; set; }
        public bool IsRunning { get; set; }
        public DoorStatus Doors { get; set; }
        public WindowStatus Windows { get; set; }
        public TyrePressures Tyres { get; set; }


        public VehicleStatus(Vehicle vehicle)
        {
            // This ensures we get the latest data
            vehicle.RefreshVehicleStatusReport();

            Vin = vehicle.vin;
            Mileage = vehicle.GetMileage();
            FuelPerc = vehicle.GetFuelLevelPercentage();
            FuelRange = vehicle.GetDistanceUntilEmpty();
            DistanceUntilService = vehicle.GetServiceDueInMiles();
            IsLocked = vehicle.IsLocked();
            IsRunning = vehicle.IsEngineRunning();
            Doors = vehicle.GetDoorLockStatus();
            Windows = vehicle.GetWindowStatus();
            Tyres = vehicle.GetTyrePressures();
        }
    }
}
