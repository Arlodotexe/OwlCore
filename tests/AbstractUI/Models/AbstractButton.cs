using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractUI.Models;

namespace OwlCore.Tests.AbstractUI.Models
{
    [TestClass]
    public class AbstractButtonTests
    {
        [TestMethod]
        public void TextPropMatchesCtor()
        {
            var data = new AbstractButton(nameof(AbstractButtonTests), "MyText");
            Assert.AreEqual("MyText", data.Text);
        }

        [TestMethod]
        public void IdPropMatchesCtor()
        {
            var data = new AbstractButton(nameof(AbstractButtonTests), "MyText");
            Assert.AreEqual(nameof(AbstractButtonTests), data.Id);
        }

        [TestMethod]
        public void TypePropMatchesCtor()
        {
            var data = new AbstractButton(nameof(AbstractButtonTests), "MyText", type: AbstractButtonType.Cancel);
            Assert.AreEqual(AbstractButtonType.Cancel, data.Type);
        }

        [TestMethod]
        public void GetSetText()
        {
            var data = new AbstractButton(nameof(AbstractButtonTests), "MyText");
            var newValue = nameof(GetSetText);
            var initialValue = data.Text;

            Assert.AreNotEqual(newValue, initialValue, "New value and old value must be different for test to be valid.");

            data.Text = newValue;

            Assert.AreEqual(newValue, data.Text);
        }

        [TestMethod, Timeout(2000)]
        public async Task SetTextRaisesChangedEvent()
        {
            var data = new AbstractButton(nameof(AbstractButtonTests), "MyText");
            var newValue = nameof(SetTextRaisesChangedEvent);

            Assert.AreNotEqual(newValue, data.Text, "New value and old value must be different for test to be valid.");

            var eventRaisedTask = OwlCore.Flow.EventAsTask<string>(x => data.TextChanged += x, x => data.TextChanged -= x, CancellationToken.None);

            data.Text = newValue;

            var res = await eventRaisedTask;

            Assert.AreEqual(newValue, res?.Result);
        }

        [TestMethod, Timeout(2000)]
        public async Task SetTextWithSameValueDoesNotRaiseChangedEvent()
        {
            var data = new AbstractButton(nameof(AbstractButtonTests), "MyText");

            var eventRaisedTask = OwlCore.Flow.EventAsTask<string>(x => data.TextChanged += x, x => data.TextChanged -= x, TimeSpan.FromMilliseconds(100));

            data.Text = data.Text;

            var res = await eventRaisedTask;

            Assert.AreEqual(null, res, "Event was raised unexpectedly.");
        }

        [TestMethod, Timeout(2000)]
        public async Task CallingClickRaisesClickedEvent()
        {
            var data = new AbstractButton(nameof(AbstractButtonTests), "MyText");

            var eventRaisedTask = OwlCore.Flow.EventAsTask(x => data.Clicked += x, x => data.Clicked -= x, TimeSpan.FromMilliseconds(100));

            data.Click();

            await eventRaisedTask;
        }
    }
}
