using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace ServiceFabric.Serilog
{
    public class SerilogEventListener : EventListener
    {
        public static void Initialise(ILogger log)
        {
            var listener = new SerilogEventListener(log);
            GC.KeepAlive(listener);
        }

        private readonly ILogger _logger;
        private readonly MessageTemplateParser _parser = new MessageTemplateParser();

        public SerilogEventListener(ILogger logger)
        {
            _logger = logger;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            EnableEvents(eventSource, EventLevel.LogAlways, (EventKeywords)~0);
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
            var properties = new List<LogEventProperty>
            {
                new LogEventProperty("etwEventId", new ScalarValue(eventData.EventId)),
                new LogEventProperty("keywords", new ScalarValue(((long)eventData.Keywords).ToString("X016"))),
                new LogEventProperty("providerId", new ScalarValue(eventData.EventSource.Guid)),
                new LogEventProperty("providerName", new ScalarValue(eventData.EventSource.Name))
            };
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
