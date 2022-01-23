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
        public void CollectionChanged_Add(bool addToModel = false)
        {
            var elementToAdd = new AbstractBoolean("bool", "label");

            var abstractUICollection = new AbstractUICollection("id");
            var abstractUICollectionViewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            var collectionChangedTaskCompletionSource = new TaskCompletionSource();
            abstractUICollectionViewModel.CollectionChanged += OnCollectionChanged;

            if (addToModel)
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
    }
}
