using Serilog;
using System;
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
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Log.Information("Exiting the app domain");
            global::Serilog.Log.CloseAndFlush();
        }

        public Task RunAsync(CancellationToken ct)
        {
            Log.Information("Starting controlled run async");
            var delayCancellationSource = new CancellationTokenSource();
            var completedSource = new TaskCompletionSource<bool>();
            var completedTask = completedSource.Task;
            var runningTask = Runner(ct);
            var registered = ct.Register(() =>
            {
                Log.Information("Cancellation requested. Waiting {Duration} for task to cancel", AbsoluteDelay);
                Task.Delay(AbsoluteDelay, delayCancellationSource.Token).ContinueWith(delayTask => completedSource.SetResult(true));
            });

            return Task.WhenAny(runningTask, completedTask).ContinueWith(finished =>
            {
                var finishedTask = finished.Result;
                Log.Information("Got to finished task, {Status}, Running = {Running}, Completed = {Completed}",
                    finishedTask.Status, finishedTask == runningTask, finishedTask == completedTask);
                delayCancellationSource.Cancel();
                if (ct.IsCancellationRequested)
                {
                    var cancelled = new OperationCanceledException(ct);
                    switch (runningTask.Status)
                    {
                        case TaskStatus.Canceled:
                        case TaskStatus.RanToCompletion:
                            Log.Information("Running task finished");
                            completedSource.TrySetException(cancelled);
                            break;

                        case TaskStatus.Faulted:
                            Log.Information(runningTask.Exception, "Running task faulted: {Exception}", runningTask.Exception.Message);
                            completedSource.TrySetException(runningTask.Exception);
                            break;

                        default:
                            Log.Warning("Running task status is {Status}", runningTask.Status);
                            completedSource.TrySetException(cancelled);
                            break;
                    }
                }
                else if (runningTask.IsFaulted)
                {
                    Log.Warning(runningTask.Exception, "Task faulted but not cancelled: {Exception}", runningTask.Exception.Message);
                    completedSource.TrySetException(runningTask.Exception);
                }
                else if (runningTask.IsCompleted)
                {
                    Log.Warning("Task completed but not cancelled");
                    completedSource.TrySetResult(true);
                }
                else if (completedSource.Task.IsCompleted)
                {
                    Log.Information("Completed source finished");
                }
                else
                {
                    Log.Warning("Not sure why here: {RunningTask} {CompletedTask}", runningTask.Status, completedTask.Status);
                    completedSource.TrySetResult(false);
                }
                registered.Dispose();
                delayCancellationSource.Dispose();

                Log.Information("Controlled task about to finish with {Status}", completedTask.Status);

                return completedTask;
            }).Unwrap();
        }
    }
}
