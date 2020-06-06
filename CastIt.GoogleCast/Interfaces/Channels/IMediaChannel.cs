using CastIt.GoogleCast.Enums;
using CastIt.GoogleCast.Models.Media;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Interfaces.Channels
{
    internal interface IMediaChannel : IStatusChannel<List<MediaStatus>>
    {
        Task<MediaStatus> GetStatusAsync(ISender sender);

        Task<MediaStatus> LoadAsync(
            ISender sender,
            string sessionId,
            MediaInformation media,
            bool autoPlay = true,
            params int[] activeTrackIds);

        Task<MediaStatus> QueueLoadAsync(ISender sender, RepeatMode repeatMode, params MediaInformation[] medias);

        Task<MediaStatus> QueueLoadAsync(ISender sender, RepeatMode repeatMode, params QueueItem[] queueItems);

        Task<MediaStatus> EditTracksInfoAsync(ISender sender, string language = null, bool enabledTextTracks = true, params int[] activeTrackIds);

        Task<MediaStatus> PlayAsync(ISender sender);

        Task<MediaStatus> PauseAsync(ISender sender);

        Task<MediaStatus> StopAsync(ISender sender);

        Task<MediaStatus> SeekAsync(ISender sender, double seconds);
    }
}
