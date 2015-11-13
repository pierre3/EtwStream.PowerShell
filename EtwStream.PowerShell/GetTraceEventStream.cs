using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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

        [ValidateSet(
            nameof(WellKnownEventSources.AspNetEventSource),
            nameof(WellKnownEventSources.ConcurrentCollectionsEventSource),
            nameof(WellKnownEventSources.FrameworkEventSource),
            nameof(WellKnownEventSources.PinnableBufferCacheEventSource),
            nameof(WellKnownEventSources.PlinqEventSource),
            nameof(WellKnownEventSources.SqlEventSource),
            nameof(WellKnownEventSources.SynchronizationEventSource),
            nameof(WellKnownEventSources.TplEventSource))]
        [Parameter(Position = 0, ParameterSetName = "wellKnown", Mandatory = true)]
        public string WellKnownEventSource { get; set; }

        [Parameter(Position = 0, ParameterSetName = "nameOrGuid", Mandatory = true)]
        public string NameOrGuid { get; set; }

        [Parameter]
        public SwitchParameter DumpWithColor { get; set; }

        [Parameter]
        public TraceEventLevel TraceLevel { get; set; } = TraceEventLevel.Verbose;


        protected override void ProcessRecord()
        {
            IObservable<TraceEvent> listener = (ParameterSetName == "wellKnown")
                ? GetWellKnownEventListener(WellKnownEventSource)
                : ObservableEventListener.FromTraceEvent(NameOrGuid);
            
            var q = new BlockingCollection<Action>();
            Exception exception = null;

            var d = listener
                .Where(x=> Process.GetCurrentProcess().Id != x.ProcessID)
                .Where(x=>x.Level <= TraceLevel)
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
    }
    
}
