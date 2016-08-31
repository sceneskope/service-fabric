using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using SceneSkope.ServiceFabric.Utilities;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace SceneSkope.ServiceFabric.Seq
{
    public class SeqEventListener : EventListener
    {
        public static SeqEventListener Initialise()
        {
            var configurationProvider = new FabricConfigurationProvider("SeqConfig");

            SeqEventListener seqListener = null;
            if (configurationProvider.HasConfiguration)
            {
                seqListener = new SeqEventListener(configurationProvider);
            }
            return seqListener;
        }

        private readonly ILogger _logger;
        public bool Disabled { get; }

        private readonly MessageTemplateParser _parser = new MessageTemplateParser();

        public SeqEventListener(IConfigurationProvider configurationProvider)
        {
            Disabled = !configurationProvider.HasConfiguration;
            if (Disabled)
            {
                return;
            }

            var seqServer = configurationProvider.GetValue("SeqServer");
            _logger = new LoggerConfiguration()
                .WriteTo.Seq(seqServer, compact: true)
                .CreateLogger();
            Log.Logger = _logger;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (!Disabled)
            {
                EnableEvents(eventSource, EventLevel.LogAlways, (EventKeywords)~0);
            }
        }

        private static readonly LogEventLevel[] s_levels = {
            LogEventLevel.Information,
            LogEventLevel.Fatal,
            LogEventLevel.Error,
            LogEventLevel.Warning,
            LogEventLevel.Information,
            LogEventLevel.Verbose
        };

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var properties = new List<LogEventProperty>();

            properties.Add(new LogEventProperty("EtwEventId", new ScalarValue(eventData.EventId)));
            properties.Add(new LogEventProperty("Keywords", new ScalarValue((long)eventData.Keywords)));
            properties.Add(new LogEventProperty("ProviderId", new ScalarValue(eventData.EventSource.Guid)));
            properties.Add(new LogEventProperty("ProviderName", new ScalarValue(eventData.EventSource.Name)));

            string escapedMessage;
            try
            {
                if (eventData.Message != null)
                {
                    // If the event has a badly formatted manifest, the FormattedMessage property getter might throw
                    escapedMessage = string.Format(CultureInfo.InvariantCulture, eventData.Message, eventData.Payload.ToArray());
                }
                else
                {
                    escapedMessage = "<not set>";
                }
            }
            catch (Exception ex)
            {
                escapedMessage = $"<error> {ex.Message}";
            }

            var messageTemplate = _parser.Parse(escapedMessage);

            if ((eventData.Payload != null) && (eventData.PayloadNames != null))
            {
                var payloadEnumerator = eventData.Payload.GetEnumerator();
                var payloadNameEnumerator = eventData.PayloadNames.GetEnumerator();

                while (payloadEnumerator.MoveNext() && payloadNameEnumerator.MoveNext())
                {
                    properties.Add(new LogEventProperty(payloadNameEnumerator.Current, new ScalarValue(payloadEnumerator.Current)));
                }
            }

            var level = s_levels[(int)eventData.Level];
            var logEvent = new LogEvent(DateTimeOffset.Now, level, null, messageTemplate, properties);
            _logger.Write(logEvent);
        }
    }
}
