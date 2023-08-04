﻿using HSMServer.Core.Model;
using HSMServer.Core.Model.Policies;
using System;

namespace HSMServer.Model.DataAlerts
{
    public sealed class SingleDataAlertViewModel<T, U> : DataAlertViewModel<T> where T : BaseValue<U>, new()
    {
        public SingleDataAlertViewModel(Guid entityId) : base(entityId) { }

        public SingleDataAlertViewModel(Policy<T, U> policy, BaseSensorModel sensor) : base(policy, sensor) { }


        protected override ConditionViewModel CreateCondition(bool isMain) => new SingleConditionViewModel<T, U>(isMain);
    }
}