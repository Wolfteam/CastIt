﻿using CastIt.Application.Common;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Test.Models
{
    public class ServerPlayList
    {
        public long Id { get; set; }
        public int Position { get; set; }
        public string Name { get; set; }
        public bool Loop { get; set; }
        public bool Shuffle { get; set; }

        public int NumberOfFiles
            => Files.Count;
        //public ServerFileItem SelectedFileItem { get; set; }

        public List<ServerFileItem> Files { get; set; }
            = new List<ServerFileItem>();

        public string PlayedTime
        {
            get
            {
                var playedSeconds = Files.Sum(i => i.PlayedSeconds);
                var formatted = FileFormatConstants.FormatDuration(playedSeconds);
                return $"{formatted}";
            }
        }

        public string TotalDuration
        {
            get
            {
                var totalSeconds = Files.Where(i => i.TotalSeconds >= 0).Sum(i => i.TotalSeconds);
                var formatted = FileFormatConstants.FormatDuration(totalSeconds);
                return $"{PlayedTime} / {formatted}";
            }
        }
    }
}
