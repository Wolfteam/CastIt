using System;
using System.Drawing;

namespace CastIt.GoogleCast.Extensions
{
    internal static class ColorExtensions
    {
        public static Color FromHexString(this string color)
        {
            return Color.FromArgb(
                 Convert.ToInt32(color.Substring(7, 2), 16),
                 Convert.ToInt32(color.Substring(1, 2), 16),
                 Convert.ToInt32(color.Substring(3, 2), 16),
                 Convert.ToInt32(color.Substring(5, 2), 16));
        }
        public static Color? FromNullableHexString(this string color)
        {
            if (string.IsNullOrEmpty(color))
            {
                return null;
            }

            return FromHexString(color);
        }

        public static string ToHexString(this Color color)
        {
            return $"#{color.R.ToString("X2", null)}{color.G.ToString("X2", null)}{color.B.ToString("X2", null)}{color.A.ToString("X2", null)}";
        }

        public static string ToHexString(this Color? color)
        {
            return color == null ? null : ToHexString((Color)color);
        }
    }
}
