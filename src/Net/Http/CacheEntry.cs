using System;

namespace OwlCore.Net.Http
{
    /// <summary>
    /// A class to hold and save cached data.
    /// </summary>
    public class CacheEntry
    {
        /// <summary>
        /// The cached response object.
        /// </summary>
        public string? RequestUri { get; set; }

        /// <summary>
        /// The http response content.
        /// </summary>
        public byte[]? ContentBytes { get; set; }

        /// <summary>
        /// Timestamp for the cache.
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }
}