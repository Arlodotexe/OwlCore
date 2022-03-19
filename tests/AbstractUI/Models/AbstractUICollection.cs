using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractUI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Frameworks;

namespace OwlCore.Tests.AbstractUI.Models
{
    [TestClass]
    public class AbstractUICollectionTests
    {
        [TestMethod]
        public void Indexer()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id")
            {
                boolElement,
            };

            Assert.AreEqual(boolElement.Id, abstractUICollection[0].Id);
        }

        [TestMethod]
        public void Indexer_ThrowsOutOfRange()
        {
            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id");

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => abstractUICollection[5]);
        }

        [TestMethod]
        public void Count()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id")
            {
                boolElement,
            };

            Assert.AreEqual(1, abstractUICollection.Count);
        }

        [TestMethod]
        public void IsReadOnly()
        {
            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id");

            // No abstract ui collection should be read only.
            Assert.AreEqual(false, abstractUICollection.IsReadOnly);
        }

        [TestMethod]
        public void Clear()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id")
            {
                boolElement,
            };

            // Collection must have an item for a clear test to be valid
            Assert.AreEqual(1, abstractUICollection.Count);
            
            abstractUICollection.Clear();
            
            Assert.AreEqual(0, abstractUICollection.Count);
        }

        [TestMethod]
        public void Contains()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id")
            {
                boolElement,
            };

            Assert.AreEqual(true, abstractUICollection.Contains(boolElement));
        }

        [TestMethod]
        public void CopyTo()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id")
            {
                boolElement,
            };

            var copied = new AbstractUIElement[abstractUICollection.Count];
            
            abstractUICollection.CopyTo(copied, 0);
            
            Assert.AreEqual(boolElement.Id, copied[0].Id);
        }

        [TestMethod]
        public void CopyTo_NonZeroIndex()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var boolElement2 = new AbstractBoolean("bool2", "label2");
            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id")
            {
                boolElement,
                boolElement2,
            };

            var copied = new AbstractUIElement[3];
            
            abstractUICollection.CopyTo(copied, 1);
            
            Assert.AreEqual(null, copied[0]);
            Assert.AreEqual(boolElement.Id, copied[1].Id);
            Assert.AreEqual(boolElement2.Id, copied[2].Id);
        }

        [TestMethod]
        public void GetEnumerator_IEnumerable()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id")
            {
                boolElement,
            };

            // Ensure no exceptions are thrown
            ((IEnumerable) abstractUICollection).GetEnumerator();
        }

        [TestMethod]
        public void GetEnumerator_IEnumerable_Generic()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id")
            {
                boolElement,
            };

            // Ensure no exceptions are thrown
            abstractUICollection.GetEnumerator();
        }

        [TestMethod, Timeout(2000)]
        public void CollectionChanged_Add()
        {
            var elementToAdd = new AbstractBoolean("bool", "label");

            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id");

            var collectionChangedTaskCompletionSource = new TaskCompletionSource();
            abstractUICollection.CollectionChanged += OnCollectionChanged;

            abstractUICollection.Add(elementToAdd);

            Assert.IsTrue(collectionChangedTaskCompletionSource.Task.IsCompleted,
                "Item not added (CollectionChanged not emitted)");

            void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Add, e.Action);
                collectionChangedTaskCompletionSource.SetResult();
            }
        }

        [TestMethod, Timeout(2000)]
        public void CollectionChanged_AddRemove()
        {
            var elementToAdd = new AbstractBoolean("bool", "label");

            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id");

            var collectionChangedTaskCompletionSource_Add = new TaskCompletionSource();
            abstractUICollection.CollectionChanged += OnCollectionChanged_Add;

            abstractUICollection.Add(elementToAdd);

            Assert.IsTrue(collectionChangedTaskCompletionSource_Add.Task.IsCompleted,
                "Item not added (CollectionChanged not emitted).");

            void OnCollectionChanged_Add(object? sender,
                System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Add, e.Action);
                collectionChangedTaskCompletionSource_Add.SetResult();
            }

            abstractUICollection.CollectionChanged -= OnCollectionChanged_Add;

            // Remove
            var collectionChangedTaskCompletionSource_Remove = new TaskCompletionSource();
            abstractUICollection.CollectionChanged += OnCollectionChanged_Remove;

            abstractUICollection.Remove(elementToAdd);

            Assert.IsTrue(collectionChangedTaskCompletionSource_Remove.Task.IsCompleted,
                "Item not removed (CollectionChanged not emitted).");

            abstractUICollection.CollectionChanged -= OnCollectionChanged_Remove;

            void OnCollectionChanged_Remove(object? sender,
                System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Remove, e.Action);
                collectionChangedTaskCompletionSource_Remove.SetResult();
            }
        }
    }
}