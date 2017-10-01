using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Serilog;

namespace SceneSkope.ServiceFabric.Actors.Serilog
{
    public abstract class SerilogActor : Actor
    {
        protected ILogger Log { get; }

        protected SerilogActor(ActorService actorService, ActorId actorId, ILogger logger) : base(actorService, actorId)
        {
            Log = logger.ForContext(new[] {  new ActorEnricher(this)});
        }
    }
}
