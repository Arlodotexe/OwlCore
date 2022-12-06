using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OwlCoreFlow = OwlCore.Flow;

namespace OwlCore.Tests.Flow
{
    [TestClass]
    public class DebounceTaskCancellation
    {
        [TestMethod, Timeout(5000), TestCategory("PerformanceCheck")]
        public async Task Debouncer_TaskCancellationExceptionTest()
        {
            // Didn't use DataRows because they don't exccute the method in parallel.
            var flowDebouncePayloads = new List<FlowDebouncePayload>()
            {
                new FlowDebouncePayload() { Delay = 2, FlowId = "1" },
                new FlowDebouncePayload() { Delay = 1, FlowId = "1" },
            };

            await Parallel.ForEachAsync(flowDebouncePayloads, new ParallelOptions { MaxDegreeOfParallelism = 2 }, async (flowDebouncePayload, token) =>
            {
                if (flowDebouncePayload.Delay == 1)
                {
                    // Prevents both tasks to run exactly at the same time.
                    await Task.Delay(20);
                }

                var result = await OwlCoreFlow.Debounce(flowDebouncePayload.FlowId, TimeSpan.FromSeconds(flowDebouncePayload.Delay));

                if (flowDebouncePayload.Delay == 2)
                {
                    // The first payload should neatly return false without throwing TaskCancellationException. The test will fail if exception is thrown.
                    Assert.IsFalse(result);
                }
                else
                {
                    Assert.IsTrue(result);
                }
            });

        }


        struct FlowDebouncePayload
        {
            public int Delay { get; set; }
            public string FlowId { get; set; }
        }
    }
}
