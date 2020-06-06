using CastIt.GoogleCast.Extensions;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Messages;
using CastIt.GoogleCast.Messages.Receiver;
using CastIt.GoogleCast.Models;
using CastIt.GoogleCast.Models.Receiver;
using MvvmCross.Logging;
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

        private readonly IMvxLog _logger;
        private readonly string _senderId;
        private readonly Func<CastMessage, Task> _onResponseMsg;

        public event EventHandler Disconnected;

        public IReceiver CurrentReceiver { get; private set; }
        private Stream NetworkStream { get; set; }
        private TcpClient TcpClient { get; set; }
        private SemaphoreSlim SendSemaphoreSlim { get; } = new SemaphoreSlim(1, 1);
        private SemaphoreSlim EnsureConnectionSemaphoreSlim { get; } = new SemaphoreSlim(1, 1);
        private CancellationTokenSource CancellationTokenSource { get; set; }
        public ConcurrentDictionary<int, object> WaitingTasks { get; } = new ConcurrentDictionary<int, object>();
        public bool IsConnected
            => NetworkStream != null;

        public Sender(
            IMvxLog logger,
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
            IMvxLog logger,
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
            Disconnect(false);

            TcpClient = new TcpClient();
            await TcpClient.ConnectAsync(CurrentReceiver.Host, CurrentReceiver.Port);

            var secureStream = new SslStream(TcpClient.GetStream(), true, (a, b, c, d) => true);
            await secureStream.AuthenticateAsClientAsync(CurrentReceiver.Host);
            NetworkStream = secureStream;

            CancellationTokenSource = new CancellationTokenSource();
            Receive(CancellationTokenSource.Token);
        }

        public Task ConnectAsync(IReceiver receiver)
        {
            CurrentReceiver = receiver;
            return ConnectAsync();
        }

        public void Disconnect(bool triggerDisconnectEvent)
        {
            Dispose(triggerDisconnectEvent);
        }

        public Task SendAsync(AppMessage msg)
        {
            return SendAsync(msg.NameSpace, msg.Message, msg.DestinationId);
        }

        public async Task SendAsync(string ns, IMessage message, string destinationId)
        {
            var castMessage = CreateCastMessage(ns, destinationId);
            castMessage.PayloadUtf8 = JsonConvert.SerializeObject(message, AppConstants.JsonSettings);
            await SendAsync(castMessage);
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
                using (var ms = new MemoryStream())
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
            foreach (var kvp in WaitingTasks)
            {
                var tcsType = kvp.Value.GetType();
                var methodToInvoke = tcsType.GetMethod("SetResult");
                methodToInvoke.Invoke(kvp.Value, new object[] { null });
                _logger.LogInfo("Calling set result...");
            }
            WaitingTasks.Clear();
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = null;
            NetworkStream?.Dispose();
            TcpClient?.Dispose();
            NetworkStream = null;
            TcpClient = null;

            if (triggerDisconnectEvent)
                Disconnected?.Invoke(this, EventArgs.Empty);
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
                    using (var ms = new MemoryStream())
                    {
                        await ms.WriteAsync(await ReadAsync(length, cancellationToken), 0, length, cancellationToken);
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
                _logger.LogError($"{nameof(Receive)}: Unknown error", e);
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
                if (nb == 0)
                {
                    throw new Exception("Number of bytes read is 0");
                }
                length += nb;
            }
            return buffer;
        }
    }
}
