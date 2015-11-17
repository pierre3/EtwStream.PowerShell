using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace EtwStream.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "TraceEventStream", DefaultParameterSetName = "nameOrGuid")]
    public class GetTraceEvent : PSCmdlet
    {
        private CompositeDisposable disposable = new CompositeDisposable();

        [Parameter(Position = 0, ParameterSetName = "nameOrGuid", Mandatory = true, ValueFromPipeline = true)]
        public string[] NameOrGuid { get; set; }

        [ValidateSet(
            nameof(WellKnownEventSources.AspNetEventSource),
            nameof(WellKnownEventSources.ConcurrentCollectionsEventSource),
            nameof(WellKnownEventSources.FrameworkEventSource),
            nameof(WellKnownEventSources.PinnableBufferCacheEventSource),
            nameof(WellKnownEventSources.PlinqEventSource),
            nameof(WellKnownEventSources.SqlEventSource),
            nameof(WellKnownEventSources.SynchronizationEventSource),
            nameof(WellKnownEventSources.TplEventSource))]
        [Parameter(Position = 0, ParameterSetName = "wellKnown", Mandatory = true, ValueFromPipeline = true)]
        public string[] WellKnownEventSource { get; set; }


        [ValidateSet(
            nameof(IISEventSources.AspDotNetEvents),
            nameof(IISEventSources.HttpEvent),
            nameof(IISEventSources.HttpLog),
            nameof(IISEventSources.HttpService),
            nameof(IISEventSources.IISAppHostSvc),
            nameof(IISEventSources.IISLogging),
            nameof(IISEventSources.IISW3Svc),
            nameof(IISEventSources.RuntimeWebApi),
            nameof(IISEventSources.RuntimeWebHttp))]
        [Parameter(Position = 0, ParameterSetName = "IIS", Mandatory = true, ValueFromPipeline = true)]
        public string[] IISEventSource { get; set; }


        [Parameter]
        public SwitchParameter DumpWithColor { get; set; }

        [Parameter]
        public TraceEventLevel TraceLevel { get; set; } = TraceEventLevel.Verbose;

        private IObservable<TraceEvent> listener = Observable.Empty<TraceEvent>();

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "wellKnown":
                    listener = listener.Merge(WellKnownEventSource.Select(x => GetWellKnownEventListener(x)).Merge());
                    break;
                case "IIS":
                    listener = listener.Merge(IISEventSource.Select(x => GetIISEventListener(x)).Merge());
                    break;
                default:
                    listener = listener.Merge(NameOrGuid.Select(x => ObservableEventListener.FromTraceEvent(x)).Merge());
                    break;
            }
        }

        protected override void EndProcessing()
        {
            var q = new BlockingCollection<Action>();
            Exception exception = null;

            var d = listener
                .Where(x => Process.GetCurrentProcess().Id != x.ProcessID)
                .Where(x => x.Level <= TraceLevel)
                .Subscribe(
                x =>
                {
                    q.Add(() =>
                    {
                        var item = new PSTraceEvent(x, Host.UI);
                        if (DumpWithColor.IsPresent)
                        {
                            item.DumpWithColor();
                        }
                        else
                        {
                            WriteObject(item);
                        }
                        WriteVerbose(item.DumpPayloadOrMessage());
                    });
                },
                e =>
                {
                    exception = e;
                    q.CompleteAdding();
                }, q.CompleteAdding);


            disposable.Add(d);
            var cts = new CancellationTokenSource();
            disposable.Add(new CancellationDisposable(cts));
            foreach (var act in q.GetConsumingEnumerable(cts.Token))
            {
                act();
            }

            if (exception != null)
            {
                ThrowTerminatingError(new ErrorRecord(exception, "1", ErrorCategory.OperationStopped, null));
            }
        }

        protected override void StopProcessing()
        {
            disposable.Dispose();
        }

        private IObservable<TraceEvent> GetWellKnownEventListener(string wellKnownEventSource)
        {
            switch (wellKnownEventSource)
            {
                case nameof(WellKnownEventSources.AspNetEventSource):
                    return ObservableEventListener.FromTraceEvent(WellKnownEventSources.AspNetEventSource);
                case nameof(WellKnownEventSources.ConcurrentCollectionsEventSource):
                    return ObservableEventListener.FromTraceEvent(WellKnownEventSources.ConcurrentCollectionsEventSource);
                case nameof(WellKnownEventSources.FrameworkEventSource):
                    return ObservableEventListener.FromTraceEvent(WellKnownEventSources.FrameworkEventSource);
                case nameof(WellKnownEventSources.PinnableBufferCacheEventSource):
                    return ObservableEventListener.FromTraceEvent(WellKnownEventSources.PinnableBufferCacheEventSource);
                case nameof(WellKnownEventSources.PlinqEventSource):
                    return ObservableEventListener.FromTraceEvent(WellKnownEventSources.PlinqEventSource);
                case nameof(WellKnownEventSources.SqlEventSource):
                    return ObservableEventListener.FromTraceEvent(WellKnownEventSources.SqlEventSource);
                case nameof(WellKnownEventSources.SynchronizationEventSource):
                    return ObservableEventListener.FromTraceEvent(WellKnownEventSources.SynchronizationEventSource);
                case nameof(WellKnownEventSources.TplEventSource):
                    return ObservableEventListener.FromTraceEvent(WellKnownEventSources.TplEventSource);
                default:
                    return Observable.Empty<TraceEvent>();
            }
        }

        private IObservable<TraceEvent> GetIISEventListener(string iisEventSource)
        {

            switch (iisEventSource)
            {
                case nameof(IISEventSources.AspDotNetEvents):
                    return ObservableEventListener.FromTraceEvent(IISEventSources.AspDotNetEvents);
                case nameof(IISEventSources.HttpEvent):
                    return ObservableEventListener.FromTraceEvent(IISEventSources.HttpEvent);
                case nameof(IISEventSources.HttpLog):
                    return ObservableEventListener.FromTraceEvent(IISEventSources.HttpLog);
                case nameof(IISEventSources.HttpService):
                    return ObservableEventListener.FromTraceEvent(IISEventSources.HttpService);
                case nameof(IISEventSources.IISAppHostSvc):
                    return ObservableEventListener.FromTraceEvent(IISEventSources.IISAppHostSvc);
                case nameof(IISEventSources.IISLogging):
                    return ObservableEventListener.FromTraceEvent(IISEventSources.IISLogging);
                case nameof(IISEventSources.IISW3Svc):
                    return ObservableEventListener.FromTraceEvent(IISEventSources.IISW3Svc);
                case nameof(IISEventSources.RuntimeWebApi):
                    return ObservableEventListener.FromTraceEvent(IISEventSources.RuntimeWebApi);
                case nameof(IISEventSources.RuntimeWebHttp):
                    return ObservableEventListener.FromTraceEvent(IISEventSources.RuntimeWebHttp);
                default:
                    return Observable.Empty<TraceEvent>();

            }
        }
    }

}
