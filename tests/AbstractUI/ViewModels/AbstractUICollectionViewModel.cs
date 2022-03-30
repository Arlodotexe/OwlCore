using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractUI.Models;

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

            Assert.AreEqual(boolElement.Id, viewModel.Items[0].Id);
        }

        [TestMethod]
        public void Indexer_ThrowsOutOfRange()
        {
            var abstractUICollection = new AbstractUICollection("id");

            var viewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => viewModel.Items[5]);
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

            Assert.AreEqual(1, viewModel.Items.Count);
        }

        [TestMethod, Timeout(1000)]
        public async Task Clear()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new AbstractUICollection("id")
            {
                boolElement
            };

            var viewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            // Collection must have an item for a clear test to be valid
            Assert.AreEqual(1, abstractUICollection.Count);
            Assert.AreEqual(1, viewModel.Items.Count);

            abstractUICollection.Clear();

            var taskCompletion = new TaskCompletionSource();
            ((INotifyCollectionChanged)viewModel.Items).CollectionChanged += ViewModelOnCollectionChanged;
            await taskCompletion.Task;

            Assert.AreEqual(0, viewModel.Items.Count);

            ((INotifyCollectionChanged)viewModel.Items).CollectionChanged -= ViewModelOnCollectionChanged;

            void ViewModelOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => taskCompletion.SetResult();
        }

        [TestMethod, Timeout(1000)]
        public async Task Contains()
        {
            var boolElement = new AbstractBoolean("bool", "label");
            var abstractUICollection = new AbstractUICollection("id");

            var viewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);
            abstractUICollection.Add(boolElement);

            var taskCompletion = new TaskCompletionSource();
            ((INotifyCollectionChanged)viewModel.Items).CollectionChanged += ViewModelOnCollectionChanged;
            await taskCompletion.Task;

            Assert.AreEqual(true, viewModel.Items.Any(x => x.Id == boolElement.Id));

            ((INotifyCollectionChanged)viewModel.Items).CollectionChanged -= ViewModelOnCollectionChanged;

            void ViewModelOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => taskCompletion.SetResult();
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
        public async Task CollectionChanged_Add()
        {
            var elementToAdd = new AbstractBoolean("bool", "label");

            var abstractUICollection = new AbstractUICollection("id");
            var abstractUICollectionViewModel = new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            var collectionChangedTaskCompletionSource = new TaskCompletionSource();
            ((INotifyCollectionChanged)abstractUICollectionViewModel.Items).CollectionChanged += OnCollectionChanged;
            
            abstractUICollection.Add(elementToAdd);

            await collectionChangedTaskCompletionSource.Task;

            void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                collectionChangedTaskCompletionSource.SetResult();
            }
        }

        [TestMethod, Timeout(2000)]
        public async Task CollectionChanged_AddRemove()
        {
            var element = new AbstractBoolean("bool", "label");

            var abstractUICollection = new AbstractUICollection("id");
            var abstractUICollectionViewModel =
                new OwlCore.AbstractUI.ViewModels.AbstractUICollectionViewModel(abstractUICollection);

            var collectionChangedTaskCompletionSource_Add = new TaskCompletionSource();
            ((INotifyCollectionChanged)abstractUICollectionViewModel.Items).CollectionChanged += OnCollectionChanged_Add;

            abstractUICollection.Add(element);

            await collectionChangedTaskCompletionSource_Add.Task;

            ((INotifyCollectionChanged)abstractUICollectionViewModel.Items).CollectionChanged -= OnCollectionChanged_Add;

            void OnCollectionChanged_Add(object? sender,
                NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                collectionChangedTaskCompletionSource_Add.SetResult();
            }

            // Remove
            var collectionChangedTaskCompletionSource_Remove = new TaskCompletionSource();
            ((INotifyCollectionChanged)abstractUICollectionViewModel.Items).CollectionChanged += OnCollectionChanged_Remove;

            abstractUICollection.Remove(element);

            await collectionChangedTaskCompletionSource_Remove.Task;

            ((INotifyCollectionChanged)abstractUICollectionViewModel.Items).CollectionChanged -= OnCollectionChanged_Remove;

            void OnCollectionChanged_Remove(object? sender,
                NotifyCollectionChangedEventArgs e)
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                collectionChangedTaskCompletionSource_Remove.SetResult();
            }
        }
    }
}