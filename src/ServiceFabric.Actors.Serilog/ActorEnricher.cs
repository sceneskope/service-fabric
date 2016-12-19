using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using ServiceFabric.Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ServiceFabric.Actors.Serilog
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
            if (_actorType == null) _actorType = propertyFactory.CreateProperty("actorType", _actor.GetType().ToString());
            if (_actorId == null) _actorId = propertyFactory.CreateProperty("actorId", _actor.Id.ToString());

            logEvent.AddPropertyIfAbsent(_actorType);
            logEvent.AddPropertyIfAbsent(_actorId);
        }
    }
}
