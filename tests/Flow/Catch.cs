using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OwlCore.Tests.Flow
{
    [TestClass]
    public class CatchTests
    {
        [TestMethod, Timeout(1000)]
        public void CatchSimple()
        {
            OwlCore.Flow.Catch(() => throw new NotImplementedException());
        }

        [TestMethod, Timeout(1000)]
        public void CatchWithResult()
        {
            var val = 5;

            var returnedValue = OwlCore.Flow.Catch(() => val);
            var defaultValue = OwlCore.Flow.Catch<int>(() => throw new NotImplementedException());

            // To properly test, these must not be the same.
            Assert.AreNotEqual(val, defaultValue);

            Assert.AreEqual(val, returnedValue);
            Assert.AreEqual(default, defaultValue);
        }

        [TestMethod, Timeout(1000)]
        public void CatchSpecificException()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                OwlCore.Flow.Catch<NotImplementedException>(() => { throw new InvalidOperationException(); });
            });

            OwlCore.Flow.Catch<NotImplementedException>(() => throw new NotImplementedException());
        }

        [TestMethod, Timeout(1000)]
        public void CatchSpecificExceptionWithResult()
        {
            var val = 5;

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                _ = OwlCore.Flow.Catch<int, NotImplementedException>(() => throw new InvalidOperationException());
            });

            var returnedValue = OwlCore.Flow.Catch<int, NotImplementedException>(() => val);
            var defaultValue = OwlCore.Flow.Catch<int, NotImplementedException>(() => throw new NotImplementedException());

            // To properly test, these must not be the same.
            Assert.AreNotEqual(val, defaultValue);

            Assert.AreEqual(val, returnedValue);
            Assert.AreEqual(default, defaultValue);
        }
    }
}
