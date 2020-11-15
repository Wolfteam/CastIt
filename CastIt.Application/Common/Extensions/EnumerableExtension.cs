using System;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Application.Common.Extensions
{
    public static class EnumerableExtension
    {
        private static readonly Random rnd = new Random();

        public static T PickRandom<T>(this IEnumerable<T> source, int exceptIndex)
        {
            var enumerable = source.ToList();
            if (enumerable.Count() == 1)
                return enumerable.ElementAt(0);

            int index = rnd.Next(enumerable.Count());
            while (index == exceptIndex)
            {
                index = rnd.Next(enumerable.Count());
            }
            return enumerable.ElementAtOrDefault(index);
        }

        public static int GetClosest(this IEnumerable<int> source, int closestTo)
        {
            return source.Aggregate((x, y) => Math.Abs(x - closestTo) < Math.Abs(y - closestTo) ? x : y);
        }
    }
}
