using System;

namespace CastIt.GoogleCast.Extensions
{
    internal static class EnumExtensions
    {
        public static T Parse<T>(this string enumString) where T : struct, IConvertible
        {
            return (T)Enum.Parse(typeof(T), enumString.ToCamelCase(), true);
        }

        public static T? ParseNullable<T>(this string enumString) where T : struct, IConvertible
        {
            return string.IsNullOrEmpty(enumString) ? (T?)null : Parse<T>(enumString);
        }

        public static string GetName<T>(this T value) where T : struct, IConvertible
        {
            return Enum.GetName(typeof(T), value).ToUnderscoreUpperInvariant();
        }

        public static string GetName<T>(this T? value) where T : struct, IConvertible
        {
            return value == null ? null : GetName((T)value);
        }
    }
}
