﻿using HSMDataCollector.Extensions;
using HSMDataCollector.Options;
using HSMSensorDataObjects;
using System;

namespace HSMDataCollector.Prototypes
{
    internal abstract class ModuleInfoPrototype : InstantSensorOptionsPrototype<ServiceSensorOptions>
    {
        internal const string ProductInfoCategory = "Module Info";

        protected override string Category => ProductInfoCategory;
    }


    internal abstract class ProductVersionInfoPrototype : InstantSensorOptionsPrototype<VersionSensorOptions>
    {
        protected override string Category => ModuleInfoPrototype.ProductInfoCategory;


        public override VersionSensorOptions Get(VersionSensorOptions customOptions)
        {
            var options = base.Get(customOptions);

            options.Type = SensorType.VersionSensor;
            options.StartTime = DateTime.UtcNow;

            options.Version = customOptions?.Version;

            return options;
        }
    }


    internal sealed class CollectorVersionPrototype : ProductVersionInfoPrototype
    {
        protected override string SensorName => "Collector version";


        public CollectorVersionPrototype() : base()
        {
            Description = "This sensor sends the current [**Version**](https://learn.microsoft.com/en-us/dotnet/api/system.version?view=netframework-4.7.2) of DataCollector " +
            "and its start time in UTC format. All available versions of DataCollector can be found [**here**](https://www.nuget.org/packages/HSMDataCollector.HSMDataCollector).";
        }


        public override VersionSensorOptions Get(VersionSensorOptions customOptions)
        {
            var options = base.Get(customOptions);

            options.Version = DataCollectorExtensions.Version;

            return options;
        }
    }


    internal sealed class ServiceCommandsPrototype : ModuleInfoPrototype
    {
        protected override string SensorName => "Service commands";


        public ServiceCommandsPrototype() : base()
        {
            Description = "This is a special sensor that sends information about various critical commands (Start, Stop, Update, Restart, etc.) and information about the initiator";
            Type = SensorType.StringSensor;
        }
    }


    internal sealed class ServiceStatusPrototype : ModuleInfoPrototype
    {
        protected override string SensorName => "Service status";


        public ServiceStatusPrototype() : base()
        {
            Description = "Current status of the connected product";
            Type = SensorType.IntSensor;
        }
    }


    internal sealed class ProductVersionPrototype : ProductVersionInfoPrototype
    {
        protected override string SensorName => "Version";


        public ProductVersionPrototype() : base()
        {
            Description = "Current connected product version after calling Start method";
        }
    }


    internal sealed class ServiceAlivePrototype : MonitoringInstantSensorOptionsPrototype<MonitoringInstantSensorOptions>
    {
        protected override string Category => ModuleInfoPrototype.ProductInfoCategory;

        protected override TimeSpan DefaultPostDataPeriod => TimeSpan.FromSeconds(15);

        protected override string SensorName => "Service alive";


        public ServiceAlivePrototype() : base()
        {
            Description = "Indicator that the monitored service is alive";
            Type = SensorType.BooleanSensor;
        }
    }
}