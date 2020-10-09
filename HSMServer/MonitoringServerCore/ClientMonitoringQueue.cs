﻿using System;
using System.Collections.Generic;
using SensorsService;

namespace HSMServer.MonitoringServerCore
{
    public class ClientMonitoringQueue
    {
        private readonly object _lockObj = new object();
        private readonly Queue<SensorUpdateMessage> _monitoringQueue;
        private readonly List<SensorUpdateMessage> _emptyQueue = new List<SensorUpdateMessage>();
        private const int ErrorCapacity = 10000;
        private const int WarningCapacity = 5000;
        private const int UpdateListCapacity = 1000;
        public event EventHandler QueueOverflow;
        public event EventHandler QueueOverflowWarning;

        private bool HasData
        {
            get
            {
                lock (_lockObj)
                {
                    return _monitoringQueue.Count > 0;
                }
            }
        } 

        public ClientMonitoringQueue()
        {
            _monitoringQueue = new Queue<SensorUpdateMessage>();
        }

        public void AddUpdate(SensorUpdateMessage message)
        {
            lock (_lockObj)
            {
                _monitoringQueue.Enqueue(message);
            }            
        }

        public List<SensorUpdateMessage> GetSensorUpdateMessages()
        {
            lock (_lockObj)
            {
                if (!HasData)
                {
                    return _emptyQueue;
                }
                List<SensorUpdateMessage> updateList = new List<SensorUpdateMessage>();
                for (int i = 0; i < UpdateListCapacity; i++)
                {
                    updateList.Add(_monitoringQueue.Dequeue());
                }

                return updateList;
            }
        }

        public void Clear()
        {
            lock (_lockObj)
            {
                int count = _monitoringQueue.Count;
                for (int i = 0; i < count; i++)
                {
                    _monitoringQueue.Dequeue();
                }
            }
        }
        private void OnQueueOverflow()
        {
            QueueOverflow?.Invoke(this, EventArgs.Empty);
        }

        private void OnQueueOverflowWarning()
        {
            QueueOverflowWarning?.Invoke(this, EventArgs.Empty);
        }
    }
}
