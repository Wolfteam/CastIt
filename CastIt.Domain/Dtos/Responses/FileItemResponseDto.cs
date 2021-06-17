using System;
using System.Collections.Generic;

namespace CastIt.Domain.Dtos.Responses
{
    public class FileItemResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double TotalSeconds { get; set; }
        public string Path { get; set; }
        public int Position { get; set; }
        public double PlayedPercentage { get; set; }
        public long PlayListId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Loop { get; set; }

        public bool IsBeingPlayed { get; set; }
        public bool IsLocalFile { get; set; }
        public bool IsUrlFile { get; set; }
        public double PlayedSeconds { get; set; }
        public bool CanStartPlayingFromCurrentPercentage { get; set; }
        public bool WasPlayed { get; set; }
        public bool IsCached { get; set; }

        public bool Exists { get; set; }
        public string Filename { get; set; }
        public string Size { get; set; }
        public string Extension { get; set; }

        public string SubTitle { get; set; }
        public string Resolution { get; set; }
        public string Duration { get; set; }
        public string PlayedTime { get; set; }
        public string TotalDuration { get; set; }
        public string FullTotalDuration { get; set; }
        public string ThumbnailUrl { get; set; }

        public List<FileItemOptionsResponseDto> CurrentFileVideos { get; set; }
            = new List<FileItemOptionsResponseDto>();
        public List<FileItemOptionsResponseDto> CurrentFileAudios { get; set; }
            = new List<FileItemOptionsResponseDto>();
        public List<FileItemOptionsResponseDto> CurrentFileSubTitles { get; set; }
            = new List<FileItemOptionsResponseDto>();
        public List<FileItemOptionsResponseDto> CurrentFileQualities { get; set; }
            = new List<FileItemOptionsResponseDto>();
        public int CurrentFileVideoStreamIndex { get; set; }
        public int CurrentFileAudioStreamIndex { get; set; }
        public int CurrentFileSubTitleStreamIndex { get; set; }
        public int CurrentFileQuality { get; set; }
    }
}
