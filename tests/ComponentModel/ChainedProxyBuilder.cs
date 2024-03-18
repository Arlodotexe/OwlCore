using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.ComponentModel;

namespace OwlCore.Tests.ComponentModel
{
    [TestClass]
    public sealed class ChainedProxyBuilder
    {
        [DataRow(0, DisplayName = "Unmodified chain")]
        [DataRow(1), DataRow(2), DataRow(5), DataRow(10)]
        [TestMethod]
        public void ChainWithAllOverrides(int itemCount)
        {
            var finalTestClass = new FinalTestClass(nameof(FinalTestClass));

            var builder = new ChainedProxyBuilder<DelegatingChainedProxyBuilderTestBase, IChainedProxyBuilderTest>();

            for (int i = 0; i < itemCount; i++)
            {
                var cur = i;
                builder.Add(x => new CustomDelegatingChainedProxyTestBase(cur.ToString(), x));
            }

            var current = builder.Execute(finalTestClass);

            if (itemCount > 0)
                Assert.AreNotSame(current, finalTestClass);

            // Deconstruct and validate each item in the chain 
            for (int i = 0; i < itemCount; i++)
            {
                Assert.AreEqual(i.ToString(), current.Id);
                Assert.IsInstanceOfType(current, typeof(IDelegable<IChainedProxyBuilderTest>));

                current = ((IDelegable<IChainedProxyBuilderTest>)current).Inner;
            }

            Assert.AreEqual(current, finalTestClass);
        }

        [DataRow(0, DisplayName = "Unmodified chain")]
        [DataRow(1), DataRow(2), DataRow(5), DataRow(10)]
        [TestMethod]
        public void ChainWithNoOverrides(int itemCount)
        {
            var finalTestClass = new FinalTestClass(nameof(FinalTestClass));

            var builder = new ChainedProxyBuilder<DelegatingChainedProxyBuilderTestBase, IChainedProxyBuilderTest>();

            for (int i = 0; i < itemCount; i++)
                builder.Add(x => new DelegatingChainedProxyBuilderTestBase(x));

            var current = builder.Execute(finalTestClass);

            if (itemCount > 0)
                Assert.AreNotSame(current, finalTestClass);

            // Deconstruct and validate each item in the chain
            for (int i = 0; i < itemCount; i++)
            {
                Assert.AreEqual(current.Id, finalTestClass.Id, "Constructed chain with no overrides should always delegate to innermost implementation.");
                Assert.IsInstanceOfType(current, typeof(IDelegable<IChainedProxyBuilderTest>));

                current = ((IDelegable<IChainedProxyBuilderTest>)current).Inner;
            }

            Assert.AreEqual(current, finalTestClass);
        }

        public interface IChainedProxyBuilderTest
        {
            public string Id { get; set; }
        }

        public class CustomDelegatingChainedProxyTestBase : DelegatingChainedProxyBuilderTestBase
        {
            public CustomDelegatingChainedProxyTestBase(string id, IChainedProxyBuilderTest inner)
                : base(inner)
            {
                Id = id;
            }

            public override string Id { get; set; }
        }

        public class DelegatingChainedProxyBuilderTestBase : IChainedProxyBuilderTest, IDelegable<IChainedProxyBuilderTest>
        {
            public DelegatingChainedProxyBuilderTestBase(IChainedProxyBuilderTest inner)
            {
                Inner = inner;
            }

            public virtual string Id
            {
                get => Inner.Id;
                set => Inner.Id = value;
            }

            public IChainedProxyBuilderTest Inner { get; }
        }

        public class FinalTestClass : IChainedProxyBuilderTest
        {
            public FinalTestClass(string id)
            {
                Id = id;
            }

            public string Id { get; set; }
        }
    }
}
