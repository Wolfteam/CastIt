using CastIt.Domain.Interfaces;
using System;

namespace CastIt.GoogleCast.Models.Events
{
    public class DeviceAddedArgs : EventArgs
    {
        public IReceiver Receiver { get; set; }

        public DeviceAddedArgs(IReceiver receiver)
        {
            Receiver = receiver;
        }
    }
}
