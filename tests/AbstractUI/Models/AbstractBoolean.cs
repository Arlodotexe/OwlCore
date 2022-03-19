using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractUI.Models;

namespace OwlCore.Tests.AbstractUI.Models
{
    [TestClass]
    public class AbstractBooleanTests
    {
        [TestMethod]
        public void LabelPropMatchesCtor()
        {
            var data = new AbstractBoolean(nameof(AbstractBooleanTests), "MyLabel");
            Assert.AreEqual("MyLabel", data.Label);
        }

        [TestMethod]
        public void IdPropMatchesCtor()
        {
            var data = new AbstractBoolean(nameof(AbstractBooleanTests), "MyLabel");
            Assert.AreEqual(nameof(AbstractBooleanTests), data.Id);
        }

        [TestMethod]
        public void GetSetState()
        {
            var data = new AbstractBoolean(nameof(AbstractBooleanTests), "MyLabel");

            var initialValue = data.State;

            data.State = !initialValue;

            Assert.AreNotEqual(initialValue, data.State);
        }

        [TestMethod, Timeout(2000)]
        public async Task SetLabelRaisesChangedEvent()
        {
            var data = new AbstractBoolean(nameof(AbstractBooleanTests), "MyLabel");
            var newValue = nameof(SetLabelRaisesChangedEvent);

            Assert.AreNotEqual(newValue, data.Label, "New value and old value must be different for test to be valid.");

            var eventRaisedTask = OwlCore.Flow.EventAsTask<string>(x => data.LabelChanged += x, x => data.LabelChanged -= x, CancellationToken.None);

            data.Label = newValue;

            var res = await eventRaisedTask;

            Assert.AreEqual(newValue, res?.Result);
        }

        [TestMethod, Timeout(2000)]
        public async Task SetLabelWithSameValueDoesNotRaiseChangedEvent()
        {
            var data = new AbstractBoolean(nameof(AbstractBooleanTests), "MyLabel");
            var newValue = "MyLabel";

            Assert.AreEqual(newValue, data.Label, "New value and old value must be the same for test to be valid.");

            var eventRaisedTask = OwlCore.Flow.EventAsTask<string>(x => data.LabelChanged += x, x => data.LabelChanged -= x, TimeSpan.FromMilliseconds(100));

            data.Label = newValue;

            var res = await eventRaisedTask;

            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }

        [TestMethod, Timeout(2000)]
        public async Task SetStateRaisesChangedEvent()
        {
            var data = new AbstractBoolean(nameof(AbstractBooleanTests), "MyLabel");
            var newValue = true;

            Assert.AreNotEqual(newValue, data.State, "New value and old value must be different for test to be valid.");

            var eventRaisedTask = OwlCore.Flow.EventAsTask<bool>(x => data.StateChanged += x, x => data.StateChanged -= x, CancellationToken.None);

            data.State = newValue;

            var res = await eventRaisedTask;

            Assert.AreEqual(newValue, res?.Result);
        }

        [TestMethod, Timeout(2000)]
        public async Task SetStateWithSameValueDoesNotRaiseChangedEvent()
        {
            var data = new AbstractBoolean(nameof(AbstractBooleanTests), "MyLabel");
            var newValue = false;

            Assert.AreEqual(newValue, data.State, "New value and old value must be the same for test to be valid.");

            var eventRaisedTask = OwlCore.Flow.EventAsTask<bool>(x => data.StateChanged += x, x => data.StateChanged -= x, TimeSpan.FromMilliseconds(100));

            data.State = newValue;

            var res = await eventRaisedTask;

            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }
    }
}
