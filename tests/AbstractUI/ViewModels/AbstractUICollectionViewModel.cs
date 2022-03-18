using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractUI.Models;
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OwlCore.Tests.AbstractUI.ViewModels
{
    [TestClass]
    public class AbstractUICollectionViewModel
    {
        [TestInitialize]
        public void Setup()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [DataRow(false, DisplayName = "Manipulate ViewModel")]
        [DataRow(true, DisplayName = "Manipulate underlying model")]
        [TestMethod]
        public void Indexer(bool manipulateModel = false)
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new AbstractUICollection("id")
            {
                boolElement,
            };

            var viewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            Assert.AreEqual(boolElement.Id, viewModel[0].Id);
        }

        [TestMethod]
        public void Indexer_ThrowsOutOfRange()
        {
            var abstractUICollection = new AbstractUICollection("id");

            var viewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => viewModel[5]);
        }

        [TestMethod]
        public void Count()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new AbstractUICollection("id")
            {
                boolElement,
            };

            var viewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            Assert.AreEqual(1, viewModel.Count);
        }

        [TestMethod]
        public void Clear()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new AbstractUICollection("id")
            {
                boolElement
            };

            var viewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            // Collection must have an item for a clear test to be valid
            Assert.AreEqual(1, abstractUICollection.Count);
            Assert.AreEqual(1, viewModel.Count);

            abstractUICollection.Clear();

            Assert.AreEqual(0, viewModel.Count);
        }

        [TestMethod]
        public void Contains()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new AbstractUICollection("id");

            var viewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);
            abstractUICollection.Add(boolElement);

            Assert.AreEqual(true, viewModel.Any(x => x.Id == boolElement.Id));
        }

        [TestMethod]
        public void GetEnumerator_IEnumerable()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new AbstractUICollection("id")
            {
                boolElement,
            };

            var viewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            // Ensure no exceptions are thrown
            ((IEnumerable) viewModel).GetEnumerator();
        }

        [TestMethod]
        public void GetEnumerator_IEnumerable_Generic()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new AbstractUICollection("id")
            {
                boolElement,
            };

            var viewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            // Ensure no exceptions are thrown
            viewModel.GetEnumerator();
        }

        [TestMethod]
        public void AllModelsHaveViewModels()
        {
            var elements = new AbstractUICollection("test")
            {
                new AbstractButton("btn", "btn"),
                new AbstractBoolean("bool", "bool"),
                new AbstractColorPicker("clr"),
                new AbstractDataList("data", Enumerable.Empty<AbstractUIMetadata>().ToList()),
                new AbstractMultiChoice("mult", new AbstractUIMetadata("default"),
                    Enumerable.Empty<AbstractUIMetadata>()),
                new AbstractProgressIndicator("prog", false),
                new AbstractRichTextBlock("rich", "text"),
                new AbstractTextBox("txtbx"),
                new AbstractUICollection("inner"),
            };

            new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(elements);
        }

        [TestMethod, Timeout(2000)]
        public void CollectionChanged_Add()
        {
            var elementToAdd = new AbstractBoolean("bool", "label");

            var abstractUICollection = new AbstractUICollection("id");
            var abstractUICollectionViewModel =
                new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            var collectionChangedTaskCompletionSource = new TaskCompletionSource();
            abstractUICollectionViewModel.CollectionChanged += OnCollectionChanged;
            
            abstractUICollection.Add(elementToAdd);

            Assert.IsTrue(collectionChangedTaskCompletionSource.Task.IsCompleted, "CollectionChanged was not emitted.");

            void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Add, e.Action);
                collectionChangedTaskCompletionSource.SetResult();
            }
        }

        [TestMethod, Timeout(2000)]
        public void CollectionChanged_AddRemove()
        {
            var element = new AbstractBoolean("bool", "label");

            var abstractUICollection = new AbstractUICollection("id");
            var abstractUICollectionViewModel =
                new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            var collectionChangedTaskCompletionSource_Add = new TaskCompletionSource();
            abstractUICollectionViewModel.CollectionChanged += OnCollectionChanged_Add;

            abstractUICollection.Add(element);

            Assert.IsTrue(collectionChangedTaskCompletionSource_Add.Task.IsCompleted, "Item not added. CollectionChanged was not emitted.");

            abstractUICollectionViewModel.CollectionChanged -= OnCollectionChanged_Add;

            void OnCollectionChanged_Add(object? sender,
                System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Add, e.Action);
                collectionChangedTaskCompletionSource_Add.SetResult();
            }

            // Remove
            var collectionChangedTaskCompletionSource_Remove = new TaskCompletionSource();
            abstractUICollectionViewModel.CollectionChanged += OnCollectionChanged_Remove;

            abstractUICollection.Remove(element);

            Assert.IsTrue(collectionChangedTaskCompletionSource_Remove.Task.IsCompleted, "Item not Removed. CollectionChanged was not emitted.");

            abstractUICollectionViewModel.CollectionChanged -= OnCollectionChanged_Remove;

            void OnCollectionChanged_Remove(object? sender,
                System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(System.Collections.Specialized.NotifyCollectionChangedAction.Remove, e.Action);
                collectionChangedTaskCompletionSource_Remove.SetResult();
            }
        }
    }
}