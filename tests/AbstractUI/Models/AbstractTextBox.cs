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
    public class AbstractTextBoxTests
    {
        [TestMethod]
        public void IdPropMatchesCtor()
        {
            var data = new AbstractTextBox(nameof(AbstractTextBoxTests), string.Empty);
            Assert.AreEqual(nameof(AbstractTextBoxTests), data.Id);
        }

        [TestMethod]
        public void ValuePropMatchesCtor()
        {
            // First constructor
            var initialValue = nameof(ValuePropMatchesCtor);
            var data = new AbstractTextBox(nameof(AbstractTextBoxTests), initialValue);

            Assert.AreEqual(initialValue, data.Value);

            // Second constructor
            var data2 = new AbstractTextBox(nameof(AbstractTextBoxTests), initialValue, string.Empty);

            Assert.AreEqual(initialValue, data2.Value);
        }

        [TestMethod]
        public void PlaceholderTextPropMatchesCtor()
        {
            var initialValue = nameof(ValuePropMatchesCtor);
            var data = new AbstractTextBox(nameof(AbstractTextBoxTests), string.Empty, initialValue);

            Assert.AreEqual(initialValue, data.PlaceholderText);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingValueRaisesEvent()
        {
            var data = new AbstractTextBox(nameof(AbstractTextBoxTests), string.Empty);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<string>(x => data.ValueChanged += x, x => data.ValueChanged -= x, TimeSpan.FromMilliseconds(100));

            var newValue = "NewVal";

            data.Value = newValue;

            var res = await eventRaisedTask;
            Assert.AreEqual(newValue, res?.Result);
            Assert.AreEqual(newValue, data.Value);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingValueWithSameValueDoesNotRaiseChangedEvent()
        {
            var data = new AbstractTextBox(nameof(AbstractTextBoxTests), string.Empty);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<string>(x => data.ValueChanged += x, x => data.ValueChanged -= x, TimeSpan.FromMilliseconds(100));

            data.Value = data.Value;

            var res = await eventRaisedTask;
            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingPlaceholderTextRaisesEvent()
        {
            var data = new AbstractTextBox(nameof(AbstractTextBoxTests), string.Empty);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<string>(x => data.PlaceholderTextChanged += x, x => data.PlaceholderTextChanged -= x, TimeSpan.FromMilliseconds(100));

            var newValue = "newValue";

            data.PlaceholderText = newValue;

            var res = await eventRaisedTask;
            Assert.AreEqual(newValue, res?.Result);
            Assert.AreEqual(newValue, data.PlaceholderText);
        }

        [TestMethod, Timeout(2000)]
        public async Task SettingPlaceholderTextWithSameValueDoesNotRaiseChangedEvent()
        {
            var data = new AbstractTextBox(nameof(AbstractTextBoxTests), string.Empty);

            var eventRaisedTask = OwlCore.Flow.EventAsTask<string>(x => data.PlaceholderTextChanged += x, x => data.PlaceholderTextChanged -= x, TimeSpan.FromMilliseconds(100));

            data.PlaceholderText = data.PlaceholderText;

            var res = await eventRaisedTask;
            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }
    }
}
