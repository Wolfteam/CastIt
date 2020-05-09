using System;
using System.Drawing;

namespace CastIt.Common.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToColor(this string hex)
        {
            hex = hex.Replace("#", string.Empty);
            if (hex.Length > 6)
            {
                byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
                byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
                byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
                byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
                return Color.FromArgb(a, r, g, b);
            }
            else
            {
                var r = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16);
                var g = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16);
                var b = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16);
                return Color.FromArgb(255, r, g, b);
            }
        }

        private static float Lerp(this float start, float end, float amount)
        {
            float difference = end - start;
            float adjusted = difference * amount;
            return start + adjusted;
        }

        public static Color LerpLight(this Color color, float amount)
            => Lerp(color, Color.White, amount);

        public static Color LerpDark(this Color color, float amount)
            => Lerp(color, Color.Black, amount);

        public static Color Lerp(this Color colour, Color to, float amount)
        {
            // start colours as lerp-able floats
            float sr = colour.R, sg = colour.G, sb = colour.B;

            // end colours as lerp-able floats
            float er = to.R, eg = to.G, eb = to.B;

            // lerp the colours to get the difference
            byte r = (byte)sr.Lerp(er, amount),
                 g = (byte)sg.Lerp(eg, amount),
                 b = (byte)sb.Lerp(eb, amount);

            // return the new colour
            return Color.FromArgb(r, g, b);
        }

        public static System.Windows.Media.Color ToMediaColor(this string hex)
        {
            hex = hex.Replace("#", string.Empty);
            if (hex.Length > 6)
            {
                byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
                byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
                byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
                byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
                return System.Windows.Media.Color.FromArgb(a, r, g, b);
            }
            else
            {
                var r = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16);
                var g = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16);
                var b = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16);
                return System.Windows.Media.Color.FromArgb(255, r, g, b);
            }
        }

        public static string ToHexString(this Color c)
            => string.Format("#{0:X6}", c.ToArgb() & 0x00FFFFFF);
    }
}
