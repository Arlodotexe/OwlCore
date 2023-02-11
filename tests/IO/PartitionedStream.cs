using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.IO;

namespace OwlCore.Tests.IO
{
    [TestClass]
    public class PartitionedStreamTests
    {
        [TestMethod]
        public void EmptySourceStreamHasNoPartitions()
        {
            var sourceStream = new MemoryStream();
            var partitionedStream = new PartitionedStream(sourceStream);

            var partitions = partitionedStream.GetAllPartitions().ToArray();

            Assert.AreEqual(0, partitions.Length);
        }

        [TestMethod]
        public void EmptySource_CreatePartition()
        {
            var sourceStream = new MemoryStream();
            var partitionedStream = new PartitionedStream(sourceStream);

            var partition = partitionedStream.CreatePartition(0);
            Assert.IsTrue(partition.ContainerStartPosition > 0);
        }

        [TestMethod]
        public void EmptySource_CreateAndDeletePartition()
        {
            var sourceStream = new MemoryStream();
            var partitionedStream = new PartitionedStream(sourceStream);

            var partition = partitionedStream.CreatePartition(0);
            Assert.IsTrue(partition.ContainerStartPosition > 0);
        }

        [TestMethod]
        public void EmptySource_CreatePartitionAndWriteByte()
        {
            var sourceStream = new MemoryStream();
            var partitionedStream = new PartitionedStream(sourceStream);

            var partition = partitionedStream.CreatePartition(0);
            Assert.IsTrue(partition.ContainerStartPosition > 0);

            partition.WriteByte(5);
        }
    }
}
