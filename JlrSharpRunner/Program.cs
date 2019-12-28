using System;
using System.Diagnostics;
using GreyMan.JlrSharp;

namespace JlrSharpRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            JlrSharpConnection jlrSharp = new JlrSharpConnection();
            jlrSharp.Connect();
        }
    }
}
