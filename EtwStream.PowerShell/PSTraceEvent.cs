using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;

namespace EtwStream.PowerShell
{
    public class PSTraceEvent
    {
        public DateTime TimeStamp { get; }
        public TraceEventLevel Level { get; }
        public string ProcessName { get; }
        public string ProviderName { get; }
        public string EventName { get; }
        public string PayloadOrMessage { get; }

        public TraceEvent Source { get; }

        private PSHostUserInterface hostUI;

        public PSTraceEvent(TraceEvent traceEvent, PSHostUserInterface hostUI)
        {
            this.hostUI = hostUI;

            Source = traceEvent;

            TimeStamp = Source.TimeStamp;
            ProcessName = Process.GetProcessById(Source.ProcessID)?.ProcessName ?? "";
            ProviderName = Source.ProviderName;
            EventName = Source.EventName;
            Level = Source.Level;
            PayloadOrMessage = Source.DumpPayloadOrMessage();
        }

        public string Dump(bool includePrettyPrint = false, bool truncateDump = false)
        {
            return Source.Dump(includePrettyPrint, truncateDump);
        }

        public void DumpPayload()
        {
            Source.DumpPayload();
        }

        public string DumpPayloadOrMessage()
        {
            return Source.DumpPayloadOrMessage();
        }

        public ConsoleColor? GetColorMap(bool isBackgroundWhite)
        {
            return Source.GetColorMap(isBackgroundWhite);
        }

        public void DumpWithColor(bool withProviderName = false, bool withProcesName = false)
        {

            var proces = (withProcesName) ? $"[{ProcessName}]" : "";
            var provider = (withProcesName) ? $"[{ProviderName}]" : "";
            var message = $"[{Source.TimeStamp}]{proces}{provider}[{Source.EventName}]:{PayloadOrMessage}";
            var foreColor = GetColorMap(hostUI.RawUI.BackgroundColor == ConsoleColor.White) ?? hostUI.RawUI.ForegroundColor;

            hostUI.WriteLine(foreColor, hostUI.RawUI.BackgroundColor, message);
        }
    }
}
