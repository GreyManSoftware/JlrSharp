using System;
using System.Diagnostics;
using GreyMan.JlrSharp;
using GreyMan.JlrSharp.Responses;

namespace JlrSharpRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            
            JlrSharpConnection jlrSharp = new JlrSharpConnection();

            Trace.TraceInformation("Grabbing default vehicle");
            Vehicle defaultVehicle = jlrSharp.GetPrimaryVehicle();
            defaultVehicle.Unlock("1253");
            defaultVehicle.Lock("1253");
        }
    }
}
