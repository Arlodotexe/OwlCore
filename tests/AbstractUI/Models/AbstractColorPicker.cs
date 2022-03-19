using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.AbstractUI.Models;

namespace OwlCore.Tests.AbstractUI.Models
{
    [TestClass]
    public class AbstractColorPickerTests
    {
        [TestMethod]
        public void IdPropMatchesCtor()
        {
            var data = new AbstractColorPicker(nameof(AbstractColorPickerTests), "MyText");
            Assert.AreEqual(nameof(AbstractColorPickerTests), data.Id);
        }

        [TestMethod, Timeout(2000)]
        public async Task CallingPickColorRaisesEvent()
        {
            var data = new AbstractColorPicker(nameof(CallingPickColorRaisesEvent));

            var eventRaisedTask = OwlCore.Flow.EventAsTask<string>(x => data.ColorPicked += x, x => data.ColorPicked -= x, TimeSpan.FromMilliseconds(100));

            var expectedRes = "test";

            data.PickColor(expectedRes);

            var res = await eventRaisedTask;
            Assert.AreEqual(expectedRes, res?.Result);
        }
    }
}
