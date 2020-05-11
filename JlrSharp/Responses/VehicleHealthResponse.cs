namespace JlrSharp.Responses
{
    /// <summary>
    /// This is an internal structure used by the Jaguar API
    /// </summary>
    public class VehicleHealthReport
    {
        public string status { get; set; }
        public string statusTimestamp { get; set; } // I had deserialization issues with this and couldn't be bothered to fix it :)
        public string startTime { get; set; } // I had deserialization issues with this and couldn't be bothered to fix it :)
        public string serviceType { get; set; }
        public string failureDescription { get; set; }
        public string customerServiceId { get; set; }
        public string vehicleId { get; set; }
        public bool active { get; set; }
        public string initiator { get; set; }
        public object eventTrigger { get; set; }
        public object serviceCommand { get; set; }
        public object serviceParameters { get; set; }
    }
}