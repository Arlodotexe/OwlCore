using System;

namespace OwlCore.IO.Streams
{
    /// <summary>
    /// Indicates the various possible states for a <see cref="StreamPartition"/>.
    /// </summary>
    [Flags]
    public enum StreamPartitionState : byte
    {
        None = 0,
        Deleted = 1,
        Fragmented = 2,
    }
}