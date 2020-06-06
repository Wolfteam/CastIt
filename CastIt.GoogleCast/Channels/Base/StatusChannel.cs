using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Interfaces.Channels;
using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Channels
{
    internal abstract class StatusChannel<TStatus, TStatusMessage> : Channel, IStatusChannel<TStatus>
        where TStatusMessage : IStatusMessage<TStatus>
    {
        public event EventHandler StatusChanged;

        protected StatusChannel(string ns, string destinationId) : base(ns, destinationId)
        {
        }

        private TStatus _status;
        public TStatus Status
        {
            get { return _status; }
            private set
            {
                if (!EqualityComparer<TStatus>.Default.Equals(_status, value))
                {
                    _status = value;
                    OnStatusChanged();
                }
            }
        }

        object IStatusChannel.Status
        {
            get => Status;
            set => Status = (TStatus)value;
        }

        public override Task<AppMessage> OnMessageReceivedAsync(ISender sender, IMessage message)
        {
            switch (message)
            {
                case TStatusMessage statusMessage:
                    Status = statusMessage.Status;
                    break;
            }

            return base.OnMessageReceivedAsync(sender, message);
        }

        protected virtual void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
