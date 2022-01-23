using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Events;
using OwlCore.Extensions;
using System;
using System.Linq;

namespace OwlCore.Tests.Extensions
{
    [TestClass]
    public class EnumerableExtensions
    {
        [TestMethod, Timeout(5000)]
        [DataRow(2)]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(10000)]
        [DataRow(100000)]
        [DataRow(1000000)]
        [DataRow(2000000)]
        public void Shuffle_Array(int max)
        {
            var originalItems = Enumerable.Range(0, max).ToArray();
            var shuffledItems = originalItems.ToArray();

            shuffledItems.Shuffle();

            CollectionAssert.AreNotEqual(originalItems, shuffledItems);
        }

        [TestMethod, Timeout(5000)]
        [DataRow(2)]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(10000)]
        [DataRow(100000)]
        [DataRow(1000000)]
        [DataRow(2000000)]
        public void Shuffle_List(int max)
        {
            var originalItems = Enumerable.Range(0, max).ToArray();
            var shuffledItems = originalItems.ToList();

            shuffledItems.Shuffle();

            CollectionAssert.AreNotEqual(originalItems, shuffledItems);
        }

        [TestMethod, Timeout(5000)]
        [DataRow(2)]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(10000)]
        [DataRow(100000)]
        [DataRow(1000000)]
        [DataRow(2000000)]
        public void Shuffle_Array_CheckShuffleMap(int max)
        {
            var originalItems = Enumerable.Range(0, max).ToArray();
            var shuffledItems = originalItems.ToArray();

            var shuffleMap = shuffledItems.Shuffle();
            CollectionAssert.AllItemsAreUnique(shuffleMap);
            CollectionAssert.AreNotEqual(originalItems, shuffledItems);

            CollectionAssert.AreNotEqual(originalItems, shuffledItems);
            CollectionAssert.AreEqual(shuffleMap, shuffledItems);
        }

        [TestMethod, Timeout(5000)]
        [DataRow(2)]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(10000)]
        [DataRow(100000)]
        [DataRow(1000000)]
        [DataRow(2000000)]
        public void Shuffle_List_CheckShuffleMap(int max)
        {
            var originalItems = Enumerable.Range(0, max).ToArray();
            var shuffledItems = originalItems.ToList();

            var shuffleMap = shuffledItems.Shuffle();
            CollectionAssert.AllItemsAreUnique(shuffleMap);
            CollectionAssert.AreNotEqual(originalItems, shuffledItems);

            CollectionAssert.AreNotEqual(originalItems, shuffledItems);
            CollectionAssert.AreEqual(shuffleMap, shuffledItems);
        }

        [TestMethod, Timeout(5000)]
        [DataRow(2)]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(10000)]
        [DataRow(100000)]
        [DataRow(1000000)]
        [DataRow(2000000)]
        public void ShuffleUnshuffle_Array(int max)
        {
            var originalItems = Enumerable.Range(0, max).ToArray();
            var shuffledItems = originalItems.ToArray();

            var shuffleMap = shuffledItems.Shuffle();
            CollectionAssert.AllItemsAreUnique(shuffleMap);
            CollectionAssert.AreNotEqual(originalItems, shuffledItems);

            shuffledItems.Unshuffle(shuffleMap);

            CollectionAssert.AreEqual(originalItems, shuffledItems);
        }

        [TestMethod, Timeout(5000)]
        [DataRow(2)]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(10000)]
        [DataRow(100000)]
        [DataRow(1000000)]
        [DataRow(2000000)]
        public void ShuffleUnshuffle_List(int max)
        {
            var originalItems = Enumerable.Range(0, max).ToList();
            var shuffledItems = originalItems.ToList();

            var shuffleMap = shuffledItems.Shuffle();
            CollectionAssert.AllItemsAreUnique(shuffleMap);
            CollectionAssert.AreNotEqual(originalItems, shuffledItems);

            shuffledItems.Unshuffle(shuffleMap);

            CollectionAssert.AreEqual(originalItems, shuffledItems);
        }

        [TestMethod, Timeout(5000)]
        [DataRow(2)]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(10000)]
        [DataRow(100000)]
        [DataRow(1000000)]
        [DataRow(2000000)]
        public void ShuffleUnshuffle_List_WithChanges(int max)
        {
            var originalItems = Enumerable.Range(0, max).ToList();
            var shuffledItems = originalItems.ToList();

            var shuffleMap = shuffledItems.Shuffle();
            CollectionAssert.AllItemsAreUnique(shuffleMap);
            CollectionAssert.AreNotEqual(originalItems, shuffledItems);

            int shuffleMapSwapVar = shuffleMap[0];
            shuffleMap[0] = shuffleMap[1];
            shuffleMap[1] = shuffleMapSwapVar;

            int shuffledItemsSwapVar = shuffledItems[0];
            shuffledItems[0] = shuffledItems[1];
            shuffledItems[1] = shuffledItemsSwapVar;

            shuffledItems.Unshuffle(shuffleMap);

            CollectionAssert.AreEqual(originalItems, shuffledItems);
        }

        [TestMethod, Timeout(5000)]
        [DataRow(2)]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(10000)]
        [DataRow(100000)]
        [DataRow(1000000)]
        [DataRow(2000000)]
        public void ShuffleUnshuffle_Array_WithChanges(int max)
        {
            var originalItems = Enumerable.Range(0, max).ToArray();
            var shuffledItems = originalItems.ToList();

            var shuffleMap = shuffledItems.Shuffle();
            CollectionAssert.AllItemsAreUnique(shuffleMap);
            CollectionAssert.AreNotEqual(originalItems, shuffledItems);

            int shuffleMapSwapVar = shuffleMap[0];
            shuffleMap[0] = shuffleMap[1];
            shuffleMap[1] = shuffleMapSwapVar;

            int shuffledItemsSwapVar = shuffledItems[0];
            shuffledItems[0] = shuffledItems[1];
            shuffledItems[1] = shuffledItemsSwapVar;

            shuffledItems.Unshuffle(shuffleMap);

            CollectionAssert.AreEqual(originalItems, shuffledItems);
        }

        [DataRow(1, 0, 1)]
        [DataRow(7, 5, 6, 7)]
        [DataRow(9999, 100, 500, 1000, 9999)]
        [TestMethod, Timeout(5000)]
        public void Pop(int expectedReturn, params int[] items)
        {
            var list = items.ToList();
            var lastItem = list.Pop();

            Assert.AreEqual(lastItem, expectedReturn);
            Assert.AreEqual(lastItem, items.Last());
        }

        [DataRow(-1, 1, 0)]
        [DataRow(0, 1, 0, 1)]
        [DataRow(1, 10, 5, 6, 7)]
        [DataRow(3, 0, 100, 500, 1000, 9999)]
        [DataRow(10, 0, 100, 500, 1000, 9999)]
        [TestMethod, Timeout(5000)]
        public void ReplaceOrAdd(int indexToReplace, int newValue, params int[] items)
        {
            var list = items.ToList();

            if (indexToReplace > items.Length || indexToReplace < 0)
            {
                Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
                {
                    list.ReplaceOrAdd(indexToReplace, newValue);
                });

                return;
            }

            list.ReplaceOrAdd(indexToReplace, newValue);

            Assert.IsTrue(list[indexToReplace] == newValue);
        }

        [TestMethod, Timeout(5000)]
        [DataRow(100, new int[] { 0 }, new int[] { })]
        [DataRow(100, new int[] { 0, 50 }, new int[] { })]
        [DataRow(100, new int[] { }, new int[] { 0, 50 })]
        [DataRow(100, new int[] { 0 }, new int[] { 1 })]
        [DataRow(100, new int[] { 0, 50 }, new int[] { 0, 50 })]
        [DataRow(100, new int[] { 99 }, new int[] { 0, 99 })]
        [DataRow(100, new int[] { 99 }, new int[] { 0, 99 })]
        [DataRow(100, new int[] { 0, 99 }, new int[] { 0, })]
        public void ChangeCollection(int numberOfItems, int[] addedIndices, int[] removedIndices)
        {
            var addedItems = addedIndices.Select(x => new CollectionChangedItem<int>(x, x)).ToList();
            var removedItems = removedIndices.Select(x => new CollectionChangedItem<int>(x, x)).ToList();

            var collection = Enumerable.Range(0, numberOfItems).ToList();

            collection.ChangeCollection(addedItems, removedItems);

            Assert.AreEqual(numberOfItems - removedIndices.Length + addedIndices.Length, collection.Count);

            // Indexes are treated as though you're always modifying the original collection.
            // This means, for example, if you add 2 items, index 10 and 50,
            // the first item is inserted at 10 and the second item shifts to 51.
            // Tests are properly compensated for that.

            for (int i = 0; i < addedIndices.Length; i++)
            {
                int addedIndex = addedIndices[i];

                // Adjust for preceding added items
                var finalIndex = addedIndex + i;

                // Adjust for preceding removed items
                var precedingRemovedItemCount = Array.FindIndex(removedIndices, 0, removedIndices.Length, x => x >= addedIndex);
                if (precedingRemovedItemCount >= 0)
                {
                    finalIndex -= precedingRemovedItemCount;
                }

                Assert.AreEqual(addedIndex, collection[finalIndex]);
            }

            for (int i = 0; i < removedIndices.Length; i++)
            {
                int removedIndex = removedIndices[i];

                // If removed and re-added with same value.
                if (addedIndices.Contains(removedIndex))
                {
                    // Adjust for preceding added items
                    var finalIndex = removedIndex + Array.FindIndex(addedIndices, x => x == removedIndex);

                    // Adjust for preceding removed items
                    var precedingRemovedItemCount = Array.FindIndex(removedIndices, x => x >= removedIndex);
                    if (precedingRemovedItemCount >= 0)
                    {
                        finalIndex -= precedingRemovedItemCount;
                    }

                    Assert.AreEqual(removedIndex, collection[finalIndex]);
                }
                else
                    Assert.AreNotEqual(removedIndex, collection[removedIndex]);
            }
        }
    }
}
