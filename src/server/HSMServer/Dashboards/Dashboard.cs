﻿using HSMDatabase.AccessManager.DatabaseEntities;
using HSMServer.ConcurrentStorage;
using HSMServer.Core.Model;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace HSMServer.Dashboards
{
    public sealed class Dashboard : BaseServerModel<DashboardEntity, DashboardUpdate>
    {
        private static readonly TimeSpan _defaultPeriod = new (0, 30, 0);


        public ConcurrentDictionary<Guid, Panel> Panels { get; } = new();


        public DateTime FromDataPeriod { get; private set; }

        public DateTime? ToDataPeriod { get; private set; }

        public TimeSpan DataPeriod { get; private set; } = new (0, 30, 0);


        internal Func<Guid, BaseSensorModel> GetSensorModel;


        internal Dashboard(DashboardEntity entity) : base(entity)
        {
            Panels = new ConcurrentDictionary<Guid, Panel>(entity.Panels?.ToDictionary(k => new Guid(k.Id), v => new Panel(v, this))) ?? new();
            DataPeriod = GetPeriod(entity.Period);
        }
        
        internal Dashboard(DashboardEntity entity, Func<Guid, BaseSensorModel> getSensorModel) : base(entity)
        {
            GetSensorModel += getSensorModel;
            DataPeriod = GetPeriod(entity.Period);
            Panels = new ConcurrentDictionary<Guid, Panel>(entity.Panels?.ToDictionary(k => new Guid(k.Id), v => new Panel(v, this))) ?? new();
        }

        internal Dashboard(DashboardAdd addModel) : base(addModel) { }

        public void UpdateDataPeriod(DateTime from, DateTime? to)
        {
            FromDataPeriod = from;
            ToDataPeriod = to;
        }


        public override void Update(DashboardUpdate update)
        {
            DataPeriod = update.FromPeriod;
            base.Update(update);
        }

        public override DashboardEntity ToEntity()
        {
            var entity = base.ToEntity();

            entity.Panels.AddRange(Panels.Select(u => u.Value.ToEntity()));
            entity.Period = DataPeriod;
            return entity;
        }


        private TimeSpan GetPeriod(TimeSpan entityPeriod) => entityPeriod == TimeSpan.Zero ? _defaultPeriod : entityPeriod;
    }
}