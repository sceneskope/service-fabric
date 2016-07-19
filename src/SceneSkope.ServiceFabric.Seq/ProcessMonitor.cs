using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace SceneSkope.ServiceFabric.Seq
{
    public sealed class ProcessMonitor
    {
        private class Monitored
        {
            public static Monitored Create()
            {
                int availableWorkerThreads;
                int availableCompletionPortThreads;
                ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionPortThreads);
                int maxWorkerThreads;
                int maxCompletionPortThreads;
                ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);

                var process = Process.GetCurrentProcess();
                var monitored = new Monitored {
                    TotalMemory = GC.GetTotalMemory(false),
                    Gen0 = GC.CollectionCount(0),
                    Gen1 = GC.CollectionCount(1),
                    Gen2 = GC.CollectionCount(2),
                    WorkerThreads = maxWorkerThreads - availableWorkerThreads,
                    CompletionPortThreads = maxCompletionPortThreads - availableCompletionPortThreads,
                    PagedMemorySize = process.PagedMemorySize64,
                    WorkingSet = process.WorkingSet64,
                    VirtualMemorySize = process.VirtualMemorySize64,
                    UserProcessorTime = process.UserProcessorTime,
                    TotalProcessorTime = process.TotalProcessorTime,
                    Threads = process.Threads.Count,
                    PrivilegedProcessorTime = process.PrivilegedProcessorTime,
                    PeakWorkingSet = process.PeakWorkingSet64,
                    PeakVirtualMemorySize = process.PeakVirtualMemorySize64,
                    PeakPagedMemorySize = process.PeakPagedMemorySize64
                };
                return monitored;

            }
            public int CompletionPortThreads { get; private set; }
            public int Gen0 { get; private set; }
            public int Gen1 { get; private set; }
            public int Gen2 { get; private set; }
            public long PagedMemorySize { get; private set; }
            public long PeakPagedMemorySize { get; private set; }
            public long PeakVirtualMemorySize { get; private set; }
            public long PeakWorkingSet { get; private set; }
            public TimeSpan PrivilegedProcessorTime { get; private set; }
            public int Threads { get; private set; }
            public long TotalMemory { get; private set; }
            public TimeSpan TotalProcessorTime { get; private set; }
            public TimeSpan UserProcessorTime { get; private set; }
            public long VirtualMemorySize { get; private set; }
            public int WorkerThreads { get; private set; }
            public long WorkingSet { get; private set; }


            public Monitored CalculateDeltaFrom(Monitored initial) => new Monitored {
                TotalMemory = TotalMemory,
                Gen0 = Gen0,
                Gen1 = Gen1,
                Gen2 = Gen2,
                CompletionPortThreads = CompletionPortThreads,
                WorkerThreads = WorkerThreads,
                PagedMemorySize = PagedMemorySize,
                PeakPagedMemorySize = PeakPagedMemorySize,
                PeakVirtualMemorySize = PeakVirtualMemorySize,
                PeakWorkingSet = PeakWorkingSet,
                PrivilegedProcessorTime = PrivilegedProcessorTime - initial.PrivilegedProcessorTime,
                Threads = Threads,
                TotalProcessorTime = TotalProcessorTime - initial.TotalProcessorTime,
                UserProcessorTime = UserProcessorTime - initial.UserProcessorTime,
                VirtualMemorySize = VirtualMemorySize,
                WorkingSet = WorkingSet
            };
        }

        private readonly TimeSpan _duration;
        private readonly Task _task;
        private readonly MessageTemplate _template;
        private readonly ILogger _logger;

        public static ProcessMonitor Start(int durationMs = 60 * 1000) => new ProcessMonitor(durationMs);

        public ProcessMonitor(int durationMs = 60 * 1000) : this(TimeSpan.FromMilliseconds(durationMs)) { }
        public ProcessMonitor(TimeSpan durationMs)
        {
            GC.KeepAlive(this);
            _duration = durationMs;
            _template = new MessageTemplateParser().Parse("{node}:{process} {cpu} {memory} {threads}");
            var context = FabricRuntime.GetNodeContext();
            var nodeName = context.NodeName;
            var processName = Process.GetCurrentProcess().ProcessName;

            _logger = Log.ForContext("node", nodeName).ForContext("process", processName);
            _task = Task.Factory.StartNew(() => CalculateAsync(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
        }

        private async Task CalculateAsync()
        {

            var initial = Monitored.Create();
            while (true)
            {
                var latest = Monitored.Create();
                var delta = latest.CalculateDeltaFrom(initial);
                Report(delta);
                await Task.Delay(_duration);
            }
        }

        private void Report(Monitored delta)
        {
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, _template, new[] {
                new LogEventProperty("cpu", new ScalarValue(delta.TotalProcessorTime.TotalSeconds)),
                new LogEventProperty("memory", new ScalarValue(delta.VirtualMemorySize)),
                new LogEventProperty("threads", new ScalarValue(delta.Threads)),
                new LogEventProperty("totalMemory", new ScalarValue(delta.TotalMemory)),
                new LogEventProperty("gen0", new ScalarValue(delta.Gen0)),
                new LogEventProperty("gen1", new ScalarValue(delta.Gen1)),
                new LogEventProperty("gen2", new ScalarValue(delta.Gen2)),
                new LogEventProperty("workerThreads", new ScalarValue(delta.WorkerThreads)),
                new LogEventProperty("completionPortThreads", new ScalarValue(delta.CompletionPortThreads)),
                new LogEventProperty("pagedMemory", new ScalarValue(delta.PagedMemorySize)),
                new LogEventProperty("workingSet", new ScalarValue(delta.WorkingSet)),
                new LogEventProperty("peakVirtualMemory", new ScalarValue(delta.PeakVirtualMemorySize)),
                new LogEventProperty("peakPagedMemory", new ScalarValue(delta.PeakPagedMemorySize)),
                new LogEventProperty("peakWorkingSet", new ScalarValue(delta.PeakWorkingSet))
            });
            _logger.Write(logEvent);
        }
    }
}
