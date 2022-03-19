using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractUI.Models;
using OwlCore.Extensions;

namespace OwlCore.Tests.AbstractUI.Models
{
    [TestClass]
    public class AbstractMultiChoiceTests
    {
        [TestMethod]
        public void IdPropMatchesCtor()
        {
            var item = new AbstractUIMetadata("data");
            var data = new AbstractMultiChoice(nameof(AbstractMultiChoiceTests), item, item.IntoList());
            Assert.AreEqual(nameof(AbstractMultiChoiceTests), data.Id);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingSelectedItemRaisesEvent()
        {
            var item = new AbstractUIMetadata("data");
            var item2 = new AbstractUIMetadata("data");
            var data = new AbstractMultiChoice(nameof(SettingSelectedItemRaisesEvent), item, new List<AbstractUIMetadata>() { item, item2 });

            var eventRaisedTask = OwlCore.Flow.EventAsTask<AbstractUIMetadata>(x => data.ItemSelected += x, x => data.ItemSelected -= x, TimeSpan.FromMilliseconds(100));

            var newValue = item2;

            data.SelectedItem = newValue;

            var res = await eventRaisedTask;
            Assert.AreEqual(newValue, res?.Result);
            Assert.AreEqual(newValue, data.SelectedItem);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingSelectedItemWithSameValueDoesNotRaiseChangedEvent()
        {
            var item = new AbstractUIMetadata("data");
            var item2 = new AbstractUIMetadata("data");
            var data = new AbstractMultiChoice(nameof(SettingSelectedItemWithSameValueDoesNotRaiseChangedEvent), item, new List<AbstractUIMetadata>() { item, item2 });

            var eventRaisedTask = OwlCore.Flow.EventAsTask<AbstractUIMetadata>(x => data.ItemSelected += x, x => data.ItemSelected -= x, TimeSpan.FromMilliseconds(100));

            data.SelectedItem = data.SelectedItem;

            var res = await eventRaisedTask;
            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }
    }
}
