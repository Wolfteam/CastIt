using CastIt.GoogleCast.Channels;
using CastIt.GoogleCast.Enums;
using CastIt.GoogleCast.Extensions;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Interfaces.Channels;
using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Messages;
using CastIt.GoogleCast.Messages.Base;
using CastIt.GoogleCast.Models.Events;
using CastIt.GoogleCast.Models.Media;
using CastIt.GoogleCast.Models.Receiver;
using MvvmCross.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.GoogleCast
{
    public class Player : IDisposable
    {
        #region Members
        private const string ApplicationId = "CC1AD845";

        private readonly IMvxLog _logger;
        private readonly ISender _sender;

        private readonly IConnectionChannel _connectionChannel;
        private readonly IHeartbeatChannel _heartbeatChannel;
        private readonly IMediaChannel _mediaChannel;
        private readonly IReceiverChannel _receiverChannel;

        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly string _destinationId;

        private static readonly SupportedMessages _supportedMsgs = new SupportedMessages();
        private CancellationTokenSource _mediaChangedToken;
        private double _seekedSeconds;
        #endregion

        #region Events
        public event EventHandler Disconnected;
        public event EventHandler<DeviceAddedArgs> DeviceAdded;
        public event EventHandler<double> TimeChanged;
        public event EventHandler<double> PositionChanged;
        public event EventHandler EndReached;
        #endregion

        #region Properties
        public static bool CanLog { get; private set; }
        public bool IsPlaying { get; private set; }
        public double CurrentMediaDuration { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public double PlayedPercentage
        {
            get
            {
                if (CurrentMediaDuration > 0)
                    return ElapsedSeconds * 100 / CurrentMediaDuration;
                return 0;
            }
        }
        #endregion

        #region Constructors
        public Player(
            IMvxLog logger,
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logMsgs = true)
            : this(logger, null, destinationId, senderId, logMsgs)
        {
        }

        public Player(
            IMvxLog logger,
            IReceiver receiver,
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logMsgs = true)
            : this(receiver?.Host, receiver?.Port ?? 0, destinationId, senderId, logMsgs)
        {
            _logger = logger;
        }

        public Player(
            IMvxLog logger,
            string host,
            int port = 8009,
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logMsgs = true)
            : this(host, port, destinationId, senderId, logMsgs)
        {
            _logger = logger;
        }

        public Player(
            IReceiver receiver,
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logMsgs = true)
            : this(receiver?.Host, receiver?.Port ?? 0, destinationId, senderId, logMsgs)
        {
        }

        public Player(
            string host,
            int port = 8009,
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logMsgs = true)
        {
            _destinationId = destinationId;
            CanLog = logMsgs;

            _sender = new Sender(_logger, senderId, host, port, HandleResponseMsg);
            _connectionChannel = new ConnectionChannel(destinationId);
            _heartbeatChannel = new HeartbeatChannel(destinationId);
            _mediaChannel = new MediaChannel(destinationId, async () =>
            {
                var app = await _receiverChannel.GetApplication(_sender, _connectionChannel, _mediaChannel.Namespace);
                return app.SessionId;
            });
            _receiverChannel = new ReceiverChannel(destinationId);
        }
        #endregion

        public void Init()
        {
            var subscription = DeviceLocator
                .FindReceiversContinuous()
                .Subscribe(d => DeviceAdded?.Invoke(this, new DeviceAddedArgs(d)));

            _subscriptions.Add(subscription);
        }

        public async Task ConnectAsync()
        {
            await _sender.ConnectAsync();
            await _connectionChannel.ConnectAsync(_sender, _destinationId);
            await _receiverChannel.LaunchAsync(_sender, ApplicationId);
        }

        public async Task ConnectAsync(IReceiver receiver)
        {
            await _sender.ConnectAsync(receiver);
            await _connectionChannel.ConnectAsync(_sender, _destinationId);
            await _receiverChannel.LaunchAsync(_sender, ApplicationId);
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            DisconnectAsync().GetAwaiter().GetResult();
            Disconnected = null;
            DeviceAdded = null;
            PositionChanged = null;
            TimeChanged = null;
            EndReached = null;
        }

        public static Task<List<IReceiver>> GetDevicesAsync()
        {
            return DeviceLocator.FindReceiversAsync();
        }

        #region Receiver Channel
        public Task<ReceiverStatus> SetVolumeAsync(float level)
        {
            return _receiverChannel.SetVolumeAsync(_sender, level);
        }

        public Task<ReceiverStatus> SetIsMutedAsync(bool isMuted)
        {
            return _receiverChannel.SetIsMutedAsync(_sender, isMuted);
        }

        public async Task DisconnectAsync()
        {
            //_receiverChannel.Status = null;
            //_mediaChannel.Status = null;
            CancelAndSetMediaToken();
            await _receiverChannel.StopAsync(_sender);
            _sender.Disconnect();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Media Channel
        public async Task<MediaStatus> LoadAsync(
            MediaInformation media,
            bool autoPlay = true,
            double seekedSeconds = 0,
            params int[] activeTrackIds)
        {
            var app = await _receiverChannel.GetApplication(_sender, _connectionChannel, _mediaChannel.Namespace);
            var status = await _mediaChannel.LoadAsync(_sender, app.SessionId, media, autoPlay, activeTrackIds);
            CurrentMediaDuration = media.Duration ?? status.Media.Duration ?? 0;
            ElapsedSeconds = 0;
            _seekedSeconds = seekedSeconds;
            IsPlaying = true;
            ListenForMediaChanges();

            return status;
        }

        public Task<MediaStatus> GetMediaStatusAsync()
        {
            return _mediaChannel.GetStatusAsync(_sender);
        }

        public Task<MediaStatus> PlayAsync()
        {
            IsPlaying = false;
            return _mediaChannel.PlayAsync(_sender);
        }

        public Task<MediaStatus> PauseAsync()
        {
            IsPlaying = false;
            return _mediaChannel.PauseAsync(_sender);
        }

        public Task<MediaStatus> StopPlaybackAsync()
        {
            IsPlaying = false;
            return _mediaChannel.StopAsync(_sender);
        }

        public Task<MediaStatus> SeekAsync(double seconds)
        {
            return _mediaChannel.SeekAsync(_sender, seconds);
        }
        #endregion

        #region Helpers
        private async Task HandleResponseMsg(CastMessage msg)
        {
            var payload = msg.PayloadType == PayloadType.Binary
                ? Encoding.UTF8.GetString(msg.PayloadBinary)
                : msg.PayloadUtf8;
            _logger.LogInfo($"RECEIVED: {msg.Namespace} : {payload}");
            var channel = GetChannel(msg.Namespace);
            if (channel == null)
            {
                _logger.LogInfo($"Cant process payload {Environment.NewLine} {payload}. {Environment.NewLine} No suitable channel was found");
                return;
            }
            var message = JsonConvert.DeserializeObject<MessageWithId>(payload, AppConstants.JsonSettings);
            if (_supportedMsgs.TryGetValue(message.Type, out Type type))
            {
                _logger.LogInfo($"Got msg type = {message.Type}");
                try
                {

                    var response = (IMessage)JsonConvert.DeserializeObject(payload, type, AppConstants.JsonSettings);
                    var appMessage = await channel.OnMessageReceivedAsync(_sender, response);
                    TaskCompletionSourceInvoke(message, "SetResult", response);
                    if (appMessage?.Message != null)
                    {
                        await _sender.SendAsync(appMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{nameof(HandleResponseMsg)}: Unknown error occurred", ex);
                    TaskCompletionSourceInvoke(message, "SetException", ex, new Type[] { typeof(Exception) });
                }
            }
            else
            {
                _logger.LogInfo($"Could not get a supported msg for type {message.Type}");
            }
        }

        private void TaskCompletionSourceInvoke(MessageWithId message, string method, object parameter, Type[] types = null)
        {
            if (message.HasRequestId && _sender.WaitingTasks.TryRemove(message.RequestId, out object tcs))
            {
                var tcsType = tcs.GetType();
                var m = types == null ? tcsType.GetMethod(method) : tcsType.GetMethod(method, types);
                m.Invoke(tcs, new object[] { parameter });
            }
        }

        private IChannel GetChannel(string ns)
        {
            var channels = new List<IChannel>
            {
                _connectionChannel,
                _mediaChannel,
                _receiverChannel,
                _heartbeatChannel
            };

            return channels.FirstOrDefault(c => c.Namespace.Equals(ns, StringComparison.OrdinalIgnoreCase));
        }

        private void CancelAndSetMediaToken()
        {
            if (_mediaChangedToken?.IsCancellationRequested == false)
            {
                _mediaChangedToken.Cancel();
            }

            _mediaChangedToken = new CancellationTokenSource();
        }

        private async void ListenForMediaChanges()
        {
            CancelAndSetMediaToken();
            try
            {
                await Task.Run(async () =>
                {
                    bool checkMediaStatus = true;
                    while (checkMediaStatus)
                    {
                        await Task.Delay(250);
                        var mediaStatus = await _mediaChannel.GetStatusAsync(_sender);
                        if (mediaStatus is null)
                        {
                            IsPlaying = false;
                            checkMediaStatus = false;
                            _logger.LogInfo("MediaStatusChanged wont be raised anymore");
                            EndReached?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                        ElapsedSeconds = mediaStatus.CurrentTime + _seekedSeconds;
                        TimeChanged?.Invoke(this, ElapsedSeconds);
                        PositionChanged?.Invoke(this, PlayedPercentage);

                        if (ElapsedSeconds >= CurrentMediaDuration)
                        {
                            IsPlaying = false;
                            EndReached?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                    }
                }, _mediaChangedToken.Token);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                    return;
                _logger.LogError("Unknown error occurred", e);
            }
        }
        #endregion
    }
}
