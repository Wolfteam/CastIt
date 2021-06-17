using CastIt.Domain.Interfaces;
using CastIt.Domain.Models.Device;
using CastIt.GoogleCast.Extensions;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Messages;
using CastIt.GoogleCast.Messages.Receiver;
using CastIt.GoogleCast.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.GoogleCast
{
    internal class Sender : ISender
    {
        private const int RECEIVE_TIMEOUT = 30000;

        private readonly ILogger _logger;
        private readonly string _senderId;
        private readonly Func<CastMessage, Task> _onResponseMsg;

        public event EventHandler Disconnected;

        public IReceiver CurrentReceiver { get; private set; }
        private SslStream NetworkStream { get; set; }
        private TcpClient TcpClient { get; set; }
        private SemaphoreSlim SendSemaphoreSlim { get; } = new SemaphoreSlim(1, 1);
        private SemaphoreSlim EnsureConnectionSemaphoreSlim { get; } = new SemaphoreSlim(1, 1);
        private CancellationTokenSource CancellationTokenSource { get; set; }
        public ConcurrentDictionary<int, object> WaitingTasks { get; } = new ConcurrentDictionary<int, object>();
        public bool IsConnected
            => NetworkStream != null;

        public Sender(
            ILogger logger,
            string senderId,
            string host,
            int port,
            Func<CastMessage, Task> onResponseMsg)
            : this(logger, senderId, new Receiver
            {
                Host = host,
                Port = port,
                FriendlyName = "N/A",
                Id = "N/A"
            }, onResponseMsg)
        {
        }

        public Sender(
            ILogger logger,
            string senderId,
            IReceiver receiver,
            Func<CastMessage, Task> onResponseMsg)
        {
            _logger = logger;
            _senderId = senderId;
            CurrentReceiver = receiver;
            _onResponseMsg = onResponseMsg;
        }

        public async Task ConnectAsync()
        {
            _logger.LogInfo(
                $"{nameof(ConnectAsync)}: Connecting to = {CurrentReceiver.FriendlyName} - " +
                $"{CurrentReceiver.Host}:{CurrentReceiver.Port}");
            Disconnect(false, false);

            TcpClient = new TcpClient();
            await TcpClient.ConnectAsync(CurrentReceiver.Host, CurrentReceiver.Port);

            var secureStream = new SslStream(TcpClient.GetStream(), true, (_, __, ___, ____) => true);
            await secureStream.AuthenticateAsClientAsync(CurrentReceiver.Host);
            NetworkStream = secureStream;

            CurrentReceiver.IsConnected = true;

            CancellationTokenSource = new CancellationTokenSource();
            Receive(CancellationTokenSource.Token);
        }

        public Task ConnectAsync(string host, int port)
        {
            CurrentReceiver = new Receiver
            {
                Host = host,
                Port = port,
                FriendlyName = "N/A",
                Type = "N/A",
                Id = "N/A"
            };
            return ConnectAsync();
        }

        public Task ConnectAsync(IReceiver receiver)
        {
            CurrentReceiver = receiver;
            return ConnectAsync();
        }

        public void Disconnect(bool triggerDisconnectEvent, bool chromeCastAskedForDisconnection)
        {
            if (chromeCastAskedForDisconnection)
                _logger.LogWarn($"{nameof(Disconnect)}: A dispose will take place and was requested from the chromecast");
            Dispose(triggerDisconnectEvent);
        }

        public Task SendAsync(AppMessage msg)
        {
            return SendAsync(msg.NameSpace, msg.Message, msg.DestinationId);
        }

        public Task SendAsync(string ns, IMessage message, string destinationId)
        {
            var castMessage = CreateCastMessage(ns, destinationId);
            castMessage.PayloadUtf8 = JsonConvert.SerializeObject(message, AppConstants.JsonSettings);
            return SendAsync(castMessage);
        }

        public async Task<TResponse> SendAsync<TResponse>(string ns, IMessageWithId message, string destinationId)
            where TResponse : IMessageWithId
        {
            var taskCompletionSource = new TaskCompletionSource<TResponse>();
            WaitingTasks[message.RequestId] = taskCompletionSource;
            await SendAsync(ns, message, destinationId);
            if (message is StopMessage)
            {
                return default;
            }
            return await taskCompletionSource.Task.TimeoutAfter(RECEIVE_TIMEOUT);
        }

        private async Task SendAsync(CastMessage castMessage)
        {
            await EnsureConnection();

            try
            {
                await SendSemaphoreSlim.WaitAsync();
                _logger.LogTrace($"{nameof(SendAsync)}: {castMessage.DestinationId}: {castMessage.PayloadUtf8}");

                byte[] message;
                await using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, castMessage);
                    message = ms.ToArray();
                }
                var header = BitConverter.GetBytes(message.Length);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(header);
                }
                await NetworkStream?.WriteAsync(header, 0, header.Length);
                await NetworkStream?.WriteAsync(message, 0, message.Length);
                await NetworkStream?.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(SendAsync)}: Unknown error", ex);
            }
            finally
            {
                SendSemaphoreSlim.Release();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Dispose(bool triggerDisconnectEvent)
        {
            _logger.LogInfo($"{nameof(Dispose)}: Disposing...");
            foreach (var kvp in WaitingTasks)
            {
                var tcsType = kvp.Value.GetType();
                var methodToInvoke = tcsType.GetMethod("SetResult");
                methodToInvoke?.Invoke(kvp.Value, new object[] { null });
                _logger.LogInfo($"{nameof(Dispose)}: Calling set result for a pending task...");
            }
            WaitingTasks.Clear();
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = null;
            NetworkStream?.Dispose();
            TcpClient?.Dispose();
            NetworkStream = null;
            TcpClient = null;

            if (triggerDisconnectEvent)
            {
                CurrentReceiver = null;
                _logger.LogInfo($"{nameof(Dispose)}: Triggering disconnected event");
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
            _logger.LogInfo($"{nameof(Dispose)}: Dispose completed");
        }

        private async Task EnsureConnection()
        {
            if (TcpClient == null && CurrentReceiver != null)
            {
                await EnsureConnectionSemaphoreSlim.WaitAsync();
                try
                {
                    if (TcpClient == null && CurrentReceiver != null)
                    {
                        _logger.LogInfo($"{nameof(EnsureConnection)} TcpClient is null, connecting...");
                        await ConnectAsync();
                    }
                }
                finally
                {
                    EnsureConnectionSemaphoreSlim.Release();
                }
            }
        }

        private async void Receive(CancellationToken cancellationToken)
        {
            _logger.LogInfo($"{nameof(Receive)}: Starting the receive loop");
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var buffer = await ReadAsync(4, cancellationToken);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(buffer);
                    }
                    var length = BitConverter.ToInt32(buffer, 0);
                    CastMessage castMessage;
                    await using (var ms = new MemoryStream())
                    {
                        var bytes = await ReadAsync(length, cancellationToken);
                        await ms.WriteAsync(bytes, 0, length, cancellationToken);
                        ms.Position = 0;
                        castMessage = Serializer.Deserialize<CastMessage>(ms);
                    }

                    await _onResponseMsg.Invoke(castMessage);
                }
            }
            catch (Exception e)
            {
                if (e is NullReferenceException || e is OperationCanceledException)
                    return;
                _logger.LogError($"{nameof(Receive)}: Unknown error, triggering a dispose", e);
                Dispose(true);
            }
            finally
            {
                _logger.LogInfo($"{nameof(Receive)}: Receive loop completed");
            }
        }

        private CastMessage CreateCastMessage(string ns, string destinationId)
        {
            return new CastMessage()
            {
                Namespace = ns,
                SourceId = _senderId,
                DestinationId = destinationId
            };
        }

        private async Task<byte[]> ReadAsync(int bufferLength, CancellationToken cancellationToken)
        {
            var buffer = new byte[bufferLength];
            int nb, length = 0;
            while (length < bufferLength)
            {
                nb = await NetworkStream.ReadAsync(buffer, length, bufferLength - length, cancellationToken);
                //a disconnection occured
                if (nb == 0 && !NetworkStream.CanRead)
                {
                    _logger.LogInfo($"{nameof(ReadAsync)}: End of stream reached");
                    throw new Exception("Number of bytes read is 0, end of stream reached");
                }
                length += nb;
            }
            return buffer;
        }
    }
}
