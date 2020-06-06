using System;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Common.Extensions
{
    public static class EnumerableExtension
    {
        private static readonly Random rnd = new Random();

        public static T PickRandom<T>(this IEnumerable<T> source, int exceptIndex)
        {
            if (source.Count() == 1)
                return source.ElementAt(0);

            int index = rnd.Next(source.Count());
            while (index == exceptIndex)
            {
                index = rnd.Next(source.Count());
            }
            return source.ElementAtOrDefault(index);
        }
    }
}
