﻿using HSMDataCollector.Core;

namespace HSMDataCollector.InstantValue
{
    abstract class InstantValueTypedSensorBase<T> : InstantValueSensorBase where T : struct
    {
        protected InstantValueTypedSensorBase(string path, string productKey, string address, IValuesQueue queue)
            : base(path, productKey, address, queue)
        {
        }
    }
}
