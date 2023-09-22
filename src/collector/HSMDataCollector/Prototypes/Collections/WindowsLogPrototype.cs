using HSMDataCollector.Options;
using HSMSensorDataObjects;

namespace HSMDataCollector.Prototypes.Collections
{
    internal abstract class WindowsLogPrototype : InstantSensorOptionsPrototype<InstantSensorOptions>
    {
        internal const string BaseDescription = "The sensor reads Windows Logs and sends all logs with {0} status. The information is read using " +
                                                "[**Event log**](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.eventlog?view=dotnet-plat-ext-7.0)";

        protected override string Category => "Windows OS info/Windows Logs";

        protected abstract string Status { get; }


        protected WindowsLogPrototype() : base()
        {
            Description = string.Format(BaseDescription, Status);
            IsSingletonSensor = true;
            IsComputerSensor = true;
            
            Type = SensorType.StringSensor;
        }
    }

    internal class WindowsErrorLogsPrototype : WindowsLogPrototype
    {
        protected override string SensorName => "Windows Error Logs";

        protected override string Status => "Error";
    }
    
    internal class WindowsWarningLogsPrototype : WindowsLogPrototype
    {
        protected override string SensorName => "Windows Warning Logs";

        protected override string Status => "Warning";
    }
}