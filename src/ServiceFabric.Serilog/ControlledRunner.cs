using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Serilog
{
    public sealed class ControlledRunner
    {
        public static Task RunAsync(ILogger logger, Func<CancellationToken, Task> runner, CancellationToken ct)
        {
            var controller = new ControlledRunner(logger, runner);
            return controller.RunAsync(ct);
        }

        private ILogger Log { get; }
        private Func<CancellationToken, Task> Runner { get; }
        public TimeSpan AbsoluteDelay { get; set; } = TimeSpan.FromSeconds(10);

        public ControlledRunner(ILogger logger, Func<CancellationToken, Task> runner)
        {
            Log = logger.ForContext<ControlledRunner>();
            Runner = runner;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            Log.Information("Starting controlled run async");
            using (var delayCancellationSource = new CancellationTokenSource())
            {
                var runningTask = Runner(ct);
                using (ct.Register(() =>
                {
                    Log.Information("Cancellation requested. Waiting {Duration} for task to cancel", AbsoluteDelay);
                    Task.Delay(AbsoluteDelay, delayCancellationSource.Token).ContinueWith(delayTask =>
                    {
                        if (delayCancellationSource.IsCancellationRequested)
                        {
                            Log.Information("Delay cancelled ok, no need to worry");
                        }
                        else if (delayTask.IsCompleted)
                        {
                            AbortProcess("Controlled run did not complete in time");
                        }
                        else if (delayTask.IsCanceled)
                        {
                            Log.Information("Controlled run finished in time!!");
                        }
                        else if (delayTask.IsFaulted)
                        {
                            Log.Warning(delayTask.Exception, "Delay task faulted - not sure why!!!: {Exception}", delayTask.Exception.Message);
                            AbortProcess("Delay task went wrong");
                        }
                        else
                        {
                            Log.Warning("Delay task status: {Status}", delayTask.Status);
                            AbortProcess("Delay task status");
                        }
                    });
                }))
                {
                    try
                    {
                        await runningTask;
                        if (ct.IsCancellationRequested)
                        {
                            Log.Information("Running task completed - cancelling delay if set");
                            delayCancellationSource.Cancel();
                            throw new OperationCanceledException(ct);
                        }
                        else
                        {
                            Log.Information("Running task completed, but cancellation is not set!");
                            AbortProcess("Completed without cancel");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            if (ex is OperationCanceledException)
                            {
                                Log.Information("Running task completed - cancelling delay if set");
                            }
                            else
                            {
                                Log.Information(ex, "Running task faulted, but is cancelled - cancelling delay if set: {Exception}", ex.Message);
                            }
                            delayCancellationSource.Cancel();
                            throw;
                        }
                        else
                        {
                            Log.Fatal(ex, "Running task faulted, but cancellation is not set: {Exception}", ex.Message);
                            AbortProcess("Faulting task");
                        }
                    }
                }
            }
        }

        private void AbortProcess(string reason)
        {
            Log.Fatal("Aborting process due to {Reason}!!!", reason);
            global::Serilog.Log.CloseAndFlush();
            Environment.Exit(0);
        }
    }
}
