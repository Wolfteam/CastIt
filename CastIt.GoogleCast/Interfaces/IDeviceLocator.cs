﻿using CastIt.GoogleCast.Shared.Device;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Interfaces
{
    internal interface IDeviceLocator
    {
        Task<List<IReceiver>> FindReceiversAsync();

        IObservable<IReceiver> FindReceiversContinuous();
    }
}