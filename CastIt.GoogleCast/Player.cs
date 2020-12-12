using CastIt.Domain.Interfaces;
using CastIt.Domain.Models.Device;
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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.GoogleCast
{
    public class Player : IPlayer
    {
        #region Members
        private const string ApplicationId = "CC1AD845";
        private const int GetMediaStatusDelay = 150;
        private const int GetReceiverStatusDelay = 1000;

        private readonly ILogger _logger;
        private readonly ISender _sender;

        private readonly IConnectionChannel _connectionChannel;
        private readonly IHeartbeatChannel _heartbeatChannel;
        private readonly IMediaChannel _mediaChannel;
        private readonly IReceiverChannel _receiverChannel;

        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly string _destinationId;

        private static readonly SupportedMessages _supportedMsgs = new SupportedMessages();
        private CancellationTokenSource _listenerToken;
        private double _seekedSeconds;
        private bool _disposed = false;
        #endregion

        #region Events
        public event EventHandler Disconnected;
        public event EventHandler<DeviceAddedArgs> DeviceAdded;
        public event EventHandler FileLoading;
        public event EventHandler FileLoaded;
        public event EventHandler<double> TimeChanged;
        public event EventHandler<double> PositionChanged;
        public event EventHandler Paused;
        public event EventHandler EndReached;
        public event EventHandler<double> VolumeLevelChanged;
        public event EventHandler<bool> IsMutedChanged;
        public event EventHandler LoadFailed;
        #endregion

        #region Properties
        public static bool CanLogToConsole { get; private set; }
        public static bool CanLogTrace { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }
        public string CurrentContentId { get; private set; }
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
        public double CurrentVolumeLevel { get; private set; }
        public bool IsMuted { get; private set; }

        private SemaphoreSlim ListenMediaChangesSemaphoreSlim { get; } = new SemaphoreSlim(1, 1);
        private SemaphoreSlim ListenReceiverChangesSemaphoreSlim { get; } = new SemaphoreSlim(1, 1);
        #endregion

        #region Constructors
        public Player(
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logToConsole = true,
            bool logTrace = false)
            : this(null, null, destinationId, senderId, logToConsole, logTrace)
        {
        }

        public Player(
            ILogger logger,
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logToConsole = true,
            bool logTrace = false)
            : this(logger, null, destinationId, senderId, logToConsole, logTrace)
        {
        }

        public Player(
            ILogger logger,
            string host,
            int port = 8009,
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logToConsole = true,
            bool logTrace = false)
            : this(logger, Receiver.Default(host, port), destinationId, senderId, logToConsole, logTrace)
        {
        }

        public Player(
            IReceiver receiver,
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logToConsole = true,
            bool logTrace = false)
            : this(null, receiver, destinationId, senderId, logToConsole, logTrace)
        {
        }

        public Player(
            string host,
            int port = 8009,
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logToConsole = true,
            bool logTrace = false)
            : this(null, Receiver.Default(host, port), destinationId, senderId, logToConsole, logTrace)
        {
        }

        public Player(
            ILogger logger,
            IReceiver receiver,
            string destinationId = AppConstants.DESTINATION_ID,
            string senderId = AppConstants.SENDER_ID,
            bool logToConsole = true,
            bool logTrace = false)
        {
            _logger = logger;
            _destinationId = destinationId;
            CanLogToConsole = logToConsole;
            CanLogTrace = logTrace;

            _sender = new Sender(_logger, senderId, receiver, HandleResponseMsg);
            _sender.Disconnected += OnDisconnect;
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

        public void ListenForDevices()
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

        public async Task ConnectAsync(string host, int port)
        {
            await _sender.ConnectAsync(host, port);
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _logger.LogInfo($"{nameof(Dispose)} Disposing subscriptions, events, and disconecting...");
                foreach (var subscription in _subscriptions)
                {
                    subscription.Dispose();
                }

                Disconnected = null;
                DeviceAdded = null;
                TimeChanged = null;
                PositionChanged = null;
                Paused = null;
                EndReached = null;
                VolumeLevelChanged = null;
                IsMutedChanged = null;
                DisconnectAsync().GetAwaiter().GetResult();
            }
            _disposed = true;
        }

        public Task<List<IReceiver>> GetDevicesAsync(TimeSpan scanTime)
        {
            return DeviceLocator.FindReceiversAsync(scanTime);
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
            if (_sender.IsConnected)
                await _receiverChannel.StopAsync(_sender);
            (_receiverChannel as IStatusChannel).Status = null;
            (_mediaChannel as IStatusChannel).Status = null;
            CancelAndSetListenerToken(false);
            _sender.Disconnect(true, false);
            CurrentContentId = null;
            OnDisconnect(this, EventArgs.Empty);
        }
        #endregion

        #region Media Channel
        public async Task<MediaStatus> LoadAsync(
            MediaInformation media,
            bool autoPlay = true,
            double seekedSeconds = 0,
            params int[] activeTrackIds)
        {
            _logger.LogInfo($"{nameof(LoadAsync)}: Trying to load media = {media.ContentId}");
            CurrentContentId = null;
            CancelAndSetListenerToken();

            FileLoading?.Invoke(this, EventArgs.Empty);

            var app = await _receiverChannel.GetApplication(_sender, _connectionChannel, _mediaChannel.Namespace);
            var status = await _mediaChannel.LoadAsync(_sender, app.SessionId, media, autoPlay, activeTrackIds);

            if (status is null)
            {
                LoadFailed?.Invoke(this, EventArgs.Empty);
                _logger.LogWarn($"{nameof(LoadAsync)}: Couldn't load media {media.ContentId}");
                return null;
            }

            CurrentContentId = media.ContentId;
            CurrentMediaDuration = media.Duration ?? status?.Media?.Duration ?? 0;
            CurrentVolumeLevel = status?.Volume?.Level ?? 0;
            IsMuted = status?.Volume?.IsMuted ?? false;
            ElapsedSeconds = 0;
            _seekedSeconds = seekedSeconds;

            TriggerTimeEvents();
            IsPlaying = true;
            IsPaused = false;
            ListenForMediaChanges(_listenerToken.Token);
            ListenForReceiverChanges(_listenerToken.Token);

            FileLoaded?.Invoke(this, EventArgs.Empty);
            _logger.LogInfo($"{nameof(LoadAsync)}: Media = {media.ContentId} was loaded. Duration = {CurrentMediaDuration} - SeekSeconds = {_seekedSeconds}");
            return status;
        }

        public Task<MediaStatus> GetMediaStatusAsync()
        {
            return _mediaChannel.GetStatusAsync(_sender);
        }

        public Task<MediaStatus> PlayAsync()
        {
            IsPlaying = true;
            IsPaused = false;
            return _mediaChannel.PlayAsync(_sender);
        }

        public Task<MediaStatus> PauseAsync()
        {
            IsPlaying = false;
            return _mediaChannel.PauseAsync(_sender);
        }

        public Task<MediaStatus> StopPlaybackAsync()
        {
            CurrentContentId = null;
            IsPlaying = false;
            CancelAndSetListenerToken(false);
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
            _logger.LogTrace($"{nameof(HandleResponseMsg)}: {msg.Namespace} : {payload}");
            var channel = GetChannel(msg.Namespace);
            if (channel == null)
            {
                _logger.LogWarn(
                    $"{nameof(HandleResponseMsg)}: Cant process payload {Environment.NewLine} {payload}. " +
                    $"{Environment.NewLine} No suitable channel was found");
                return;
            }
            var message = JsonConvert.DeserializeObject<MessageWithId>(payload, AppConstants.JsonSettings);
            if (_supportedMsgs.TryGetValue(message.Type, out Type type))
            {
                _logger.LogTrace($"{nameof(HandleResponseMsg)}: Got msg type = {message.Type}");
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
                _logger.LogWarn($"{nameof(HandleResponseMsg)}: Could not get a supported msg for type {message.Type}. Payload = {payload}");
            }
        }

        private void TaskCompletionSourceInvoke(MessageWithId message, string method, object parameter, Type[] types = null)
        {
            if (!message.HasRequestId || !_sender.WaitingTasks.TryRemove(message.RequestId, out object tcs))
                return;
            var tcsType = tcs.GetType();
            var methodToInvoke = types == null ? tcsType.GetMethod(method) : tcsType.GetMethod(method, types);
            var methodParameters = methodToInvoke.GetParameters();
            bool isAsignable = methodParameters.First().ParameterType.IsAssignableFrom(parameter.GetType());

            methodToInvoke.Invoke(tcs, isAsignable ? new[] { parameter } : new object[] { null });
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

            return channels.Find(c => c.Namespace.Equals(ns, StringComparison.OrdinalIgnoreCase));
        }

        private void CancelAndSetListenerToken(bool createNewToken = true)
        {
            _logger.LogInfo($"{nameof(CancelAndSetListenerToken)}: Cancelling listener token... Creating a new token = {createNewToken}");
            if (_listenerToken?.IsCancellationRequested == false)
                _listenerToken.Cancel();

            if (createNewToken)
                _listenerToken = new CancellationTokenSource();
        }

        private async void ListenForMediaChanges(CancellationToken token)
        {
            try
            {
                _logger.LogInfo($"{nameof(ListenForMediaChanges)}: Starting the media changes loop");
                await ListenMediaChangesSemaphoreSlim.WaitAsync(token);
                await Task.Run(async () =>
                {
                    bool checkMediaStatus = true;
                    while (checkMediaStatus || !token.IsCancellationRequested)
                    {
                        await Task.Delay(GetMediaStatusDelay, token);

                        var mediaStatus = await _mediaChannel.GetStatusAsync(_sender);
                        if (token.IsCancellationRequested)
                            break;

                        if (mediaStatus is null)
                        {
                            bool contentIsBeingPlayed = !string.IsNullOrEmpty(CurrentContentId);
                            IsPlaying = false;
                            checkMediaStatus = false;
                            _logger.LogInfo(
                                $"{nameof(ListenForMediaChanges)}: Media is null, content was being played = {contentIsBeingPlayed}, " +
                                $"player was paused = {IsPaused} and we are connected = {_sender.IsConnected}. " +
                                $"CurrentContentId = {CurrentContentId}");
                            CancelAndSetListenerToken(false);

                            //Only call the end reached if we were playing something, the player was not paused and we are connected
                            if (contentIsBeingPlayed && !IsPaused && _sender.IsConnected)
                                EndReached?.Invoke(this, EventArgs.Empty);
                            IsPaused = false;
                            break;
                        }

                        ElapsedSeconds = mediaStatus.CurrentTime + _seekedSeconds;
                        if (mediaStatus.PlayerState == PlayerState.Paused)
                        {
                            IsPaused = true;
                            IsPlaying = false;
                            Paused?.Invoke(this, EventArgs.Empty);
                            continue;
                        }
                        IsPlaying = true;
                        IsPaused = false;
                        TriggerTimeEvents();
                        //If CurrentMediaDuration  <= 0, that means that a live streaming is being played
                        if (CurrentMediaDuration > 0 && Math.Round(ElapsedSeconds, 4) >= Math.Round(CurrentMediaDuration, 4))
                        {
                            _logger.LogInfo(
                                $"{nameof(ListenForMediaChanges)}: End reached because the " +
                                $"ElapsedSeconds = {ElapsedSeconds} is greater / equal to " +
                                $"CurrentMediaDuration = {CurrentMediaDuration}. CurrentContentId = {CurrentContentId}");
                            IsPlaying = false;
                            CancelAndSetListenerToken(false);
                            EndReached?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                    }
                }, token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException || e is OperationCanceledException)
                    return;
                _logger.LogError($"{nameof(ListenForMediaChanges)}: Unknown error occurred", e);
            }
            finally
            {
                _logger.LogInfo($"{nameof(ListenForMediaChanges)}: Media changes loop completed");
                if (ListenMediaChangesSemaphoreSlim.CurrentCount == 0)
                    ListenMediaChangesSemaphoreSlim.Release();
            }
        }

        private async void ListenForReceiverChanges(CancellationToken token)
        {
            try
            {
                _logger.LogInfo($"{nameof(ListenForReceiverChanges)}: Starting the receiver changes loop");
                await ListenReceiverChangesSemaphoreSlim.WaitAsync(token);
                await Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        await Task.Delay(GetReceiverStatusDelay, token);
                        var status = await _receiverChannel.GetStatusAsync(_sender);
                        if (token.IsCancellationRequested)
                            break;
                        TriggerVolumeEvents(status.Volume?.Level ?? 0, status.Volume?.IsMuted ?? false);
                    }
                }, token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException || e is OperationCanceledException)
                    return;
                _logger.LogError($"{nameof(ListenForReceiverChanges)}: Unknown error occurred", e);
            }
            finally
            {
                _logger.LogInfo($"{nameof(ListenForMediaChanges)}: Receiver changes loop completed");
                if (ListenReceiverChangesSemaphoreSlim.CurrentCount == 0)
                    ListenReceiverChangesSemaphoreSlim.Release();
            }
        }

        private void TriggerTimeEvents()
        {
            TimeChanged?.Invoke(this, ElapsedSeconds);
            PositionChanged?.Invoke(this, PlayedPercentage);
        }

        private void TriggerVolumeEvents(double newLevel, bool newIsMuted)
        {
            if (newLevel != CurrentVolumeLevel)
            {
                CurrentVolumeLevel = newLevel;
                VolumeLevelChanged?.Invoke(this, CurrentVolumeLevel);
            }

            if (newIsMuted != IsMuted)
            {
                IsMuted = newIsMuted;
                IsMutedChanged?.Invoke(this, IsMuted);
            }
        }

        private void OnDisconnect(object sender, EventArgs e)
        {
            Disconnected?.Invoke(this, e);
            IsPlaying = false;
            _receiverChannel.IsConnected = false;
        }
        #endregion
    }
}
