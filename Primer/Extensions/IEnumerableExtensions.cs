using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public static class IEnumerableExtensions
    {
        [Pure]
        public static IEnumerable<(int index, T value)> WithIndex<T>(this IEnumerable<T> items)
        {
            return items.Select((value, i) => (i, value));
        }

        [Pure]
        public static T[] Append<T>(this T[] self, params T[] other) {
            var result = new T[self.Length + other.Length];
            self.CopyTo(result, 0);
            other.CopyTo(result, self.Length);
            return result;
        }

        [Pure]
        public static T RandomItem<T>(this IEnumerable<T> enumerable, Rng rng = null)
        {
            var list = enumerable as List<T> ?? enumerable.ToList();
            var index = rng.Range(0, list.Count);
            return list[index];
        }

        #region enumerable.Shuffle()
        public static List<T> Shuffle<T>(this IEnumerable<T> self, Rng rng = null)
        {
            var list = new List<T>(self);
            Randomize(list, rng);
            return list;
        }

        // Pulled from https://stackoverflow.com/a/1262619/3920202
        private static void Randomize<T>(IList<T> list, Rng rng)
        {
            var n = list.Count;

            while (n > 1) {
                n--;
                var k = rng.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
        #endregion

        #region enumerable.MaxBy(x => x.value);
        [Pure]
        public static TSource MaxBy<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var list = source.ToList();

            if (list.Count is 0)
                return default(TSource);

            var max = list[0];
            var maxVal = selector(max);

            foreach (var item in list.Skip(1)) {
                var key = selector(item);

                if (key > maxVal) {
                    max = item;
                    maxVal = key;
                }
            }

            return max;
        }
        #endregion

        #region enumerable.IsSame(otherEnumerable);
        [Pure]
        public static bool IsSame<T>(this IEnumerable<T> left, IEnumerable<T> right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.SequenceEqual(right);
        }
        #endregion

        #region enumerable.SpaceEvenly(distanceBetweenChildren);
        [Pure]
        public static IEnumerable<(T, float)> SpaceEvenly<T>(this IEnumerable<T> items, float distanceBetweenChildren)
        {
            var children = items.ToArray();

            for (var i = 0; i < children.Length; i++) {
                var y = GetEvenlySpacedFloatGivenStep(i, children.Length, distanceBetweenChildren);
                yield return (children[i], y);
            }
        }

        private static float GetEvenlySpacedFloatGivenStep(int objectIndex, int totalObjects, float distanceBetweenObjects)
        {
            var totalSpace = distanceBetweenObjects * (totalObjects - 1);
            var min = -totalSpace / 2;
            return min + objectIndex * distanceBetweenObjects;
        }
        #endregion

        #region var (a, b, c) = enumerable;
        public static void Deconstruct<T>(this IEnumerable<T> self, out T first)
        {
            var list = self.ToList();
            first = list[0];
        }
        public static void Deconstruct<T>(this IEnumerable<T> self, out T first, out T second)
        {
            var list = self.ToList();
            first = list[0];
            second = list[1];
        }
        public static void Deconstruct<T>(this IEnumerable<T> self, out T first, out T second, out T third)
        {
            var list = self.ToList();
            first = list[0];
            second = list[1];
            third = list[2];
        }
        #endregion
    }
}
