namespace CastIt.Domain.Dtos.Requests
{
    public class SetMultiFileOptionsRequestDto
    {
        public int AudioStreamIndex { get; set; }
        public int SubtitleStreamIndex { get; set; }
        public int Quality { get; set; }
    }
}
