using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            var task = OwlCore.Flow.EventAsTask(x => data.NonGenericEventHandler += x, x => data.NonGenericEventHandler -= x, CancellationToken.None);

            data.RaiseEvent();
            await task;
        }

        [TestMethod, Timeout(1000)]
        public async Task NonGenericEventAsTaskCompletes_TimeSpanOverload()
        {
            var data = new EventRaisingTestClass();

            var task = OwlCore.Flow.EventAsTask(x => data.NonGenericEventHandler += x, x => data.NonGenericEventHandler -= x, TimeSpan.FromSeconds(20));

            data.RaiseEvent();
            await task;
        }

        [TestMethod, Timeout(1000)]
        public async Task NonGenericEventAsTaskCancellation_CancellationTokenOverload()
        {
            var data = new EventRaisingTestClass();

            var tokenSource = new CancellationTokenSource();

            var task = OwlCore.Flow.EventAsTask(x => data.NonGenericEventHandler += x, x => data.NonGenericEventHandler -= x, tokenSource.Token);

            tokenSource.Cancel();

            await task;
        }

        [TestMethod, Timeout(1000)]
        public async Task NonGenericEventAsTaskCancellation_TimeSpanOverload()
        {
            var data = new EventRaisingTestClass();

            var task = OwlCore.Flow.EventAsTask(x => data.NonGenericEventHandler += x, x => data.NonGenericEventHandler -= x, TimeSpan.FromMilliseconds(100));

            await task;
        }

        [TestMethod, Timeout(1000)]
        public async Task GenericEventAsTaskCompletes_CancellationTokenOverload()
        {
            var data = new EventRaisingTestClass();

            var task = OwlCore.Flow.EventAsTask<string>(x => data.GenericEventHandler += x, x => data.GenericEventHandler -= x, CancellationToken.None);

            var raisedValue = "Test";

            data.RaiseEvent(raisedValue);
            var res = await task;
            Assert.AreEqual(res?.Result, raisedValue);
        }

        [TestMethod, Timeout(1000)]
        public async Task GenericEventAsTaskCompletes_TimeSpanOverload()
        {
            var data = new EventRaisingTestClass();

            var task = OwlCore.Flow.EventAsTask<string>(x => data.GenericEventHandler += x, x => data.GenericEventHandler -= x, TimeSpan.FromSeconds(20));

            var raisedValue = "Test";

            data.RaiseEvent(raisedValue);
            var res = await task;
            Assert.AreEqual(res?.Result, raisedValue);
        }

        [TestMethod, Timeout(1000)]
        public async Task GenericEventAsTaskCancellation_CancellationTokenOverload()
        {
            var data = new EventRaisingTestClass();

            var tokenSource = new CancellationTokenSource();

            var task = OwlCore.Flow.EventAsTask<string>(x => data.GenericEventHandler += x, x => data.GenericEventHandler -= x, tokenSource.Token);

            tokenSource.Cancel();

            await task;
        }

        [TestMethod, Timeout(1000)]
        public async Task GenericEventAsTaskCancellation_TimeSpanOverload()
        {
            var data = new EventRaisingTestClass();

            var task = OwlCore.Flow.EventAsTask<string>(x => data.GenericEventHandler += x, x => data.GenericEventHandler -= x, TimeSpan.FromMilliseconds(100));

            await task;
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
