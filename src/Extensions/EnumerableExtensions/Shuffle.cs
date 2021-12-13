using System;
using System.Collections.Generic;

namespace OwlCore.Extensions
{
    /// <summary>
    /// Enumerable-related extension methods.
    /// </summary>
    public static partial class EnumerableExtensions
    {
        private static readonly Random _rng = new Random();

        /// <summary>Shuffles the given array in place using a slightly modified fisher-yates algorithm, ensuring that no item remains in the same position.</summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">The array to shuffle.</param>
        /// <returns>
        /// A 1:1 "shuffle map" containing the original indexes for the shuffled items in the passed <paramref name="array"/>. Can be used to unshuffle the array in O(n) time.
        /// <para/>
        /// For example, if <c>shuffleMap[0] == 5</c>, then the item at <c>array[0]</c> was originally at index 5.
        /// </returns>
        public static int[] Shuffle<T>(this T[] array)
        {
            // Create and shuffle a list of indexes (shuffle map).
            // Can be used to restore original order.
            var shuffleMap = new int[array.Length];

            // Fisher-yates, Sattolo's algorithm
            int currentIndex = array.Length;
            while (currentIndex > 1)
            {
                currentIndex--;

                int newValueIndex = _rng.Next(currentIndex);

                if (shuffleMap[newValueIndex] == 0)
                    shuffleMap[newValueIndex] = newValueIndex;

                if (shuffleMap[currentIndex] == 0)
                    shuffleMap[currentIndex] = currentIndex;

                // Hold original value
                var currentItemValue = array[currentIndex];
                var currentShuffleMapIndex = shuffleMap[currentIndex];

                // Replace original value with random.
                array[currentIndex] = array[newValueIndex];
                shuffleMap[currentIndex] = shuffleMap[newValueIndex];

                // Assign at new index.
                array[newValueIndex] = currentItemValue;
                shuffleMap[newValueIndex] = currentShuffleMapIndex;
            }

            return shuffleMap;
        }

        /// <summary>Shuffles the given list in place using a slightly modified fisher-yates algorithm, ensuring that no item remains in the same position.</summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        /// <returns>
        /// A 1:1 "shuffle map" containing the original indexes for the shuffled items in the passed <paramref name="list"/>. Can be used to unshuffle the array in O(n) time.
        /// <para/>
        /// For example, if <c>shuffleMap[0] == 5</c>, then the item at <c>array[0]</c> was originally at index 5.
        /// </returns>
        public static int[] Shuffle<T>(this IList<T> list)
        {
            // Create and shuffle a list of indexes (shuffle map).
            // Can be used to restore original order.
            var shuffleMap = new int[list.Count];

            // Fisher-yates, Sattolo's algorithm
            int currentIndex = list.Count;
            while (currentIndex > 1)
            {
                currentIndex--;

                int newValueIndex = _rng.Next(currentIndex);

                if (shuffleMap[newValueIndex] == 0)
                    shuffleMap[newValueIndex] = newValueIndex;

                if (shuffleMap[currentIndex] == 0)
                    shuffleMap[currentIndex] = currentIndex;

                // Hold original value
                var currentItemValue = list[currentIndex];
                var currentShuffleMapIndex = shuffleMap[currentIndex];

                // Replace original value with random.
                list[currentIndex] = list[newValueIndex];
                shuffleMap[currentIndex] = shuffleMap[newValueIndex];

                // Assign at new index.
                list[newValueIndex] = currentItemValue;
                shuffleMap[newValueIndex] = currentShuffleMapIndex;
            }

            return shuffleMap;
        }
    }
}