using CommunityToolkit.Diagnostics;
using OwlCore.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using OwlCore.Storage.SystemIO;

namespace OwlCore.Net.Http
{
    /// <summary>
    /// An <see cref="HttpClientHandler"/> that caches requests and returns them when specified (with optional cache filtering).
    /// </summary>
    /// <remarks>
    /// This is a <see cref="DelegatingHandler"/>. By default, the <see cref="DelegatingHandler.InnerHandler"/> property is an <see cref="HttpClientHandler"/> so it can handle HTTP requests without further config.
    /// <para/>
    /// You can assign a different <c>InnerHandler</c>, including other <see cref="DelegatingHandler"/>s, to chain handlers together.
    /// </remarks>
    public class CachedHttpClientHandler : DelegatingHandler
    {
        private readonly IModifiableFolder _cacheFolder;
        private readonly TimeSpan _defaultCacheTime;

        /// <summary>
        /// Creates an instance of the <see cref="CachedHttpClientHandler"/>.
        /// </summary>
        public CachedHttpClientHandler(string cacheFolderPath, TimeSpan defaultCacheTime)
        {
            var path = Path.GetFullPath(cacheFolderPath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            _cacheFolder = new SystemFolder(cacheFolderPath);
            _defaultCacheTime = defaultCacheTime;

            InnerHandler = new HttpClientHandler();
        }

        /// <summary>
        /// Raised when cache is found and is about to be used.
        /// </summary>
        public event EventHandler<CachedRequestEventArgs>? CachedRequestFound;

        /// <summary>
        /// Raised when new data is retrieved and is about to be saved.
        /// </summary>
        public event EventHandler<CachedRequestEventArgs>? CachedRequestSaving;

        /// <inheritdoc cref="HttpClientHandler.SendAsync(HttpRequestMessage, CancellationToken)"/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // check if item is cached
            var cachedData = ReadCachedFile(path, request.RequestUri.AbsoluteUri);

            var shouldUseCache = true;
            if (cachedData != null)
            {
                var requestFoundEventArgs = new CachedRequestEventArgs(request.RequestUri, cachedData);
                CachedRequestFound?.Invoke(this, requestFoundEventArgs);

                shouldUseCache = !requestFoundEventArgs.Handled;
            }

            if (cachedData != null && shouldUseCache)
            {
                // if cache found and not expired
                if (cachedData.TimeStamp + _defaultCacheTime > DateTime.UtcNow && cachedData.ContentBytes != null)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ByteArrayContent(cachedData.ContentBytes);

                    return response;
                }

                // If expired, remove entry.
                var cachedFilePath = GetCachedFilePath(path, request.RequestUri.AbsoluteUri);

                try
                {
                    File.Delete(cachedFilePath);
                }
                catch
                {
                    // ignored
                }
            }

            var result = await base.SendAsync(request, cancellationToken);
            var freshCacheData = await CreateCachedData(request.RequestUri.AbsoluteUri, result);

            var shouldSaveEventArgs = new CachedRequestEventArgs(request.RequestUri, freshCacheData);
            CachedRequestSaving?.Invoke(this, shouldSaveEventArgs);

            if (!shouldSaveEventArgs.Handled)
                WriteCachedFile(path, freshCacheData);

            return result;
        }

        /// <summary>
        /// Creates cached data.
        /// </summary>
        /// <param name="request">API request information.</param>
        /// <param name="response">The response string to be cached.</param>
        /// <returns>Returns a <see cref="Task" /> that represents the asynchronous operation.</returns>
        public static async Task<CacheEntry> CreateCachedData(string request, HttpResponseMessage response)
        {
            var contentBytes = await response.Content.ReadAsByteArrayAsync();

            return new CacheEntry
            {
                ContentBytes = contentBytes,
                RequestUri = request,
                TimeStamp = DateTime.UtcNow,
            };
        }

        /// <summary>
        /// Writes cache to the file.
        /// </summary>
        /// <param name="path">Path to cache file.</param>
        /// <param name="cacheEntry">The cache data to write to disk.</param>
        /// <returns>Returns a <see cref="Task" /> that represents the asynchronous operation.</returns>
        public static void WriteCachedFile(string path, CacheEntry cacheEntry)
        {
            Guard.IsNotNull(cacheEntry.RequestUri, nameof(cacheEntry.RequestUri));

            var cachedFilePath = GetCachedFilePath(path, cacheEntry.RequestUri);

            var serializedData = JsonSerializer.Serialize(cacheEntry);

            File.WriteAllText(cachedFilePath, serializedData);
        }

        /// <summary>
        /// Read cache data.
        /// </summary>
        /// <param name="file">The file to read</param>
        /// <param name="request">API request information</param>
        /// <returns>Information related to cache in a <see cref="CacheEntry"/></returns>
        private static CacheEntry? ReadCachedFile(IFile file, string request)
        {
            CacheEntry? cacheEntry = null;
            bool fileExists = false;
            try
            {
                fileExists = File.Exists(cachedFilePath);

                var fileBytes = File.ReadAllText(cachedFilePath);
                cacheEntry = JsonSerializer.Deserialize<CacheEntry>(fileBytes);
            }
            catch (Exception ex)
            {
                if (fileExists)
                    Debug.WriteLine($"WARNING: Failed to read or deserialized the file at \"{cachedFilePath}\". The data will be discarded. ({ex})");
            }

            if (cacheEntry?.RequestUri is null)
                return null;

            // Check if the cached request matches the given (could be a hash collision).
            if (!request.Contains(cacheEntry.RequestUri))
                return null;

            return cacheEntry;
        }

        /// <summary>
        /// Generates a file for the cache.
        /// </summary>
        /// <param name="basePath">Path to the directory where the file is stored.</param>
        /// <param name="requestUri">The request uri.</param>
        /// <returns>The file path.</returns>
        private static string GetCachedFilePath(string basePath, string requestUri)
        {
            return Path.Combine(basePath, requestUri.HashMD5Fast()) + ".cache";
        }
    }
}