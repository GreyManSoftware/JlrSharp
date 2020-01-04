using System;
using System.Diagnostics;

namespace JlrSharp.Utils
{
    public static class Validators
    {
        public static Guid GenerateDeviceId(string deviceIdString)
        {
            // Validate existing device ID or compute new one
            if (!String.IsNullOrEmpty(deviceIdString))
            {
                return !Guid.TryParse(deviceIdString, out Guid deviceId) ? Guid.NewGuid() : deviceId;
            }

            Trace.TraceInformation("Computing new device ID");
            return Guid.NewGuid();
        }
    }
}
