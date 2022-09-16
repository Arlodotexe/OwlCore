using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OwlCore.Tests.Flow
{
    [TestClass]
    public class EventAsTaskTests
    {
        [TestMethod, Timeout(1000)]
        public async Task NonGenericEventAsTaskCompletes_CancellationTokenOverload()
        {
            var data = new EventRaisingTestClass();
            var unsubbed = false;

            var task = OwlCore.Flow.EventAsTask(x => data.NonGenericEventHandler += x, x =>
            {
                data.NonGenericEventHandler -= x;
                unsubbed = true;
            }, CancellationToken.None);

            data.RaiseEvent();
            await task;
            Assert.IsTrue(unsubbed, "Event not unsubscribed.");
        }

        [TestMethod, Timeout(1000)]
        public async Task NonGenericEventAsTaskCompletes_TimeSpanOverload()
        {
            var data = new EventRaisingTestClass();
            var unsubbed = false;

            var task = OwlCore.Flow.EventAsTask(x => data.NonGenericEventHandler += x, x =>
            {
                data.NonGenericEventHandler -= x;
                unsubbed = true;
            }, TimeSpan.FromSeconds(20));

            data.RaiseEvent();
            await task;
            Assert.IsTrue(unsubbed, "Event not unsubscribed.");
        }

        [TestMethod, Timeout(1000)]
        public async Task NonGenericEventAsTaskCancellation_CancellationTokenOverload()
        {
            var data = new EventRaisingTestClass();

            var tokenSource = new CancellationTokenSource();
            var unsubbed = false;

            var task = OwlCore.Flow.EventAsTask(x => data.NonGenericEventHandler += x, x =>
            {
                data.NonGenericEventHandler -= x;
                unsubbed = true;
            }, tokenSource.Token);

            tokenSource.Cancel();

            await task;
            Assert.IsTrue(unsubbed, "Event not unsubscribed.");
        }

        [TestMethod, Timeout(1000)]
        public async Task NonGenericEventAsTaskCancellation_TimeSpanOverload()
        {
            var data = new EventRaisingTestClass();
            var unsubbed = false;

            var task = OwlCore.Flow.EventAsTask(x => data.NonGenericEventHandler += x, x =>
            {
                data.NonGenericEventHandler -= x;
                unsubbed = true;
            }, TimeSpan.FromMilliseconds(100));

            await task;
            Assert.IsTrue(unsubbed, "Event not unsubscribed.");
        }

        [TestMethod, Timeout(1000)]
        public async Task GenericEventAsTaskCompletes_CancellationTokenOverload()
        {
            var data = new EventRaisingTestClass();
            var unsubbed = false;

            var task = OwlCore.Flow.EventAsTask<string>(x => data.GenericEventHandler += x, x =>
            {
                data.GenericEventHandler -= x;
                unsubbed = true;
            }, CancellationToken.None);

            var raisedValue = "Test";

            data.RaiseEvent(raisedValue);
            var res = await task;
            Assert.AreEqual(res?.Result, raisedValue);
            Assert.IsTrue(unsubbed, "Event not unsubscribed.");
        }

        [TestMethod, Timeout(1000)]
        public async Task GenericEventAsTaskCompletes_TimeSpanOverload()
        {
            var data = new EventRaisingTestClass();
            var unsubbed = false;

            var task = OwlCore.Flow.EventAsTask<string>(x => data.GenericEventHandler += x, x =>
            {
                data.GenericEventHandler -= x;
                unsubbed = true;
            }, TimeSpan.FromSeconds(20));

            var raisedValue = "Test";

            data.RaiseEvent(raisedValue);
            var res = await task;
            Assert.AreEqual(res?.Result, raisedValue);
            Assert.IsTrue(unsubbed, "Event not unsubscribed.");
        }

        [TestMethod, Timeout(1000)]
        public async Task GenericEventAsTaskCancellation_CancellationTokenOverload()
        {
            var data = new EventRaisingTestClass();
            var unsubbed = false;

            var tokenSource = new CancellationTokenSource();

            var task = OwlCore.Flow.EventAsTask<string>(x => data.GenericEventHandler += x, x =>
            {
                data.GenericEventHandler -= x;
                unsubbed = true;
            }, tokenSource.Token);

            tokenSource.Cancel();

            await task;
            Assert.IsTrue(unsubbed, "Event not unsubscribed.");
        }

        [TestMethod, Timeout(1000)]
        public async Task GenericEventAsTaskCancellation_TimeSpanOverload()
        {
            var data = new EventRaisingTestClass();
            var unsubbed = false;

            var task = OwlCore.Flow.EventAsTask<string>(x => data.GenericEventHandler += x, x =>
            {
                data.GenericEventHandler -= x;
                unsubbed = true;
            }, TimeSpan.FromMilliseconds(100));

            await task;
            Assert.IsTrue(unsubbed, "Event not unsubscribed.");
        }

        public class EventRaisingTestClass
        {
            public void RaiseEvent() => NonGenericEventHandler?.Invoke(this, EventArgs.Empty);

            public void RaiseEvent(string value) => GenericEventHandler?.Invoke(this, value);

            public event EventHandler? NonGenericEventHandler;

            public event EventHandler<string>? GenericEventHandler;
        }
    }
}
