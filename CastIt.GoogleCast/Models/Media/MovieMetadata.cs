using CastIt.GoogleCast.Enums;

namespace CastIt.GoogleCast.Models.Media
{
    public class MovieMetadata : GenericMediaMetadata
    {
        public MovieMetadata()
        {
            MetadataType = MetadataType.Movie;
        }

        public string Studio { get; set; }
    }
}
