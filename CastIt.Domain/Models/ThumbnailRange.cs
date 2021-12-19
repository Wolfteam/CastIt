namespace CastIt.Domain.Models
{
    public class ThumbnailRange
    {
        public Range<long> Range { get; }
        public byte[] Image { get; private set; }
        public bool HasImage
            => Image != null;
        public bool IsBeingGenerated { get; private set; }

        public ThumbnailRange(long minimum, long maximum, int index)
        {
            Range = new Range<long>(minimum, maximum, index);
        }

        public ThumbnailRange Generating()
        {
            IsBeingGenerated = true;
            return this;
        }

        public ThumbnailRange Generated()
        {
            IsBeingGenerated = false;
            return this;
        }

        public ThumbnailRange SetImage(byte[] image)
        {
            Image = image;
            return this;
        }
    }

}
