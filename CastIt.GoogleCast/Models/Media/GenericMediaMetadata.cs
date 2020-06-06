using CastIt.GoogleCast.Enums;
using System.Collections.Generic;

namespace CastIt.GoogleCast.Models.Media
{
    public class GenericMediaMetadata
    {
        public GenericMediaMetadata()
        {
            MetadataType = MetadataType.Default;
        }

        public MetadataType MetadataType { get; protected set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public List<Image> Images { get; set; }
            = new List<Image>();
    }
}
