using CastIt.GoogleCast.Enums;

namespace CastIt.GoogleCast.Models.Media
{
    public class MusicTrackMediaMetadata : GenericMediaMetadata
    {
        public MusicTrackMediaMetadata()
        {
            MetadataType = MetadataType.Music;
        }

        public string AlbumArtist { get; set; }

        public string AlbumName { get; set; }

        public string Artist { get; set; }

        public string Composer { get; set; }

        public int? DiscNumber { get; set; }

        public string ReleaseDate { get; set; }

        public string SongName { get; set; }

        public int? TrackNumber { get; set; }
    }
}
