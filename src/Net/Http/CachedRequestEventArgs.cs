using System;

namespace OwlCore.Net.Http
{
    /// <summary>
    /// <see cref="EventArgs"/> used to handle if a request should be saved to disk or used in <see cref="CachedHttpClientHandler"/>.
    /// </summary>
    public class CachedRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="CachedRequestEventArgs"/>.
        /// </summary>
        public CachedRequestEventArgs(Uri requestUri, CacheEntry cacheEntry)
        {
            RequestUri = requestUri;
            CacheEntry = cacheEntry;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event was handled.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// The uri of the current request.
        /// </summary>
        public Uri RequestUri { get; set; }

        /// <summary>
        /// The cached data, if present.
        /// </summary>
        public CacheEntry CacheEntry { get; set; }
    }
}