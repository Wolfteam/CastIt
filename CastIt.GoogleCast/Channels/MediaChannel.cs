using CastIt.GoogleCast.Enums;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Interfaces.Channels;
using CastIt.GoogleCast.Interfaces.Messages;
using CastIt.GoogleCast.Messages.Base;
using CastIt.GoogleCast.Messages.Media;
using CastIt.GoogleCast.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Channels
{
    internal class MediaChannel : StatusChannel<List<MediaStatus>, MediaStatusMessage>, IMediaChannel
    {
        private readonly Func<Task<string>> _getCurrentSessionId;

        public MediaChannel(string destinationId, Func<Task<string>> getCurrentSessionId) : base("media", destinationId)
        {
            _getCurrentSessionId = getCurrentSessionId;
        }

        public Task<MediaStatus> GetStatusAsync(ISender sender)
        {
            var msg = new GetStatusMessage() { MediaSessionId = Status?.First().MediaSessionId };
            return SendAndSetSessionIdAsync(sender, msg, false);
        }

        public async Task<MediaStatus> LoadAsync(
            ISender sender,
            string sessionId,
            MediaInformation media,
            bool autoPlay = true,
            params int[] activeTrackIds)
        {
            var msg = new LoadMessage()
            {
                Media = media,
                AutoPlay = autoPlay,
                ActiveTrackIds = activeTrackIds.ToList(),
                SessionId = sessionId
            };
            return await SendAsync(sender, msg);
        }

        public Task<MediaStatus> QueueLoadAsync(ISender sender, RepeatMode repeatMode, params MediaInformation[] medias)
        {
            return QueueLoadAsync(sender, repeatMode, medias.Select(mi => new QueueItem() { Media = mi }).ToList());
        }

        public Task<MediaStatus> QueueLoadAsync(ISender sender, RepeatMode repeatMode, params QueueItem[] queueItems)
        {
            return QueueLoadAsync(sender, repeatMode, queueItems.ToList());
        }

        private Task<MediaStatus> QueueLoadAsync(ISender sender, RepeatMode repeatMode, List<QueueItem> queueItems)
        {
            return SendAsync(sender, new QueueLoadMessage
            {
                RepeatMode = repeatMode,
                Items = queueItems
            });
        }

        public Task<MediaStatus> EditTracksInfoAsync(ISender sender, string language = null, bool enabledTextTracks = true, params int[] activeTrackIds)
        {
            return SendAndSetSessionIdAsync(sender, new EditTracksInfoMessage()
            {
                Language = language,
                EnableTextTracks = enabledTextTracks,
                ActiveTrackIds = activeTrackIds.ToList()
            });
        }

        public Task<MediaStatus> PlayAsync(ISender sender)
        {
            return SendAndSetSessionIdAsync(sender, new PlayMessage());
        }

        public Task<MediaStatus> PauseAsync(ISender sender)
        {
            return SendAndSetSessionIdAsync(sender, new PauseMessage());
        }

        public Task<MediaStatus> StopAsync(ISender sender)
        {
            return SendAndSetSessionIdAsync(sender, new StopMessage());
        }

        public Task<MediaStatus> SeekAsync(ISender sender, double seconds)
        {
            return SendAndSetSessionIdAsync(sender, new SeekMessage() { CurrentTime = seconds });
        }

        private Task<MediaStatus> SendAndSetSessionIdAsync(ISender sender, MediaSessionMessage message, bool mediaSessionIdRequired = true)
        {
            var mediaSessionId = Status?.FirstOrDefault()?.MediaSessionId;
            if (mediaSessionIdRequired && mediaSessionId == null)
            {
                return Task.FromResult<MediaStatus>(null);
            }
            message.MediaSessionId = mediaSessionId;
            return SendAsync(sender, message);
        }

        private async Task<MediaStatus> SendAsync(ISender sender, IMessageWithId message)
        {
            try
            {
                var sessionId = await _getCurrentSessionId.Invoke();
                var response = await sender.SendAsync<MediaStatusMessage>(Namespace, message, sessionId);
                return response.Status.FirstOrDefault();
            }
            catch (Exception)
            {
                ((IStatusChannel)this).Status = null;
                throw;
            }
        }
    }
}
