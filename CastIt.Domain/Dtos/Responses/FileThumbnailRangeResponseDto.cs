using CastIt.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Domain.Dtos.Responses
{
    public class FileThumbnailRangeResponseDto
    {
        public string PreviewThumbnailUrl { get; set; }
        public Range<long> ThumbnailRange { get; set; }
        public List<FileThumbnailPositionResponseDto> ThumbnailPositions { get; set; } = new List<FileThumbnailPositionResponseDto>();

        public void SetMatrixOfSeconds(int items)
        {
            SetMatrixOfSeconds(items, ThumbnailRange.Minimum);
        }

        public void SetMatrixOfSeconds(int items, long startingSecond)
        {
            for (int y = 0; y < items; y++)
            {
                for (int x = 0; x < items; x++)
                {
                    ThumbnailPositions.Add(new FileThumbnailPositionResponseDto(x, y, startingSecond));
                    startingSecond++;
                }
            }
        }

        public FileThumbnailPositionResponseDto GetPosition(int x, int y)
            => ThumbnailPositions.First(p => p.X == x && p.Y == y);

        public FileThumbnailPositionResponseDto GetPositionBySecond(long second)
        {
            var position = ThumbnailPositions.Find(x => x.Second == second);
            //The seconds goes from [Range.Min...Range.Max - 1]
            return position ?? ThumbnailPositions.Find(x => x.Second == second - 1);
        }

        public long GetSecond(int x, int y)
            => GetPosition(x, y).Second;
    }

    public class FileThumbnailPositionResponseDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public long Second { get; set; }

        public FileThumbnailPositionResponseDto()
        {
        }

        public FileThumbnailPositionResponseDto(int x, int y, long second)
        {
            X = x;
            Y = y;
            Second = second;
        }
    }
}
