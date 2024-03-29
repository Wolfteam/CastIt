﻿namespace CastIt.GoogleCast.Shared.Device
{
    public interface IReceiver
    {
        string Id { get; }

        string FriendlyName { get; }

        string Type { get; }

        string Host { get; }

        int Port { get; }

        bool IsConnected { get; set; }
    }
}
