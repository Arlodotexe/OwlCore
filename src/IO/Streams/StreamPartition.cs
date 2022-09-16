using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Extensions;
using CommunityToolkit.Diagnostics;

namespace OwlCore.IO.Streams
{
    /// <summary>
    /// A thread-safe implementation of <see cref="Stream"/> that interacts with a single partition in a <see cref="PartitionedStream"/>.
    /// </summary>
    public class StreamPartition : Stream
    {
        internal const long NextFragmentPointerByteCount = 8;
        internal const long FragmentLengthByteCount = 8;
        internal const long FragmentHeaderLength = NextFragmentPointerByteCount + FragmentLengthByteCount;

        private readonly PartitionedStream _container;

        /// <summary>
        /// Creates an instance of <see cref="StreamPartition"/>.
        /// </summary>
        /// <param name="id"> A unique identifier for this partition.</param>
        /// <param name="containerStartPosition">The position of the start of this partition within the original source stream.</param>
        /// <param name="container">The <see cref="PartitionedStream"/> that this partition belongs to.</param>
        internal StreamPartition(byte id, long containerStartPosition, PartitionedStream container)
        {
            Id = id;
            ContainerStartPosition = containerStartPosition;
            _container = container;
        }

        /// <summary>
        /// A unique identifier for this partition.
        /// </summary>
        public byte Id { get; }

        /// <summary>
        /// Gets a value indicating if the stream's partition has been marked for deletion.
        /// </summary>
        public bool IsDeleted { get; internal set; }

        /// <inheritdoc />
        public override bool CanRead => !IsDeleted;

        /// <inheritdoc />
        public override bool CanSeek => !IsDeleted;

        /// <inheritdoc />
        public override bool CanWrite => !IsDeleted && _container.Source.CanWrite;

        /// <inheritdoc />
        public override long Length => GetLengthRaw();

        /// <summary>
        /// Gets the position within the containing stream where this partition begins.
        /// </summary>
        /// <remarks>
        /// Since fragmented partitions are used, this is only updated when <see cref="Flush"/> or <see cref="FlushAsync"/> is called.
        /// </remarks>
        internal long ContainerStartPosition { get; set; }

        /// <inheritdoc />
        public override long Position { get; set; }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (IsDeleted)
                ThrowHelper.ThrowObjectDisposedException(nameof(StreamPartition));

            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - 1 - offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            return Position;
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            lock (_container.Source)
            {
                if (IsDeleted)
                    ThrowHelper.ThrowObjectDisposedException(nameof(StreamPartition));

                // If set to smaller, simply set the length and refuse to read past that point.
                // Extra data is cleaned up on flush.
                if (value < Length)
                {
                    // do nothing
                }

                var partitionMap = _container.TryGetRawPartitionMap();

                // If set to larger, pad with zeroes.
                if (value > Length)
                {
                    var bytes = Enumerable.Repeat((byte)0, (int)(value - Length)).ToArray();
                    WriteBytesToPartition(bytes, Length, partitionMap);
                }

                SetLengthRaw(value, partitionMap);
            }
        }

        private void SetLengthRaw(long value, byte[] partitionMap)
        {
            EnsureValidPartitionMap(partitionMap);

            var mapEntryStartingPosition =
                FindPartitionIndexInMap(partitionMap) * PartitionedStream.SinglePartitionMapEntrySize;

            BitConverter.GetBytes(value).CopyTo(partitionMap,
                mapEntryStartingPosition + PartitionedStream.PartitionLengthByteOffset);
        }

        private long GetLengthRaw(byte[]? partitionMap = null)
        {
            partitionMap ??= _container.TryGetRawPartitionMap();
            EnsureValidPartitionMap(partitionMap);

            var mapEntryStartingPosition =
                FindPartitionIndexInMap(partitionMap) * PartitionedStream.SinglePartitionMapEntrySize;

            return BitConverter.ToInt64(partitionMap,
                mapEntryStartingPosition + PartitionedStream.PartitionLengthByteOffset);
        }

        private void SetStateRaw(StreamPartitionState state, byte[] partitionMap)
        {
            EnsureValidPartitionMap(partitionMap);

            var mapEntryStartingPosition =
                FindPartitionIndexInMap(partitionMap) * PartitionedStream.SinglePartitionMapEntrySize;

            partitionMap[mapEntryStartingPosition + PartitionedStream.PartitionStateFlagsByteOffset] = (byte)state;
        }

        private StreamPartitionState GetStateRaw(byte[]? partitionMap = null)
        {
            partitionMap ??= _container.TryGetRawPartitionMap();
            EnsureValidPartitionMap(partitionMap);

            var mapEntryStartingPosition = FindPartitionIndexInMap(partitionMap) * PartitionedStream.SinglePartitionMapEntrySize;

            return (StreamPartitionState)partitionMap[mapEntryStartingPosition + PartitionedStream.PartitionStateFlagsByteOffset];
        }

        public override int ReadByte()
        {
            var buffer = new byte[1];
            ReadBytesFromPartition(buffer, bufferOffset: 0, partitionOffset: Position, count: 1);
            return buffer[0];
        }

        public override void WriteByte(byte value)
        {
            var buffer = new[] { value };
            WriteBytesToPartition(buffer, Position);
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadBytesFromPartition(buffer, bufferOffset: offset, partitionOffset: Position, count);

            return count;
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteBytesToPartition(buffer, partitionOffset: Position);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            throw new System.NotImplementedException();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            throw new System.NotImplementedException();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new System.NotImplementedException();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new System.NotImplementedException();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public override void Close()
        {
            throw new System.NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new System.NotImplementedException();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Writes the provided bytes to this partition at the given partitionOffset.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="bufferOffset">The position in <paramref name="buffer"/> where the first byte should be written.</param>
        /// <param name="partitionOffset">The partitionOffset to start reading from, as if the partition were a single congruent stream.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <param name="partitionMap">An existing partition map, if you already have it.</param>
        private void ReadBytesFromPartition(byte[] buffer, long bufferOffset, long partitionOffset, long count, byte[]? partitionMap = null)
        {
            Guard.IsGreaterThanOrEqualTo(0, partitionOffset, nameof(partitionOffset));

            partitionMap ??= _container.TryGetRawPartitionMap();
            EnsureValidPartitionMap(partitionMap);

            var partitionLength = GetLengthRaw(partitionMap);
            Guard.IsLessThanOrEqualTo(value: count, maximum: partitionLength);

            var bytesRead = 0L;

            // Find only the partitions needed to read the bytes.
            var fragments = GetFragmentsToFitLength(partitionOffset, ContainerStartPosition);

            // A properly yielded IEnumerable allows us to read the header data during enumeration.
            // That means stream position is only moved forward - never backward - for a sequential read.
            foreach (var fragment in fragments)
            {
                // Skip fragments until we arrive at the partitionOffset.
                // If position + length > partitionOffset, we've passed the starting position for our first byte.
                if (fragment.Position + fragment.Length < partitionOffset)
                    continue;

                // Skipping fragment header bytes to reach data
                var firstDataBytePositionForFragment = fragment.Position + FragmentHeaderLength;
                var lastBytePositionForFragment = fragment.Position + fragment.Length;
                var availableBytesInFragment = lastBytePositionForFragment - firstDataBytePositionForFragment;

                // Sanity checks
                Guard.IsLessThanOrEqualTo(value: availableBytesInFragment, maximum: partitionLength);
                Guard.IsLessThan(value: firstDataBytePositionForFragment, maximum: lastBytePositionForFragment);

                // Seek to start of data.
                _container.Source.Position = firstDataBytePositionForFragment;

                for (int i = 0; i < availableBytesInFragment; i++)
                {
                    var readByte = _container.Source.ReadByte();
                    
                    // Must not attempt to advance and read while at end of stream.
                    Guard.IsNotEqualTo(value: readByte, target: -1);

                    buffer[bufferOffset + ++bytesRead] = (byte)readByte;

                    Position++;
                }
            }

            // All requested bytes must have been read.
            Guard.IsEqualTo(value: bytesRead, target: count);
        }

        /// <summary>
        /// Writes the provided bytes to this partition at the given partitionOffset.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        /// <param name="partitionOffset">The partitionOffset to write <paramref name="bytes"/> to, as if the partition were a single congruent stream.</param>
        /// <param name="partitionMap">An existing partition map, if you already have it.</param>
        private void WriteBytesToPartition(byte[] bytes, long partitionOffset, byte[]? partitionMap = null)
        {
            Guard.IsGreaterThanOrEqualTo(0, partitionOffset, nameof(partitionOffset));

            partitionMap ??= _container.TryGetRawPartitionMap();
            EnsureValidPartitionMap(partitionMap);

            // GetLengthRaw allow us to reuse a known partition map without reading it again.
            var originalLength = GetLengthRaw(partitionMap);
            var bytesWritten = 0L;

            // Find only the partitions needed to write the bytes.
            // Discovery happens from the start of the stream, so pad with partitionOffset.
            var fragments = GetFragmentsToFitLength(bytes.Length + partitionOffset, ContainerStartPosition);
            FragmentData? lastKnownFragment = null;

            // Overwrite data in existing fragments
            // A properly yielded IEnumerable allows us to read the header data during enumeration.
            // That means stream position is only moved forward - never backward - for a sequential read.
            foreach (var fragment in fragments)
            {
                // Skip fragments until we arrive at the partitionOffset.
                // If position + length > partitionOffset, we've passed the starting position for our first byte.
                if (fragment.Position + fragment.Length < partitionOffset)
                    continue;

                // Skipping fragment header bytes to reach data
                var firstDataBytePositionForFragment = fragment.Position + FragmentHeaderLength;

                var lastBytePositionForFragment = fragment.Position + fragment.Length;
                var availableBytesInFragment = lastBytePositionForFragment - firstDataBytePositionForFragment;
                var remainingBytesToWrite = bytes.Length - bytesWritten;

                var isLastFragment = fragment.NextPosition == PartitionedStream.PartitionEndSentinel;
                var isLastPartition = lastBytePositionForFragment + partitionMap.Length >= _container.Source.Length;

                // Seek to start of data.
                _container.Source.Position = firstDataBytePositionForFragment;

                // If this is the very last fragment of the last partition in the source stream,
                // We can expand the size of the fragment and directly write all remaining bytes
                // without needing to move any data from other partitions.
                // Doing this will overwrite at least part of the partition map (minimum 1 byte)
                // It'll need to be added back at the end.
                if (isLastFragment && isLastPartition && remainingBytesToWrite > availableBytesInFragment)
                {
                    _container.Source.Write(bytes, (int)bytesWritten, (int)remainingBytesToWrite);
                    bytesWritten += remainingBytesToWrite;

                    // Update the length of the fragment
                    var newFragmentLength = remainingBytesToWrite;
                    var newFragmentLengthBytes = BitConverter.GetBytes(newFragmentLength);

                    _container.Source.Seek(firstDataBytePositionForFragment - 8, SeekOrigin.Begin);
                    _container.Source.Write(newFragmentLengthBytes, 0, 8);

                    // Update the length of the partition
                    var bytesWrittenPastEndOfFragment = newFragmentLength - fragment.Length;
                    var newPartitionLength = originalLength + bytesWrittenPastEndOfFragment;

                    SetLengthRaw(newPartitionLength, partitionMap);

                    // Write modified partition map to the end.
                    _container.Source.Write(partitionMap, (int)bytesWritten, partitionMap.Length);

                    // Since this is the last fragment in the last partition, we know we have no more data to write.
                    // Sanity check, then skip all other operations.
                    Guard.IsEqualTo(bytesWritten, bytes.Length, nameof(bytesWritten));
                    Guard.IsEqualTo(Length, originalLength + bytesWrittenPastEndOfFragment, nameof(Length));
                    return;
                }

                // If there's not enough space to write remaining bytes
                var remainingBytesForFragment = availableBytesInFragment < remainingBytesToWrite
                    ? remainingBytesToWrite - availableBytesInFragment // Limit to available space
                    : remainingBytesToWrite; // Write as many bytes as possible

                _container.Source.Write(bytes, (int)bytesWritten, (int)remainingBytesForFragment);
                bytesWritten += remainingBytesForFragment;

                // If we didn't overwrite all bytes in the fragment, partition is now smaller.
                // this will create unused junk data that needs to be cleaned up on defragment / before flush.
                if (remainingBytesToWrite < availableBytesInFragment)
                {
                    // Ensure we've written as many bytes as possible
                    Guard.IsEqualTo(bytesWritten, bytes.Length, nameof(bytesWritten));

                    var unwrittenByteCount = availableBytesInFragment - remainingBytesForFragment;
                    var newPartitionLength = originalLength - unwrittenByteCount;

                    SetLengthRaw(newPartitionLength, partitionMap);
                    _container.Source.Write(partitionMap, (int)bytesWritten, partitionMap.Length);

                    SavePartitionMap();
                }

                lastKnownFragment = fragment;
            }

            // If not all bytes have been written, create a new fragment for remaining data.
            if (bytesWritten < bytes.Length)
            {
                Guard.IsNotNull(lastKnownFragment, nameof(lastKnownFragment));
                Guard.IsEqualTo(lastKnownFragment.NextPosition, PartitionedStream.PartitionEndSentinel, nameof(lastKnownFragment.NextPosition));

                var startingPositionForNewFragment = _container.Source.Length - partitionMap.Length;
                var startingPositionForNewFragmentBytes = BitConverter.GetBytes(startingPositionForNewFragment);

                var remainingBytesToWrite = bytes.Length - bytesWritten;

                // Padded with 8 bytes for a pointer to the next fragment
                // and 8 bytes for the length of this fragment.
                var fragment = new byte[remainingBytesToWrite + 16];

                // Create new fragment with header data.
                PartitionedStream.PartitionEndSentinelBytes.CopyTo(fragment, 0);
                BitConverter.GetBytes(remainingBytesToWrite).CopyTo(fragment, 8);

                // Write remaining data to the fragment.
                bytes.CopyTo(fragment, bytesWritten);

                // Update the fragment pointer chain of the original last fragment.
                _container.Source.Position = lastKnownFragment.Position;
                _container.Source.Write(startingPositionForNewFragmentBytes, 0, startingPositionForNewFragmentBytes.Length);

                // Update the partition length and fragmentation state
                var newLength = originalLength + remainingBytesToWrite;
                SetLengthRaw(newLength, partitionMap);

                var state = GetStateRaw(partitionMap);
                SetStateRaw(state | StreamPartitionState.Fragmented, partitionMap);
                
                SavePartitionMap();
            }

            void SavePartitionMap()
            {
                // Write modified partition map to source.
                _container.Source.Write(partitionMap, (int)_container.Source.Length - partitionMap.Length, partitionMap.Length);
            }
        }

        private IEnumerable<FragmentData> GetFragmentsToFitLength(long length, long partitionStartPosition)
        {
            var currentPositionPointer = partitionStartPosition;
            var partitionLengthTraveled = 0L;

            while (partitionLengthTraveled < length)
            {
                // Each fragment begins with
                // 8 bytes for a long (int64) pointer to the partition's next fragment.
                // 8 bytes for a long (int64) length of the fragment's data.
                _container.Source.Seek(currentPositionPointer, SeekOrigin.Begin);

                var nextFragmentPosAndLengthBytes = new byte[16];
                _container.Source.Read(nextFragmentPosAndLengthBytes, 0, nextFragmentPosAndLengthBytes.Length);

                var nextFragmentPosition = BitConverter.ToInt64(nextFragmentPosAndLengthBytes, 0);
                var fragmentLength = BitConverter.ToInt64(nextFragmentPosAndLengthBytes, 8);

                var fragment = new FragmentData(currentPositionPointer, nextFragmentPosition, fragmentLength);

                // Sanity checks
                Guard.IsGreaterThan(value: fragment.Length, minimum: 0);
                Guard.IsGreaterThan(value: fragment.Position, minimum: 0);
                Guard.IsGreaterThan(value: fragment.NextPosition, minimum: 0);

                Guard.IsLessThan(value: fragment.Length, maximum: _container.Source.Length);
                Guard.IsLessThan(value: fragment.Position, maximum: _container.Source.Length);
                Guard.IsLessThan(value: fragment.NextPosition, maximum: _container.Source.Length);

                yield return fragment;
                partitionLengthTraveled += fragment.Length;
                currentPositionPointer = nextFragmentPosition;
            }
        }

        /// <summary>
        /// Returns the 0-indexed position of the partition in the partition map.
        /// For example, if this is the 2nd partition, 1 is returned. If not found, -1 is returned.
        /// </summary>
        private int FindPartitionIndexInMap(byte[]? partitionMap = null)
        {
            partitionMap ??= _container.TryGetRawPartitionMap();
            EnsureValidPartitionMap(partitionMap);

            // Find the index of this partition's data in the map
            for (var i = 0; i < partitionMap.Length; i += PartitionedStream.SinglePartitionMapEntrySize)
                if (partitionMap[i] + PartitionedStream.PartitionIdByteOffset == Id)
                    return i;

            return -1;
        }

        private static void EnsureValidPartitionMap(byte[] partitionMap)
        {
            Guard.IsNotNull(partitionMap, nameof(partitionMap));

            if (partitionMap.Length == 0)
                ThrowHelper.ThrowInvalidDataException("Partition map was unexpectedly empty");

            if (partitionMap.Length < 20)
                ThrowHelper.ThrowInvalidDataException("Partition map is too small to be valid.");
        }

        private record FragmentData(long Position, long NextPosition, long Length);
    }
}