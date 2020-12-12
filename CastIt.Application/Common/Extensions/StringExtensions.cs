using System;
using System.Linq;

namespace CastIt.Application.Common.Extensions
{
    public static class StringExtensions
    {
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

        public static string AppendDelimiter(this string input, string delimiter, params string[] args)
        {
            return args.Where(arg => !string.IsNullOrEmpty(arg)).Aggregate(input, (current, arg) => current + $" {delimiter} {arg}");
        }
    }
}
