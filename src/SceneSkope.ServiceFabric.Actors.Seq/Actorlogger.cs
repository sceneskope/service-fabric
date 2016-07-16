using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using SceneSkope.ServiceFabric.Seq;
using Serilog;

namespace SceneSkope.ServiceFabric.Actors.Seq
{
    public static class ActorLogger
    {
        public static ILogger CreateLogger(this Actor actor) =>
            ServiceFabricLogger.Logger.ForContext(new[] { new ActorEnricher(actor) });
    }
}
