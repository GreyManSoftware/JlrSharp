using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JlrSharp.Responses.Vehicles
{
    public class PhevVehicle : ElectricVehicle
    {
        private GasVehicle GasFunctions;

        public override int GetDistanceUntilEmpty()
        {
            return Convert.ToInt32(VehicleStatusRaw.vehicleStatus
                .First(status => status.key == "EV_PHEV_RANGE_COMBINED_MILES").value);
        }

        public int GetFuelLevelPercentage() => GasFunctions.GetFuelLevelPercentage();

        public int GetBatteryRange()
        {
            return Convert.ToInt32(VehicleStatusRaw.vehicleStatus
                .First(status => status.key == "EV_RANGE_ON_BATTERY_MILES").value);
        }

        public PhevVehicle(Vehicle vehicle) : base(vehicle)
        {
            // This allows us to run certain GasVehicle functions
            GasFunctions = new GasVehicle(vehicle);
        }
    }
}
