using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using SceneSkope.ServiceFabric.Utilities;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace SceneSkope.ServiceFabric.Serilog.Seq
{
    public static class SeqEventListener
    {
        public static void Initialise()
        {
            var seqListener = new SerilogEventListener(SeqLogger.DefaultLogger);
            GC.KeepAlive(seqListener);
        }
    }
}
