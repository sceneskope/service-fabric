using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using SceneSkope.ServiceFabric.Serilog;
using Serilog.Core;
using Serilog.Events;

namespace SceneSkope.ServiceFabric.Actors.Serilog
{
    internal class ActorEnricher : StatefulServiceEnricher
    {
        private readonly Actor _actor;

        private LogEventProperty _actorType;
        private LogEventProperty _actorId;

        public ActorEnricher(Actor actor) : base(actor.ActorService.Context)
        {
            _actor = actor;
        }

        public override void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            base.Enrich(logEvent, propertyFactory);
            if (_actorType == null) _actorType = propertyFactory.CreateProperty("ActorType", _actor.GetType().ToString());
            if (_actorId == null) _actorId = propertyFactory.CreateProperty("ActorId", _actor.Id.ToString());

            logEvent.AddPropertyIfAbsent(_actorType);
            logEvent.AddPropertyIfAbsent(_actorId);
        }
    }
}
