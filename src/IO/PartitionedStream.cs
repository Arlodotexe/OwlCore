using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;

namespace OwlCore.IO;

/// <summary>
/// A <see cref="PartitionedStream"/> allows for partitioning a given stream of data into multiple Streams, which can be passed around to standard APIs and operated on without loading the entire dataset into memory.
/// </summary>
public class PartitionedStream
{
    // Each partition entry in the map is 18 bytes
    // 1 byte for an identifier.
    // 8 bytes for a long (int64) pointer to the partition's first byte.
    // 8 bytes for a long (int64) length of the partition
    // 1 byte for state flags
    internal const byte PartitionIdByteOffset = 0;
    internal const byte PartitionPointerByteOffset = 1;
    internal const byte PartitionLengthByteOffset = 9;
    internal const byte PartitionStateFlagsByteOffset = 17;
    internal const byte SinglePartitionMapEntrySize = 18;

    /// <summary>
    /// Creates a new instance of <see cref="PartitionedStream"/>.
    /// </summary>
    /// <param name="source">A stream that can be used to store multiple partitions. Reading, writing and seeking must be enabled.</param>
    public PartitionedStream(Stream source)
    {
        Guard.IsTrue(source.CanRead, nameof(source.CanRead));
        Guard.IsTrue(source.CanSeek, nameof(source.CanSeek));
        Source = Stream.Synchronized(source);
    }

    /// <summary>
    /// A sequence of bytes that indicates the end of a partition.
    /// </summary>
    internal static byte[] PartitionEndSentinelBytes { get; } = BitConverter.GetBytes(PartitionEndSentinel);

    /// <summary>
    /// A 64-bit integer that indicates the end of a partition.
    /// </summary>
    internal static long PartitionEndSentinel { get; } = -1L;

    /// <summary>
    /// The underlying stream where partitions are stored.
    /// </summary>
    public Stream Source { get; }

    /// <summary>
    /// Discovers and returns all existing partitions in the source stream.
    /// </summary>
    /// <returns>A collection of all found partitions.</returns>
    public IEnumerable<StreamPartition> GetAllPartitions() => GetExistingPartitionData(this);

    /// <summary>
    /// Creates and appends a new, empty partition to the end of the source stream.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. Value is the created partition.</returns>
    public StreamPartition CreatePartition(byte id)
    {
        if (!Source.CanWrite)
            ThrowHelper.ThrowInvalidDataException(
                "Underlying stream does not support writing. Cannot create partition.");

        lock (Source)
        {
            // TODO Check if ID is already in use. Collision options?

            // Clone existing partition map (padded with space for new partition)
            var partitionMap = TryGetRawPartitionMap(Source);
            var newPartitionMap = new byte[partitionMap.Length + SinglePartitionMapEntrySize];
            partitionMap.CopyTo(newPartitionMap, 0);

            // Insert new partition data into map
            newPartitionMap[partitionMap.Length + PartitionIdByteOffset] = id;
            newPartitionMap[partitionMap.Length + PartitionStateFlagsByteOffset] = (byte) StreamPartitionState.None;

            BitConverter.GetBytes(0L).CopyTo(newPartitionMap, partitionMap.Length + PartitionLengthByteOffset);
            PartitionEndSentinelBytes.CopyTo(newPartitionMap, partitionMap.Length + PartitionPointerByteOffset);

            // Write modified partition map to source.
            Source.Write(newPartitionMap, (int) Source.Length - partitionMap.Length, newPartitionMap.Length);

            return new StreamPartition(id, Source.Position, this);
        }
    }

    /// <summary>
    /// Marks a partition for deletion in the source stream.
    /// Data is fully removed when the source stream is flushed to the underlying device.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. Value is the created partition.</returns>
    public void DeletePartition(StreamPartition partition)
    {
        if (!Source.CanWrite)
            ThrowHelper.ThrowInvalidDataException(
                "Underlying stream does not support writing. Cannot delete partition.");

        lock (Source)
        {
            var partitionMap = TryGetRawPartitionMap(Source);
            var indexInPartitionMap = FindMapIndex(Source, partition.Id, partitionMap);
            if (indexInPartitionMap == -1)
                ThrowHelper.ThrowInvalidDataException("Provided partition could not be found.");

            // Feature flag byte indicates deletion.
            var featureFlagsByte = partitionMap[indexInPartitionMap + PartitionStateFlagsByteOffset];
            var newFeatureFlagValue = (StreamPartitionState) featureFlagsByte | StreamPartitionState.Deleted;

            partitionMap[indexInPartitionMap + PartitionStateFlagsByteOffset] = (byte) newFeatureFlagValue;

            // Write modified partition map to source.
            Source.Write(partitionMap, (int) Source.Length - partitionMap.Length, partitionMap.Length);

            partition.IsDeleted = true;
        }
    }

    /// <summary>
    /// Unmarks a partition for deletion in the source stream.
    /// Data is fully removed when the source stream is flushed to the underlying device.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. Value is the created partition.</returns>
    public void RestorePartition(StreamPartition partition)
    {
        Guard.IsEqualTo(true, partition.IsDeleted, nameof(partition.IsDeleted));

        lock (Source)
        {
            var partitionMap = TryGetRawPartitionMap(Source);
            var indexInPartitionMap = FindMapIndex(Source, partition.Id, partitionMap);

            // Feature flag byte indicates deletion.
            var featureFlagsByte = partitionMap[indexInPartitionMap + PartitionStateFlagsByteOffset];
            var newFeatureFlagValue = (StreamPartitionState) featureFlagsByte ^ StreamPartitionState.Deleted;

            partitionMap[indexInPartitionMap + PartitionStateFlagsByteOffset] = (byte) newFeatureFlagValue;

            // Write modified partition map to source.
            Source.Write(partitionMap, (int) Source.Length - partitionMap.Length, partitionMap.Length);

            partition.IsDeleted = false;
        }
    }

    /// <summary>
    /// Fully deletes a partition from the source stream.
    /// WIP.
    /// </summary>
    private void HardRemovePartition(StreamPartition partition)
    {
        // Clone existing partition map
        var partitionMap = TryGetRawPartitionMap(Source);
        var newPartitionMap = new byte[partitionMap.Length - SinglePartitionMapEntrySize];

        // Clone existing partition map
        if (partitionMap.Length == 0)
            throw new InvalidDataException("Partition map was unexpectedly empty.");

        var found = false;

        // Recreate partition map without this partition's data.
        for (var i = 0; i < partitionMap.Length; i += SinglePartitionMapEntrySize)
        {
            // Skip the data for the removed partition.
            if (partitionMap[i] + PartitionIdByteOffset == partition.Id)
            {
                found = true;
                continue;
            }

            newPartitionMap[i] = partitionMap[i];
        }

        if (!found)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(partition.Id), "The provided partition ID was not found.");
        }

        // Remove old partition map entirely.
        Source.SetLength(Source.Length - partitionMap.Length);

        // Insert new partition map.
        Source.Seek(0, SeekOrigin.End);
        Source.Write(newPartitionMap, 0, newPartitionMap.Length);

        // Defragmenting first allows us to sequentially remove all bytes for this partition.
        // Much easier than deleting each fragment.
        Defragment();

        // Shift all data after this partition backwards to replace this partition
        Source.Position = partition.ContainerStartPosition;

        // TODO
    }

    /// <summary>
    /// Defragments all partitions, ensuring that all data is sequential.
    /// </summary>
    public void Defragment()
    {
    }

    internal static int FindMapIndex(Stream source, byte id, byte[]? partitionMap = null)
    {
        partitionMap ??= TryGetRawPartitionMap(source);
        if (partitionMap.Length == 0)
            throw new InvalidDataException("Partition map was unexpectedly empty.");

        // Find the index of this partition's data in the map
        for (var i = 0; i < partitionMap.Length; i += SinglePartitionMapEntrySize)
            if (partitionMap[i] + PartitionIdByteOffset == id)
                return i;

        return -1;
    }

    /// <summary>
    /// Reads the provided <param name="container"/> for the partition map and returns the positions and IDs of all known partitions.
    /// </summary>
    private static IEnumerable<StreamPartition> GetExistingPartitionData(PartitionedStream container)
    {
        var partitionMap = TryGetRawPartitionMap(container.Source);
        if (partitionMap.Length == 0)
            yield break;

        for (var i = 0; i < partitionMap.Length; i += SinglePartitionMapEntrySize)
        {
            var partitionIdOffset = i + PartitionIdByteOffset;
            var startPositionOffset = i + PartitionPointerByteOffset;

            var startPosition = BitConverter.ToInt64(partitionMap, startPositionOffset);

            // Partitions are stored in the map in the same order as partitions live in the stream
            // First partition data == first partition.
            yield return new StreamPartition(partitionMap[partitionIdOffset], startPosition, container);
        }
    }

    internal void SetRawPartitionData(byte id, byte[] newPartitionData, byte[]? partitionMap = null)
        => SetRawPartitionData(Source, id, newPartitionData, partitionMap);

    internal static void SetRawPartitionData(Stream source, byte id, byte[] newPartitionData,
        byte[]? partitionMap = null)
    {
        partitionMap ??= TryGetRawPartitionMap(source);
        if (partitionMap.Length == 0)
            throw new InvalidDataException("Partition map was unexpectedly empty.");

        var mapIndex = FindMapIndex(source, id, partitionMap);
        Guard.IsNotEqualTo(-1, mapIndex, nameof(mapIndex));

        // Copy new partition data to the map
        newPartitionData.CopyTo(partitionMap, mapIndex * SinglePartitionMapEntrySize);

        // Write new partition map to source.
        source.Write(partitionMap, (int) source.Length - partitionMap.Length, partitionMap.Length);
    }

    internal byte[] TryGetRawPartitionMap() => TryGetRawPartitionMap(Source);

    internal static byte[] TryGetRawPartitionMap(Stream stream)
    {
        // A valid, defragmented partitioned stream has a minimum of 20 bytes of data
        // 1 byte for a partition identifier.
        // 8 bytes for the starting position of 1 partition
        // 8 bytes for the length of the partition
        // 1 byte for state flags
        // 1 byte for actual data.
        // 1 byte for map size
        // If fragmented,
        // + 8 bytes for the length of the fragment
        // + 8 bytes for the position of the next fragment
        if (stream.Length < 20)
            return Array.Empty<byte>();

        // Partition map lives at the end of the stream.
        // The map is frequently modified, so this avoids the need
        // to shift all other data just to modify the map.
        stream.Position = stream.Length - 1;

        // Actual map size is the last byte in the stream.
        stream.Seek(-1, SeekOrigin.End);

        // Seek to start of map
        var actualMapSize = stream.ReadByte();
        stream.Seek(actualMapSize * -1, SeekOrigin.End);

        // Read map
        var mapChunk = new byte[actualMapSize];
        var bytesRead = stream.Read(mapChunk, 0, actualMapSize);
        Guard.IsEqualTo(bytesRead, actualMapSize, nameof(bytesRead));

        // Ensure map can be evenly split into valid partitions.
        Guard.IsEqualTo(0, actualMapSize % SinglePartitionMapEntrySize, nameof(actualMapSize));
        return mapChunk;
    }


    private record PartitionData(byte Id, long StartPosition);
}