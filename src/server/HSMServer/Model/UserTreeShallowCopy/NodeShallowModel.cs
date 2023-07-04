﻿using HSMServer.Core.Model;
using HSMServer.Extensions;
using HSMServer.Model.Authentication;
using HSMServer.Model.TreeViewModel;
using System.Collections.Generic;
using HSMServer.Model.TreeViewModels;

namespace HSMServer.Model.UserTreeShallowCopy
{
    public sealed class NodeShallowModel : BaseNodeShallowModel<ProductNodeViewModel>
    {
        public List<NodeShallowModel> Nodes { get; } = new(1 << 4);

        public List<SensorShallowModel> Sensors { get; } = new(1 << 4);


        public IntegrationState GrafanaState { get; } = new();

        public UserNotificationsState AccountState { get; } = new();


        public override bool IsGrafanaEnabled => GrafanaState.IsAllEnabled;

        public override bool IsAccountsEnable => AccountState.IsAllEnabled;

        public override bool IsAccountsIgnore => AccountState.IsAllIgnored;


        public int WidthDifference => Data.Sensors.Count + Data.Nodes.Count - VisibleTreeViewModel.RenderWidth;
        
        public bool IsDisabledNodeShown => WidthDifference > 0 && Sensors.Count + Nodes.Count > 0;
        
        
        public int VisibleSensorsCount { get; private set; }

        public string SensorsCountString
        {
            get
            {
                var sensorsCount = VisibleSensorsCount == Data.AllSensorsCount
                    ? $"{Data.AllSensorsCount}"
                    : $"{VisibleSensorsCount}/{Data.AllSensorsCount}";

                return $"({sensorsCount} sensors)";
            }
        }


        internal NodeShallowModel(ProductNodeViewModel data, User user) : base(data, user) { }


        internal BaseNodeShallowModel<SensorNodeViewModel> AddChildState(SensorShallowModel shallowSensor, User user)
        {
            shallowSensor.Parent = this;
            
            var sensor = shallowSensor.Data;

            if (sensor.State != SensorState.Muted)
            {
                AccountState.CalculateState(user.Notifications, sensor.Id);
                UpdateGroupsState(shallowSensor);
            }

            var isSensorMuted = sensor.State == SensorState.Muted;

            _mutedValue = !_mutedValue.HasValue ? isSensorMuted : _mutedValue & isSensorMuted;

            GrafanaState.CalculateState(shallowSensor);

            if (user.IsSensorVisible(sensor))
                VisibleSensorsCount++;

            return shallowSensor;
        }

        internal BaseNodeShallowModel<SensorNodeViewModel> AddChild(SensorShallowModel shallowSensor)
        {
            Sensors.Add(shallowSensor);

            return shallowSensor;
        }

        internal NodeShallowModel AddChildState(NodeShallowModel node, User user)
        {
            node.Parent = this;
            
            if (node._mutedValue.HasValue)
            {
                if (!node._mutedValue.Value)
                {
                    AccountState.CalculateState(node.AccountState);
                    UpdateGroupsState(node);
                }

                _mutedValue = !_mutedValue.HasValue ? node._mutedValue : _mutedValue & node._mutedValue;
            }

            GrafanaState.CalculateState(node.GrafanaState);

            VisibleSensorsCount += node.VisibleSensorsCount;

            return node;
        }

        internal NodeShallowModel AddChild(NodeShallowModel node)
        {
            Nodes.Add(node);

            return node;
        }
    }
}
