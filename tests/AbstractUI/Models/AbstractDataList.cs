using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractUI.Models;
using OwlCore.Extensions;
using OwlCore.ComponentModel;

namespace OwlCore.Tests.AbstractUI.Models
{
    [TestClass]
    public class AbstractDataListTests
    {
        [TestMethod]
        public void IdPropMatchesCtor()
        {
            var item = new AbstractUIMetadata("Test")
            {
                Title = "abcdef"
            };

            var data = new AbstractDataList(nameof(IdPropMatchesCtor), item.IntoList());

            Assert.AreEqual(nameof(IdPropMatchesCtor), data.Id);
        }

        [TestMethod]
        public void ItemsPropMatchesCtor()
        {
            var item = new AbstractUIMetadata("Test")
            {
                Title = "abcdef"
            };

            var data = new AbstractDataList(nameof(CallingTapItemRaisesEvent), item.IntoList());

            Assert.AreSame(item, data.Items[0]);
        }

        [TestMethod, Timeout(2000)]
        public async Task CallingTapItemRaisesEvent()
        {
            var item = new AbstractUIMetadata("Test")
            {
                Title = "abcdef"
            };

            var data = new AbstractDataList(nameof(CallingTapItemRaisesEvent), item.IntoList());

            var eventRaisedTask = OwlCore.Flow.EventAsTask<AbstractUIMetadata>(x => data.ItemTapped += x, x => data.ItemTapped -= x, TimeSpan.FromMilliseconds(100));

            data.TapItem(item);

            var res = await eventRaisedTask;
            Assert.AreEqual(item, res?.Result);
        }

        [TestMethod, Timeout(2000)]
        public async Task CallingRequestNewItemRaisesEvent()
        {
            var item = new AbstractUIMetadata("Test")
            {
                Title = "abcdef"
            };

            var data = new AbstractDataList(nameof(CallingRequestNewItemRaisesEvent), item.IntoList());

            var eventRaisedTask = OwlCore.Flow.EventAsTask(x => data.AddRequested += x, x => data.AddRequested -= x, TimeSpan.FromMilliseconds(100));

            data.RequestNewItem();

            await eventRaisedTask;
        }

        [TestMethod, Timeout(2000)]
        public async Task CallingAddItemRaisesEvent()
        {
            var item = new AbstractUIMetadata("Test")
            {
                Title = "abcdef"
            };

            var data = new AbstractDataList(nameof(CallingRequestNewItemRaisesEvent), item.IntoList());

            var taskCompletionSource = new TaskCompletionSource();
            var eventRaisedTask = taskCompletionSource.Task;

            var newItem = new AbstractUIMetadata("Test1")
            {
                Title = "abcdefghi"
            };

            data.ItemsChanged += Data_ItemsChanged;
            data.AddItem(newItem);

            await eventRaisedTask;

            data.ItemsChanged -= Data_ItemsChanged;

            void Data_ItemsChanged(object sender, IReadOnlyList<CollectionChangedItem<AbstractUIMetadata>> addedItems, IReadOnlyList<CollectionChangedItem<AbstractUIMetadata>> removedItems)
            {
                Assert.AreEqual(1, addedItems.Count);
                Assert.AreEqual(0, removedItems.Count);

                Assert.AreSame(newItem, addedItems[0].Data);
                Assert.AreEqual(data.Items.Count - 1, addedItems[0].Index);

                taskCompletionSource.SetResult();
            }
        }

        [TestMethod, Timeout(2000)]
        public async Task CallingInsertItemRaisesEvent()
        {
            var item = new AbstractUIMetadata("Test")
            {
                Title = "abcdef"
            };

            var data = new AbstractDataList(nameof(CallingRequestNewItemRaisesEvent), item.IntoList());

            var taskCompletionSource = new TaskCompletionSource();
            var eventRaisedTask = taskCompletionSource.Task;

            var newItem = new AbstractUIMetadata("Test1")
            {
                Title = "abcdefghi"
            };

            data.ItemsChanged += Data_ItemsChanged;
            data.InsertItem(newItem, 0);

            await eventRaisedTask;

            data.ItemsChanged -= Data_ItemsChanged;

            void Data_ItemsChanged(object sender, IReadOnlyList<CollectionChangedItem<AbstractUIMetadata>> addedItems, IReadOnlyList<CollectionChangedItem<AbstractUIMetadata>> removedItems)
            {
                Assert.AreEqual(1, addedItems.Count);
                Assert.AreEqual(0, removedItems.Count);

                Assert.AreSame(newItem, addedItems[0].Data);
                Assert.AreEqual(0, addedItems[0].Index);
                taskCompletionSource.SetResult();
            }
        }

        [TestMethod, Timeout(2000)]
        public async Task CallingRemoveItemRaisesEvent()
        {
            var item = new AbstractUIMetadata("Test")
            {
                Title = "abcdef"
            };

            var data = new AbstractDataList(nameof(CallingRequestNewItemRaisesEvent), item.IntoList());

            var taskCompletionSource = new TaskCompletionSource();
            var eventRaisedTask = taskCompletionSource.Task;

            data.ItemsChanged += Data_ItemsChanged;

            data.RemoveItem(item);

            await eventRaisedTask;

            data.ItemsChanged -= Data_ItemsChanged;

            void Data_ItemsChanged(object sender, IReadOnlyList<CollectionChangedItem<AbstractUIMetadata>> addedItems, IReadOnlyList<CollectionChangedItem<AbstractUIMetadata>> removedItems)
            {
                Assert.AreEqual(0, addedItems.Count);
                Assert.AreEqual(1, removedItems.Count);

                Assert.AreSame(item, removedItems[0].Data);
                Assert.AreEqual(0, removedItems[0].Index);
                taskCompletionSource.SetResult();
            }
        }

        [TestMethod, Timeout(2000)]
        public async Task CallingRemoveItemAtRaisesEvent()
        {
            var item = new AbstractUIMetadata("Test")
            {
                Title = "abcdef"
            };

            var data = new AbstractDataList(nameof(CallingRequestNewItemRaisesEvent), item.IntoList());

            var taskCompletionSource = new TaskCompletionSource();
            var eventRaisedTask = taskCompletionSource.Task;

            data.ItemsChanged += Data_ItemsChanged;

            data.RemoveItemAt(0);

            await eventRaisedTask;

            data.ItemsChanged -= Data_ItemsChanged;

            void Data_ItemsChanged(object sender, IReadOnlyList<CollectionChangedItem<AbstractUIMetadata>> addedItems, IReadOnlyList<CollectionChangedItem<AbstractUIMetadata>> removedItems)
            {
                Assert.AreEqual(0, addedItems.Count);
                Assert.AreEqual(1, removedItems.Count);

                Assert.AreSame(item, removedItems[0].Data);
                Assert.AreEqual(0, removedItems[0].Index);
                taskCompletionSource.SetResult();
            }
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingIsUserEditingEnabledRaisesEvent()
        {
            var item = new AbstractUIMetadata("Test")
            {
                Title = "abcdef"
            };

            var data = new AbstractDataList(nameof(CallingTapItemRaisesEvent), item.IntoList());

            var eventRaisedTask = OwlCore.Flow.EventAsTask<bool>(x => data.IsUserEditingEnabledChanged += x, x => data.IsUserEditingEnabledChanged -= x, TimeSpan.FromMilliseconds(100));

            var newValue = !data.IsUserEditingEnabled;

            Assert.AreNotEqual(newValue, data.IsUserEditingEnabled);

            data.IsUserEditingEnabled = newValue;

            var res = await eventRaisedTask;
            Assert.AreEqual(newValue, res?.Result);
            Assert.AreEqual(newValue, data.IsUserEditingEnabled);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingIsUserEditingEnabledWithSameValueDoesNotRaiseChangedEvent()
        {
            var item = new AbstractUIMetadata("Test")
            {
                Title = "abcdef"
            };

            var data = new AbstractDataList(nameof(CallingTapItemRaisesEvent), item.IntoList());

            var eventRaisedTask = OwlCore.Flow.EventAsTask<bool>(x => data.IsUserEditingEnabledChanged += x, x => data.IsUserEditingEnabledChanged -= x, TimeSpan.FromMilliseconds(100));

            data.IsUserEditingEnabled = data.IsUserEditingEnabled;

            var res = await eventRaisedTask;

            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }
    }
}
