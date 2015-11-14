# EtwStream.PowerShell
EtmStream listner for PowerShell
- [neuecc/EtwStream](https://github.com/neuecc/EtwStream)

## Get-TraceEventStream Cmdlet

```ps1
Get-TraceEventStream [-NameOrGuid] <string> [-DumpWithColor]
[-TraceLevel <TraceEventLevel> {
    Always
    | Critical
    | Error
    | Warning
    | Informational
    | Verbose
}] [<CommonParameters>]

Get-TraceEventStream [-WellKnownEventSource] <string> {
    AspNetEventSource
    | ConcurrentCollectionsEventSource
    | FrameworkEventSource
    | PinnableBufferCacheEventSource
    | PlinqEventSource
    | SqlEventSource
    | SynchronizationEventSource
    | TplEventSource}
[-DumpWithColor] [-TraceLevel <TraceEventLevel> {
    Always
    | Critical
    | Error
    | Warning
    | Informational
    | Verbose}]  [<CommonParameters>]
```

### Object pipeline
Get-TraceEventStream Cmdlet output [PSTraceEvent](https://github.com/pierre3/EtwStream.PowerShell/blob/master/EtwStream.PowerShell/PSTraceEvent.cs) objects.

![PSEventSource](https://raw.githubusercontent.com/pierre3/Images/master/EtwStreamPS_PSTraceEvent.png)

### DumpWithColor Switch

```ps1
PS C:\> Get-TraceEventStream -NameOrGuid SampleEventSource -DumpWithColor
```
![DumpWithColor](https://raw.githubusercontent.com/pierre3/Images/master/EtwStreamPS_DumpWithColor.png)

### WellKnownEventSource
You can choose WellKnownEventSource providers.
![WellKnownEventSource](https://raw.githubusercontent.com/pierre3/Images/master/EtwStreamPS_WellKnownEventSource.png)

### View in GridView-Window

```ps1
PS C:\> Get-TraceEventStream SampleEventSource | Out-GridView
```

![EtwStreamPS_Out-GridView.png](https://raw.githubusercontent.com/pierre3/Images/master/EtwStreamPS_Out-GridView.png)

#### Sorting and Filtering items

![EtwStreamPS_Out-GridView_filter.png](https://raw.githubusercontent.com/pierre3/Images/master/EtwStreamPS_Out-GridView_filter.png)

![EtwStreamPS_Out-GridView_filter2.png](https://raw.githubusercontent.com/pierre3/Images/master/EtwStreamPS_Out-GridView_filter2.png)
