using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCore.Tests.AbstractUI.ViewModels
{
    [TestClass]
    public class AbstractUICollectionViewModel
    {
        [DataRow(false, DisplayName = "Add to ViewModel")]
        [DataRow(true, DisplayName = "Add to underlying model")]
        [TestMethod, Timeout(2000)]
        public void CollectionChanged_Add(bool manipulateModel = false)
        {
            var elementToAdd = new AbstractBoolean("bool", "label");

            var abstractUICollection = new AbstractUICollection("id");
            var abstractUICollectionViewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            var collectionChangedTaskCompletionSource = new TaskCompletionSource();
            abstractUICollectionViewModel.CollectionChanged += OnCollectionChanged;

            if (manipulateModel)
                abstractUICollection.Add(elementToAdd);
            else
                abstractUICollectionViewModel.Add(new OwlCore.AbstractUI.ViewModels.AbstractBooleanViewModel(elementToAdd));

            Assert.IsTrue(collectionChangedTaskCompletionSource.Task.IsCompleted, "CollectionChanged was not emitted.");

            void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Add, e.Action);
                collectionChangedTaskCompletionSource.SetResult();
            }
        }

        [DataRow(false, DisplayName = "Remove from ViewModel")]
        [DataRow(true, DisplayName = "Remove from underlying model")]
        [TestMethod, Timeout(2000)]
        public void CollectionChanged_AddRemove(bool manipulateModel = false)
        {
            var element = new AbstractBoolean("bool", "label");
            var elementViewModel = new OwlCore.AbstractUI.ViewModels.AbstractBooleanViewModel(element);

            var abstractUICollection = new AbstractUICollection("id");
            var abstractUICollectionViewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            var collectionChangedTaskCompletionSource_Add = new TaskCompletionSource();
            abstractUICollectionViewModel.CollectionChanged += OnCollectionChanged_Add;

            if (manipulateModel)
                abstractUICollection.Add(element);
            else
                abstractUICollectionViewModel.Add(elementViewModel);

            Assert.IsTrue(collectionChangedTaskCompletionSource_Add.Task.IsCompleted, "Item not added. CollectionChanged was not emitted.");

            abstractUICollectionViewModel.CollectionChanged -= OnCollectionChanged_Add;

            void OnCollectionChanged_Add(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Add, e.Action);
                collectionChangedTaskCompletionSource_Add.SetResult();
            }

            // Remove
            var collectionChangedTaskCompletionSource_Remove = new TaskCompletionSource();
            abstractUICollectionViewModel.CollectionChanged += OnCollectionChanged_Remove;

            if (manipulateModel)
                abstractUICollection.Remove(element);
            else
                abstractUICollectionViewModel.Remove(elementViewModel);

            Assert.IsTrue(collectionChangedTaskCompletionSource_Remove.Task.IsCompleted, "Item not Removed. CollectionChanged was not emitted.");

            abstractUICollectionViewModel.CollectionChanged -= OnCollectionChanged_Remove;

            void OnCollectionChanged_Remove(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Remove, e.Action);
                collectionChangedTaskCompletionSource_Remove.SetResult();
            }
        }
    }
}
