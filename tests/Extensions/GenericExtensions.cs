using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OwlCore.Tests.Extensions
{
    [TestClass]
    public class GenericExtensions
    {
        [DataRow(10, 10), DataRow(5, 5), DataRow(3, 3), DataRow(100, 100)]
        [DataRow(10, 9), DataRow(5, 4), DataRow(3, 2), DataRow(100, 99)]
        [DataRow(10, -1), DataRow(5, -1), DataRow(3, -1), DataRow(100, -1)]
        [DataRow(10, 0), DataRow(5, 0), DataRow(3, 0), DataRow(100, 0)]
        [TestMethod, Timeout(5000)]
        public void CrawlBy(int totalDepth, int expectedInstanceAt = -1)
        {
            var expectedInstance = new CrawlByTestClass
            {
                Value = "Hello world!",
            };

            if (expectedInstanceAt == -1)
                expectedInstance = null;

            var instance = CreateTestClass(totalDepth, expectedInstanceAt, expectedInstance);

            // Start at -1 to make zero-indexed. (0 = child of root was checked, 1 = grandchild of root was checked, etc)
            // Note that the filter predicate is always run at least once.
            var depthReached = -1;
            var result = instance.CrawlBy(x => x.Inner, x => { ++depthReached; return ReferenceEquals(x, expectedInstance); });

            // -1 means the whole tree is crawled and nothing should be found.
            if (expectedInstanceAt == -1)
            {
                // totalDepth is not zero-indexed
                Assert.AreEqual(totalDepth - 1, depthReached, "Crawled depth does not match total depth.");
                Assert.IsNull(result, "Value should not be present.");
            }
            // 0 means only the child of root is checked.
            else if (expectedInstanceAt == 0)
            {
                Assert.AreEqual(0, depthReached, "Reached depth was not at child of root");
                Assert.IsNotNull(result, "Value should be present.");
            }
            // > 0 means arbitrary position in the tree is checked.
            else
            {
                // expectedInstanceAt is not zero-indexed
                Assert.AreEqual(expectedInstanceAt - 1, depthReached, "Did not crawl to the depth of the expected instance.");
                Assert.IsNotNull(result, "Value should be present.");
            }

            Assert.AreNotSame(result, instance, "Root instance was returned.");
            Assert.AreSame(result, expectedInstance);
            Assert.AreEqual(result?.Value, expectedInstance?.Value);

            CrawlByTestClass CreateTestClass(int totalDepth, int expectedAt, CrawlByTestClass? expectedInstance)
            {
                Assert.IsTrue(totalDepth > 0, "Total depth must be at least 1");
                Assert.IsTrue(totalDepth >= expectedAt, "Expected depth position was greater than total depth.");

                var root = new CrawlByTestClass();
                var current = root;

                if (expectedAt == 0)
                {
                    root.Inner = expectedInstance;
                    return root;
                }

                // totalDepth is not zero-indexed.
                // Handle all expected indexes >= 1.
                for (int i = 1; i <= totalDepth; i++)
                {
                    var newInstance = new CrawlByTestClass();

                    // expectedAt is not zero-indexed.
                    if (i == expectedAt && expectedAt > -1 && expectedInstance is not null)
                        newInstance = expectedInstance;

                    current.Inner = newInstance;
                    current = newInstance;
                }

                return root;
            }
        }

        [DataRow(10, 10), DataRow(5, 5), DataRow(3, 3), DataRow(100, 100)]
        [DataRow(10, 9), DataRow(5, 4), DataRow(3, 2), DataRow(100, 99)]
        [DataRow(10, -1), DataRow(5, -1), DataRow(3, -1), DataRow(100, -1)]
        [DataRow(10, 0), DataRow(5, 0), DataRow(3, 0), DataRow(100, 0)]

        [DataRow(10, 10, true), DataRow(5, 5, true), DataRow(3, 3, true), DataRow(100, 100, true)]
        [DataRow(10, 9, true), DataRow(5, 4, true), DataRow(3, 2, true), DataRow(100, 99, true)]
        [DataRow(10, -1, true), DataRow(5, -1, true), DataRow(3, -1, true), DataRow(100, -1, true)]
        [DataRow(10, 0, true), DataRow(5, 0, true), DataRow(3, 0, true), DataRow(100, 0, true)]
        [TestMethod, Timeout(10000)]
        public async Task CrawlByAsync(int totalDepth, int expectedInstanceAt = -1, bool withAsyncFilter = false)
        {
            var expectedInstance = new CrawlByAsyncTestClass
            {
                Value = "Hello world!",
            };

            if (expectedInstanceAt == -1)
                expectedInstance = null;

            var instance = CreateTestClass(totalDepth, expectedInstanceAt, expectedInstance);

            // Start at -1 to make zero-indexed. (0 = child of root was checked, 1 = grandchild of root was checked, etc)
            // Note that the filter predicate is always run at least once.
            var depthReached = -1;

            var result = withAsyncFilter ?
                await instance.CrawlByAsync(x => x.GetInnerAsync(), async x => { await Task.Yield(); ++depthReached; return ReferenceEquals(x, expectedInstance); }) :
                await instance.CrawlByAsync(x => x.GetInnerAsync(), x => { ++depthReached; return ReferenceEquals(x, expectedInstance); });

            // -1 means the whole tree is crawled and nothing should be found.
            if (expectedInstanceAt == -1)
            {
                // totalDepth is not zero-indexed
                Assert.AreEqual(totalDepth - 1, depthReached, "Crawled depth does not match total depth.");
                Assert.IsNull(result, "Value should not be present.");
            }
            // 0 means only the child of root is checked.
            else if (expectedInstanceAt == 0)
            {
                Assert.AreEqual(0, depthReached, "Reached depth was not at child of root");
                Assert.IsNotNull(result, "Value should be present.");
            }
            // > 0 means arbitrary position in the tree is checked.
            else
            {
                // expectedInstanceAt is not zero-indexed
                Assert.AreEqual(expectedInstanceAt - 1, depthReached, "Did not crawl to the depth of the expected instance.");
                Assert.IsNotNull(result, "Value should be present.");
            }

            Assert.AreNotSame(result, instance, "Root instance was returned.");
            Assert.AreSame(result, expectedInstance);
            Assert.AreEqual(result?.Value, expectedInstance?.Value);

            CrawlByAsyncTestClass CreateTestClass(int totalDepth, int expectedAt, CrawlByAsyncTestClass? expectedInstance)
            {
                Assert.IsTrue(totalDepth > 0, "Total depth must be at least 1");
                Assert.IsTrue(totalDepth >= expectedAt, "Expected depth position was greater than total depth.");

                var root = new CrawlByAsyncTestClass();
                var current = root;

                if (expectedAt == 0)
                {
                    root.Inner = expectedInstance;
                    return root;
                }

                // totalDepth is not zero-indexed.
                // Handle all expected indexes >= 1.
                for (int i = 1; i <= totalDepth; i++)
                {
                    var newInstance = new CrawlByAsyncTestClass();

                    // expectedAt is not zero-indexed.
                    if (i == expectedAt && expectedAt > -1 && expectedInstance is not null)
                        newInstance = expectedInstance;

                    current.Inner = newInstance;
                    current = newInstance;
                }

                return root;
            }
        }

        private class CrawlByTestClass
        {
            public CrawlByTestClass? Inner { get; set; }

            public string? Value { get; set; }
        }

        private class CrawlByAsyncTestClass
        {

            public async Task<CrawlByAsyncTestClass?> GetInnerAsync()
            {
                await Task.Delay(10);
                return Inner;
            }

            public CrawlByAsyncTestClass? Inner { private get; set; }

            public string? Value { get; set; }
        }
    }
}
