using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCore.Tests.AbstractUI.Models
{
    [TestClass]
    public class AbstractUICollection
    {
        [TestMethod, Timeout(2000)]
        public void CollectionChanged_Add()
        {
            var elementToAdd = new AbstractBoolean("bool", "label");

            var abstractUICollection = new OwlCore.AbstractUI.Models.AbstractUICollection("id");

            var collectionChangedTaskCompletionSource = new TaskCompletionSource();
            abstractUICollection.CollectionChanged += OnCollectionChanged;

            abstractUICollection.Add(elementToAdd);

            Assert.IsTrue(collectionChangedTaskCompletionSource.Task.IsCompleted, "Item not added (CollectionChanged not emitted)");

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

            Assert.IsTrue(collectionChangedTaskCompletionSource_Add.Task.IsCompleted, "Item not added (CollectionChanged not emitted).");

            void OnCollectionChanged_Add(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Add, e.Action);
                collectionChangedTaskCompletionSource_Add.SetResult();
            }

            abstractUICollection.CollectionChanged -= OnCollectionChanged_Add;

            // Remove
            var collectionChangedTaskCompletionSource_Remove = new TaskCompletionSource();
            abstractUICollection.CollectionChanged += OnCollectionChanged_Remove;

            abstractUICollection.Remove(elementToAdd);

            Assert.IsTrue(collectionChangedTaskCompletionSource_Remove.Task.IsCompleted, "Item not removed (CollectionChanged not emitted).");

            abstractUICollection.CollectionChanged -= OnCollectionChanged_Remove;

            void OnCollectionChanged_Remove(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Remove, e.Action);
                collectionChangedTaskCompletionSource_Remove.SetResult();
            }
        }
    }
}
