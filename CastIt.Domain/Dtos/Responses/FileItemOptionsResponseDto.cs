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


        public static FileItemOptionsResponseDto ForVideo(int id, bool isSelected, bool isEnabled, string text)
        {
            return new FileItemOptionsResponseDto
            {
                Id = id,
                IsSelected = isSelected,
                IsEnabled = isEnabled,
                IsVideo = true,
                Text = text
            };
        }

        public static FileItemOptionsResponseDto ForAudio(int id, bool isSelected, bool isEnabled, string text)
        {
            return new FileItemOptionsResponseDto
            {
                Id = id,
                IsSelected = isSelected,
                IsEnabled = isEnabled,
                IsAudio = true,
                Text = text
            };
        }

        public static FileItemOptionsResponseDto ForLocalSubtitles(int id, string text, string path)
        {
            return new FileItemOptionsResponseDto
            {
                Id = id,
                IsSelected = true,
                IsEnabled = true,
                IsSubTitle = true,
                Text = text,
                Path = path
            };
        }

        public static FileItemOptionsResponseDto ForEmbeddedSubtitles(int id, bool isSelected, bool isEnabled, string text)
        {
            return new FileItemOptionsResponseDto
            {
                Id = id,
                IsSelected = isSelected,
                IsEnabled = isEnabled,
                IsSubTitle = true,
                Text = text
            };
        }
    }
}
