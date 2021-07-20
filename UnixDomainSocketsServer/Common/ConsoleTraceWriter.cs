using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixDomainSocketsServer.Common
{
    public class ConsoleTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter => TraceLevel.Verbose;

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            Console.WriteLine($"{level}:{message}");
            if(ex != null) Console.WriteLine(ex.ToString());
        }
    }
}
