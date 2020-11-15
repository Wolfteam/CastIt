namespace CastIt.Domain.Dtos.Responses
{
    public class FileItemOptionsResponseDto
    {
        public int Id { get; set; }
        public bool IsVideo { get; set; }
        public bool IsAudio { get; set; }
        public bool IsSubTitle { get; set; }
        public bool IsQuality { get; set; }
        public string Path { get; set; }
        public string Text { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEnabled { get; set; }
    }
}
