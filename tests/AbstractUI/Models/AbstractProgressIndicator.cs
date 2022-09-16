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
    public class AbstractProgressIndicatorTests
    {
        [TestMethod]
        public void IdPropMatchesCtor()
        {
            var data = new AbstractProgressIndicator(nameof(AbstractProgressIndicatorTests), default);
            Assert.AreEqual(nameof(AbstractProgressIndicatorTests), data.Id);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingValueRaisesEvent()
        {
            var data = new AbstractProgressIndicator(nameof(SettingValueRaisesEvent), default);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<double>(x => data.ValueChanged += x, x => data.ValueChanged -= x, TimeSpan.FromMilliseconds(100));

            var newValue = 5;

            data.Value = newValue;

            var res = await eventRaisedTask;
            Assert.AreEqual(newValue, res?.Result);
            Assert.AreEqual(newValue, data.Value);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingValueWithSameValueDoesNotRaiseChangedEvent()
        {
            var data = new AbstractProgressIndicator(nameof(SettingValueWithSameValueDoesNotRaiseChangedEvent), default);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<double>(x => data.ValueChanged += x, x => data.ValueChanged -= x, TimeSpan.FromMilliseconds(100));

            data.Value = data.Value;

            var res = await eventRaisedTask;
            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingMaximumRaisesEvent()
        {
            var data = new AbstractProgressIndicator(nameof(SettingMaximumRaisesEvent), default);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<double>(x => data.MaximumChanged += x, x => data.MaximumChanged -= x, TimeSpan.FromMilliseconds(100));

            var newValue = 5;

            data.Maximum = newValue;

            var res = await eventRaisedTask;
            Assert.AreEqual(newValue, res?.Result);
            Assert.AreEqual(newValue, data.Maximum);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingMaximumWithSameValueDoesNotRaiseChangedEvent()
        {
            var data = new AbstractProgressIndicator(nameof(SettingMaximumWithSameValueDoesNotRaiseChangedEvent), default);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<double>(x => data.MaximumChanged += x, x => data.MaximumChanged -= x, TimeSpan.FromMilliseconds(100));

            data.Maximum = data.Maximum;

            var res = await eventRaisedTask;
            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingMinimumRaisesEvent()
        {
            var data = new AbstractProgressIndicator(nameof(SettingMinimumRaisesEvent), default);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<double>(x => data.MinimumChanged += x, x => data.MinimumChanged -= x, TimeSpan.FromMilliseconds(100));

            var newValue = 5;

            data.Minimum = newValue;

            var res = await eventRaisedTask;
            Assert.AreEqual(newValue, res?.Result);
            Assert.AreEqual(newValue, data.Minimum);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingMinimumWithSameValueDoesNotRaiseChangedEvent()
        {
            var data = new AbstractProgressIndicator(nameof(SettingMinimumWithSameValueDoesNotRaiseChangedEvent), default);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<double>(x => data.MinimumChanged += x, x => data.MinimumChanged -= x, TimeSpan.FromMilliseconds(100));

            data.Minimum = data.Minimum;

            var res = await eventRaisedTask;
            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingIsIndeterminateRaisesEvent()
        {
            var data = new AbstractProgressIndicator(nameof(SettingIsIndeterminateRaisesEvent), default);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<bool>(x => data.IsIndeterminateChanged += x, x => data.IsIndeterminateChanged -= x, TimeSpan.FromMilliseconds(100));

            var newValue = !data.IsIndeterminate;

            data.IsIndeterminate = newValue;

            var res = await eventRaisedTask;
            Assert.AreEqual(newValue, res?.Result);
            Assert.AreEqual(newValue, data.IsIndeterminate);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingIsIndeterminateWithSameValueDoesNotRaiseChangedEvent()
        {
            var data = new AbstractProgressIndicator(nameof(SettingIsIndeterminateWithSameValueDoesNotRaiseChangedEvent), default);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<bool>(x => data.IsIndeterminateChanged += x, x => data.IsIndeterminateChanged -= x, TimeSpan.FromMilliseconds(100));

            data.IsIndeterminate = data.IsIndeterminate;

            var res = await eventRaisedTask;
            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }
    }
}
