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

            Assert.IsTrue(collectionChangedTaskCompletionSource.Task.IsCompleted, "CollectionChanged was not emitted.");

            void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Add, e.Action);
                collectionChangedTaskCompletionSource.SetResult();
            }
        }
    }
}
