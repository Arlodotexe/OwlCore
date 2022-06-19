using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OwlCore.Tests.Extensions
{
    [TestClass]
    public class AsyncExtensions
    {
        [DataRow(null)]
        [DataRow(null, null)]
        [DataRow(null, null, null)]
        [TestMethod, Timeout(1000)]
        public async Task InParallel_AggregateException_NoneThrowing(params Type?[] exceptionTypesToThrow)
        {
            Assert.IsTrue(exceptionTypesToThrow.Length > 0);

            var exceptionsToThrow = exceptionTypesToThrow.Select(x => x is not null ? (Exception?)Activator.CreateInstance(x) : null);
            var tasksToThrow = exceptionsToThrow.Select(x => Task.Run(() =>
            {
                if (x is not null)
                    throw x;
            }));

            await tasksToThrow.InParallel(x => x);
        }

        [DataRow(typeof(InvalidOperationException))]
        [DataRow(typeof(InvalidOperationException), typeof(NullReferenceException))]
        [TestMethod, Timeout(1000)]
        public async Task InParallel_AggregateException_AllThrowing(params Type[] exceptionTypesToThrow)
        {
            var exceptionsToThrow = exceptionTypesToThrow.Select(Activator.CreateInstance).Cast<Exception>();
            var tasksToThrow = exceptionsToThrow.Select(x => Task.Run(() => throw x));

            try
            {
                await tasksToThrow.InParallel(x => x);
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(exceptionTypesToThrow.Length, ex.InnerExceptions.Count);

                var exceptionsTypesToThrowList = exceptionTypesToThrow.ToList();

                for (int i = 0; i < ex.InnerExceptions.Count; i++)
                {
                    var exception = ex.InnerExceptions[i];

                    var relevantException = exceptionsTypesToThrowList.FirstOrDefault(x => x == exception.GetType());
                    Assert.IsNotNull(relevantException);

                    exceptionsTypesToThrowList.Remove(relevantException);
                }

                return;
            }

            Assert.Fail("Aggregate exception not thrown");
        }

        // AggregateException functionality was added to InParallel because
        // .AsParallel() from plinq would limit number of inner exceptions
        // to approximately the number of processor cores.
        // This hid exceptions we cared about in some specific scenarios.
        //
        // To make sure this doesn't happen again, test that InParallel is capable
        // of catching more thrown exceptions than any computer will have processing cores.
        [DataRow(10), DataRow(100), DataRow(1000), DataRow(10_000), DataRow(100_000)]
        [TestMethod, Timeout(15_000)]
        public Task InParallel_AggregateException_AllThrowing_ExtremeCount(int amount)
        {
            var exceptionTypes = Enumerable.Range(0, amount).Select(x => typeof(NullReferenceException));

            return InParallel_AggregateException_AllThrowing(exceptionTypes.ToArray());
        }

        [DataRow(typeof(InvalidOperationException), null)]
        [DataRow(typeof(InvalidOperationException), null, typeof(NullReferenceException))]
        [TestMethod, Timeout(1000)]
        public async Task InParallel_AggregateException_SomeThrowing(params Type?[] exceptionTypesToThrow)
        {
            Assert.IsTrue(exceptionTypesToThrow.Length > 0);

            var exceptionsToThrow = exceptionTypesToThrow.Select(x => x is not null ? (Exception?)Activator.CreateInstance(x) : null);
            var tasksToThrow = exceptionsToThrow.Select(x => Task.Run(() =>
            {
                if (x is not null)
                    throw x;
            }));


            try
            {
                await tasksToThrow.InParallel(x => x);
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(exceptionTypesToThrow.Where(x => x is not null).Count(), ex.InnerExceptions.Count);

                var exceptionsTypesToThrowList = exceptionTypesToThrow.ToList();

                for (int i = 0; i < ex.InnerExceptions.Count; i++)
                {
                    var exception = ex.InnerExceptions[i];

                    var relevantException = exceptionsTypesToThrowList.FirstOrDefault(x => x == exception.GetType());
                    Assert.IsNotNull(relevantException);

                    exceptionsTypesToThrowList.Remove(relevantException);
                }
            }
        }

        [DataRow(typeof(InvalidOperationException))]
        [DataRow(typeof(InvalidOperationException), typeof(NullReferenceException))]
        [TestMethod, Timeout(1000)]
        public async Task InParallel_ReturnValue_AggregateException_AllThrowing(params Type[] exceptionTypesToThrow)
        {
            var exceptionsToThrow = exceptionTypesToThrow.Select(Activator.CreateInstance).Cast<Exception>();
            var tasksToThrow = exceptionsToThrow.Select(x => Task.Run<bool>(async () => throw x));

            try
            {
                await tasksToThrow.InParallel(x => x);
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(exceptionTypesToThrow.Length, ex.InnerExceptions.Count);

                var exceptionsTypesToThrowList = exceptionTypesToThrow.ToList();

                for (int i = 0; i < ex.InnerExceptions.Count; i++)
                {
                    var exception = ex.InnerExceptions[i];

                    var relevantException = exceptionsTypesToThrowList.FirstOrDefault(x => x == exception.GetType());
                    Assert.IsNotNull(relevantException);

                    exceptionsTypesToThrowList.Remove(relevantException);
                }

                return;
            }

            Assert.Fail("Aggregate exception not thrown");
        }

        // AggregateException functionality was added to InParallel because
        // .AsParallel() from plinq would limit number of inner exceptions
        // to approximately the number of processor cores.
        // This hid exceptions we cared about in some specific scenarios.
        //
        // To make sure this doesn't happen again, test that InParallel is capable
        // of catching more thrown exceptions than any computer will have processing cores.
        [DataRow(10), DataRow(100), DataRow(1000), DataRow(10_000), DataRow(100_000)]
        [TestMethod, Timeout(15_000)]
        public Task InParallel_ReturnValue_AggregateException_AllThrowing_ExtremeCount(int amount)
        {
            var exceptionTypes = Enumerable.Range(0, amount).Select(x => typeof(NullReferenceException));

            return InParallel_AggregateException_AllThrowing(exceptionTypes.ToArray());
        }

        [DataRow(typeof(InvalidOperationException), null)]
        [DataRow(typeof(InvalidOperationException), null, typeof(NullReferenceException))]
        [TestMethod, Timeout(1000)]
        public async Task InParallel_ReturnValue_AggregateException_SomeThrowing(params Type?[] exceptionTypesToThrow)
        {
            Assert.IsTrue(exceptionTypesToThrow.Length > 0);

            var exceptionsToThrow = exceptionTypesToThrow.Select(x => x is not null ? (Exception?)Activator.CreateInstance(x) : null);
            var tasksToThrow = exceptionsToThrow.Select(x => Task.Run(() =>
            {
                if (x is not null)
                    throw x;

                return true;
            }));

            try
            {
                var res = await tasksToThrow.InParallel(x => x);
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(exceptionTypesToThrow.Where(x => x is not null).Count(), ex.InnerExceptions.Count);

                var exceptionsTypesToThrowList = exceptionTypesToThrow.ToList();

                for (int i = 0; i < ex.InnerExceptions.Count; i++)
                {
                    var exception = ex.InnerExceptions[i];

                    var relevantException = exceptionsTypesToThrowList.FirstOrDefault(x => x == exception.GetType());
                    Assert.IsNotNull(relevantException);

                    exceptionsTypesToThrowList.Remove(relevantException);
                }
            }
        }

        [DataRow(null)]
        [DataRow(null, null)]
        [DataRow(null, null, null)]
        [TestMethod, Timeout(1000)]
        public async Task InParallel_ReturnValue_AggregateException_NoneThrowing(params Type?[] exceptionTypesToThrow)
        {
            Assert.IsTrue(exceptionTypesToThrow.Length > 0);

            var exceptionsToThrow = exceptionTypesToThrow.Select(x => x is not null ? (Exception?)Activator.CreateInstance(x) : null);
            var tasksToThrow = exceptionsToThrow.Select(x => Task.Run(() =>
            {
                if (x is not null)
                    throw x;

                return true;
            }));

            var res = await tasksToThrow.InParallel(x => x);

            Assert.IsTrue(res.Length > 0);

            foreach (var item in res)
                Assert.IsTrue(item);
        }

        [TestMethod, Timeout(1000)]
        public async Task PostAsync()
        {
            var syncContext = new SynchronizationContext();

            var isSuccess = false;

            await syncContext.PostAsync(async () =>
            {
                await Task.Delay(100);

                isSuccess = true;
            });

            Assert.IsTrue(isSuccess);
        }

        [TestMethod, Timeout(1000)]
        public async Task PostAsync_ReturnVal()
        {
            var syncContext = new SynchronizationContext();

            var isSuccess = await syncContext.PostAsync(async () =>
            {
                await Task.Delay(100);

                return true;
            });

            Assert.IsTrue(isSuccess);
        }

        [TestMethod, Timeout(1000)]
        public async Task WhenCancelled_SimpleCancellation()
        {
            var cancellationTokenSource = new CancellationTokenSource(100);

            await cancellationTokenSource.Token.WhenCancelled(CancellationToken.None);

            Assert.IsTrue(cancellationTokenSource.IsCancellationRequested);
        }

        [TestMethod, Timeout(1000)]
        public async Task WhenCancelled_WithSelfCancellation()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var selfCancellationTokenSource = new CancellationTokenSource(100);

            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => cancellationTokenSource.Token.WhenCancelled(selfCancellationTokenSource.Token));

            Assert.IsFalse(cancellationTokenSource.IsCancellationRequested);
            Assert.IsTrue(selfCancellationTokenSource.IsCancellationRequested);
        }
    }
}
