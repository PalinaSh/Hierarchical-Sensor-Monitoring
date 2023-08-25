﻿using HSMDataCollector.Alerts;
using HSMDataCollector.DefaultSensors.SystemInfo;
using HSMDataCollector.Options;
using HSMSensorDataObjects;
using HSMSensorDataObjects.SensorRequests;

namespace HSMDataCollector.Prototypes.Collections.Disks
{
    internal sealed class WindowsActiveTimeDiskPrototype : BarDisksMonitoringPrototype
    {
        private string _sensorName = "Active time on {0} disk";

        protected override string SensorName => _sensorName;


        public WindowsActiveTimeDiskPrototype() : base()
        {
            Type = SensorType.DoubleBarSensor;
            SensorUnit = Unit.Percents;

            Alerts.Add(AlertsFactory.IfMean(AlertOperation.GreaterThanOrEqual, 80)
                                    .ThenSendNotification("[$product]$path $property $operation $target%")
                                    .AndSetIcon(AlertIcon.Warning).Build());
        }


        protected override DiskBarSensorOptions SetDiskInfo(DiskBarSensorOptions options)
        {
            options.SetInfo(new WindowsDiskInfo(options.TargetPath));

            _sensorName = string.Format(SensorName, options.DiskInfo.DiskLetter);

            return options;
        }
    }
}