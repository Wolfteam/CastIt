using System;

namespace CastIt.Common.Extensions
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

        public static string AppendDelimitator(this string input, string delimitator, params string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (string.IsNullOrEmpty(arg))
                    continue;
                input += $" {delimitator} {arg}";
            }

            return input;
        }
    }
}
