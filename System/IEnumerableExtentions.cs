using System;
using System.Collections.Generic;
using System.Linq;

namespace NewWordLearner.System
{
    public static class IEnumerableExtentions
    {
        public delegate bool ConditionalConverter<I, O>(I inData, out O outData);
        
        public static IEnumerable<O> WhereSelect<I, O>(this IEnumerable<I> enumerable, ConditionalConverter<I, O> conditionalConverter)
        {
            O newData = default;
            foreach (I data in enumerable)
            {
                if (conditionalConverter(data, out newData))
                {
                    yield return newData;
                }
            }
        }
        
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            T[] elements = source.ToArray();
            // Note i > 0 to avoid final pointless iteration
            for (int i = elements.Length - 1; i > 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                int swapIndex = (int)MyRandom.MyRandom.GetRandomDouble(0, i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
                // we don't actually perform the swap, we can forget about the
                // swapped element because we already returned it.
            }

            // there is one item remaining that was not returned - we return it now
            yield return elements[0];
        }

        public static IEnumerable<T> FromParams<T>(params T[] data)
        {
            return data.AsEnumerable();
        }

        public static IEnumerable<char> GetCharsFromString(this string data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                yield return data[i];
            }
        }

    }
}