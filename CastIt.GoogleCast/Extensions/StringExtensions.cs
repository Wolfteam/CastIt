using System;
using System.Text;

namespace CastIt.GoogleCast.Extensions
{
    internal static class StringExtensions
    {
        public static string ToUnderscoreUpperInvariant(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var stringBuilder = new StringBuilder();
            var first = true;
            foreach (var c in str)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (Char.IsUpper(c))
                    {
                        stringBuilder.AppendFormat("_{0}", c);
                        continue;
                    }
                }
                stringBuilder.Append(Char.ToUpperInvariant(c));
            }
            return stringBuilder.ToString();
        }

        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var stringBuilder = new StringBuilder();
            var underscore = true;
            foreach (var c in str)
            {
                if (underscore)
                {
                    underscore = false;
                    stringBuilder.Append(Char.ToUpperInvariant(c));
                }
                else
                {
                    if (c == '_')
                    {
                        underscore = true;
                    }
                    else
                    {
                        stringBuilder.Append(Char.ToLowerInvariant(c));
                    }
                }
            };
            return stringBuilder.ToString();
        }

        public static string ReplaceAt(this string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }
    }
}
