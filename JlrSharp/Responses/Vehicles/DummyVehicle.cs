using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JlrSharp.Responses.Vehicles
{
    /// <summary>
    /// This class is used to store the initial data back from Jaguar
    /// We upgrade the objects from this when we know the fuel type
    /// </summary>
    [Serializable]
    internal class DummyVehicle : Vehicle
    {
        public override int GetDistanceUntilEmpty()
        {
            throw new NotImplementedException();
        }
    }
}
