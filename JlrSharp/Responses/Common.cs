using System;
using System.Collections.Generic;
using System.Text;

namespace JlrSharp.Responses
{
    /// <summary>
    /// Contains the lock status for each door
    /// </summary>
    public class DoorStatus
    {
        public bool IsBonnetLocked { get; set; }
        public bool IsFrontLeftDoorLocked { get; set; }
        public bool IsFrontRightDoorLocked { get; set; }
        public bool IsRearLeftDoorLocked { get; set; }
        public bool IsRearRightDoorLocked { get; set; }
        public bool IsBootLocked { get; set; }
    }

    /// <summary>
    /// Contains the status of each window
    /// </summary>
    public class WindowStatus
    {
        public bool IsFrontLeftWindowClosed { get; set; }
        public bool IsFrontRightWindowClosed { get; set; }
        public bool IsRearLeftWindowClosed { get; set; }
        public bool IsRearRightWindowClosed { get; set; }
    }
    
    /// <summary>
    /// Contains the tyre pressures, stored in pound-force per square inch (PSI)
    /// </summary>
    public class TyrePressures
    {
        public int FrontLeft { get; set; }
        public int FrontRight { get; set; }
        public int RearLeft { get; set; }
        public int RearRight { get; set; }
    }
}
